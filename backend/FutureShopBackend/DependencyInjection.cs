using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using FutureShopBackend.Application;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace FutureShopBackend.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services)
        {
            services
                .AddApplicationDI()
                .AddInfrastructureDI();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var validationErrors = context.ModelState
                            .Where(entry => entry.Value?.Errors.Count > 0)
                            .ToDictionary(
                                entry => JsonNamingPolicy_CamelCase(entry.Key),
                                entry => entry.Value!.Errors
                                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                                    .ToArray());

                        var errorResponse = new ApiErrorResponse
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Code = "VALIDATION_ERROR",
                            Message = "One or more validation errors occurred.",
                            Errors = validationErrors,
                            TraceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier,
                            Timestamp = DateTimeOffset.UtcNow
                        };

                        return new BadRequestObjectResult(errorResponse);
                    };
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Future Market API",
                    Version = "v1",
                    Description =
                        "RESTful API contract between the Backend (ASP.NET Core) and Frontend (React + TypeScript) for the Future Market platform.\n\n" +
                        "**Authentication:** Session Cookie (`FutureMarket.Session`).\n" +
                        "**RBAC Roles:** `User` (buyer / individual seller / sole proprietor), `Company` (verified corporate seller), `Admin` (platform administration).\n" +
                        "**Error Handling:** All error responses (`400`, `401`, `403`, `404`, `500`) use the standardized `ApiErrorResponse` schema."
                });

                options.AddSecurityDefinition("SessionCookie", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Name = "FutureMarket.Session",
                    Description = "HTTP-only session authentication cookie issued upon login or registration."
                });

                var xmlAssemblies = new[]
                {
                    Assembly.GetExecutingAssembly(),
                    typeof(ApiErrorResponse).Assembly,
                    typeof(UserRole).Assembly
                };

                foreach (var assembly in xmlAssemblies)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                    }
                }
            });

            return services;
        }

        private static string JsonNamingPolicy_CamelCase(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }

            var parts = key.Split('.');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToLowerInvariant(parts[i][0]) + parts[i][1..];
                }
            }

            return string.Join('.', parts);
        }
    }
}
