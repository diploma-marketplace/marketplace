using FutureShopBackend.Application;
using FutureShopBackend.Infrastructure;

namespace FutureShopBackend.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppDI(this IServiceCollection services)
        {
            services
                .AddApplicationDI()
                .AddInfrastructureDI();
            return services;
        }
    }
}
