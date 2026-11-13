using System.Reflection;
using System.Text.Json.Serialization;
using FutureShopBackend.Application;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure;
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
    }
}
