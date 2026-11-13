using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Exceptions;

namespace FutureShopBackend.Api.Middleware;

/// <summary>
/// Global exception handling middleware that maps application and unhandled exceptions
/// into the standardized <see cref="ApiErrorResponse"/> JSON contract (400, 401, 403, 404, 500).
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        ApiErrorResponse errorResponse;
        int statusCode;

        if (exception is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            errorResponse = new ApiErrorResponse
            {
                Status = appEx.StatusCode,
                Code = appEx.Code,
                Message = appEx.Message,
                Errors = appEx.Errors,
                TraceId = traceId,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        else
        {
            logger.LogError(exception, "Unhandled exception occurred while processing {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);

            statusCode = StatusCodes.Status500InternalServerError;
            errorResponse = new ApiErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Code = "INTERNAL_SERVER_ERROR",
                Message = "An unexpected server error occurred.",
                TraceId = traceId,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }
}
