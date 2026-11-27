using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FutureShopBackend.Infrastructure.Services;

/// <summary>
/// Implements validation and local storage for uploaded product photos.
/// Enforces limits on file size, extensions, MIME types, and binary file signatures (magic bytes).
/// </summary>
public sealed class PhotoUploadService(IWebHostEnvironment? webHostEnvironment = null) : IPhotoUploadService
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 Megabytes
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/pjpeg",
        "image/x-png"
    };

    public void ValidatePhoto(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("No image file provided or file is empty.", "EMPTY_FILE");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            var sizeMb = file.Length / (1024.0 * 1024.0);
            throw new BadRequestException(
                $"File '{file.FileName}' exceeds the maximum allowed size of 5 MB (provided size: {sizeMb:F2} MB).",
                "IMAGE_SIZE_EXCEEDED");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException(
                $"File format '{extension}' is not supported. Allowed formats: {string.Join(", ", AllowedExtensions)}.",
                "INVALID_IMAGE_FORMAT");
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !AllowedMimeTypes.Contains(file.ContentType))
        {
            throw new BadRequestException(
                $"Content-Type '{file.ContentType}' is not an accepted image MIME type.",
                "INVALID_IMAGE_MIME_TYPE");
        }

        // Validate file header (magic bytes) to prevent spoofing
        ValidateMagicBytes(file, extension);
    }

    private static void ValidateMagicBytes(IFormFile file, string extension)
    {
        using var stream = file.OpenReadStream();
        Span<byte> header = stackalloc byte[16];
        var bytesRead = stream.Read(header);

        if (bytesRead < 4)
        {
            throw new BadRequestException("Uploaded file header is corrupted or incomplete.", "INVALID_IMAGE_SIGNATURE");
        }

        var extLower = extension.ToLowerInvariant();
        var isValid = extLower switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 &&
                      header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                      header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            ".webp" => bytesRead >= 12 &&
                       header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 && // RIFF
                       header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50, // WEBP
            _ => false
        };

        if (!isValid)
        {
            throw new BadRequestException(
                $"File content does not match the expected image signature for '{extension}'.",
                "INVALID_IMAGE_SIGNATURE");
        }
    }

    public async Task<string> UploadPhotoAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidatePhoto(file);

        var webRoot = webHostEnvironment?.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads", "products");

        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsDir, fileName);

        await using var targetStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await file.CopyToAsync(targetStream, cancellationToken);

        return $"/uploads/products/{fileName}";
    }

    public async Task<IReadOnlyList<string>> UploadPhotosAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken = default)
    {
        var fileList = files?.ToList() ?? [];
        if (fileList.Count == 0)
        {
            throw new BadRequestException("At least one image file is required.", "EMPTY_FILE_LIST");
        }

        if (fileList.Count > 10)
        {
            throw new BadRequestException("Cannot upload more than 10 images at once.", "MAX_IMAGE_COUNT_EXCEEDED");
        }

        // Validate all files first before writing any to disk
        foreach (var file in fileList)
        {
            ValidatePhoto(file);
        }

        var uploadedUrls = new List<string>(fileList.Count);
        foreach (var file in fileList)
        {
            var url = await UploadPhotoAsync(file, cancellationToken);
            uploadedUrls.Add(url);
        }

        return uploadedUrls;
    }
}
