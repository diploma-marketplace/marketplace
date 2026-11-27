using System.Text;
using FluentAssertions;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FutureShopBackend.Tests.Uploads;

public class PhotoUploadValidationTests
{
    private readonly PhotoUploadService _service = new(null);

    private static IFormFile CreateMockFile(string fileName, string contentType, byte[] content)
    {
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(content);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        return fileMock.Object;
    }

    [Fact]
    public void ValidatePhoto_WhenFileIsEmpty_ThrowsBadRequestException()
    {
        var emptyFile = CreateMockFile("test.jpg", "image/jpeg", []);

        var act = () => _service.ValidatePhoto(emptyFile);

        var ex = act.Should().Throw<BadRequestException>().Which;
        ex.Code.Should().Be("EMPTY_FILE");
    }

    [Fact]
    public void ValidatePhoto_WhenFileSizeExceeds5Mb_ThrowsBadRequestException()
    {
        var largeContent = new byte[PhotoUploadService.MaxFileSizeBytes + 10];
        // Give valid JPEG header
        largeContent[0] = 0xFF;
        largeContent[1] = 0xD8;
        largeContent[2] = 0xFF;

        var largeFile = CreateMockFile("large.jpg", "image/jpeg", largeContent);

        var act = () => _service.ValidatePhoto(largeFile);

        var ex = act.Should().Throw<BadRequestException>().Which;
        ex.Code.Should().Be("IMAGE_SIZE_EXCEEDED");
    }

    [Theory]
    [InlineData("script.exe")]
    [InlineData("page.html")]
    [InlineData("document.pdf")]
    [InlineData("image.svg")]
    [InlineData("vector.ai")]
    public void ValidatePhoto_WhenExtensionDisallowed_ThrowsBadRequestException(string fileName)
    {
        var content = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var file = CreateMockFile(fileName, "application/octet-stream", content);

        var act = () => _service.ValidatePhoto(file);

        var ex = act.Should().Throw<BadRequestException>().Which;
        ex.Code.Should().Be("INVALID_IMAGE_FORMAT");
    }

    [Fact]
    public void ValidatePhoto_WhenInvalidMagicBytes_ThrowsBadRequestException()
    {
        // Disguised .jpg that is actually plain text
        var fakeJpg = Encoding.UTF8.GetBytes("MZ_NOT_A_JPEG_FILE");
        var file = CreateMockFile("fake.jpg", "image/jpeg", fakeJpg);

        var act = () => _service.ValidatePhoto(file);

        var ex = act.Should().Throw<BadRequestException>().Which;
        ex.Code.Should().Be("INVALID_IMAGE_SIGNATURE");
    }

    [Fact]
    public void ValidatePhoto_WhenValidJpegHeader_Succeeds()
    {
        var jpegContent = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        var file = CreateMockFile("valid.jpg", "image/jpeg", jpegContent);

        var act = () => _service.ValidatePhoto(file);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePhoto_WhenValidPngHeader_Succeeds()
    {
        var pngContent = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
        var file = CreateMockFile("valid.png", "image/png", pngContent);

        var act = () => _service.ValidatePhoto(file);

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidatePhoto_WhenValidWebpHeader_Succeeds()
    {
        // RIFF....WEBP
        var webpContent = new byte[] {
            0x52, 0x49, 0x46, 0x46, // RIFF
            0x24, 0x00, 0x00, 0x00, // size
            0x57, 0x45, 0x42, 0x50, // WEBP
            0x56, 0x50, 0x38, 0x20  // VP8
        };
        var file = CreateMockFile("valid.webp", "image/webp", webpContent);

        var act = () => _service.ValidatePhoto(file);

        act.Should().NotThrow();
    }
}
