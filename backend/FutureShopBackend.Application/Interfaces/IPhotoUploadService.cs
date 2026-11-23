using Microsoft.AspNetCore.Http;

namespace FutureShopBackend.Application.Interfaces;

/// <summary>
/// Service contract for validating and storing uploaded product photos.
/// </summary>
public interface IPhotoUploadService
{
    /// <summary>
    /// Validates an image file by size, extension, MIME type, and magic bytes header.
    /// </summary>
    void ValidatePhoto(IFormFile file);

    /// <summary>
    /// Validates and saves a single photo file to local storage, returning its relative URL path.
    /// </summary>
    Task<string> UploadPhotoAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and saves a batch of photo files to local storage, returning their relative URL paths.
    /// </summary>
    Task<IReadOnlyList<string>> UploadPhotosAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken = default);
}
