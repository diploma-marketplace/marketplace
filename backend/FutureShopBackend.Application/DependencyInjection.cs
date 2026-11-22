using FutureShopBackend.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FutureShopBackend.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, Services.AuthService>();

            return services;
        }
    }
}