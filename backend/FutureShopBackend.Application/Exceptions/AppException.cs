namespace FutureShopBackend.Application.Exceptions;

/// <summary>
/// Base application exception mapped to a standardized HTTP error response.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    protected AppException(
        int statusCode,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        Errors = errors;
    }
}

public sealed class BadRequestException(
    string message,
    string code = "BAD_REQUEST",
    IReadOnlyDictionary<string, string[]>? errors = null)
    : AppException(400, code, message, errors);

public sealed class UnauthorizedException(
    string message = "Authentication is required.",
    string code = "UNAUTHORIZED")
    : AppException(401, code, message);

public sealed class ForbiddenException(
    string message = "You do not have permission to perform this action.",
    string code = "FORBIDDEN")
    : AppException(403, code, message);

public sealed class NotFoundException(
    string message = "The requested resource was not found.",
    string code = "NOT_FOUND")
    : AppException(404, code, message);

public sealed class ConflictException(
    string message,
    string code = "CONFLICT")
    : AppException(409, code, message);
