using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Infrastructure.Persistence;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FutureShopBackend.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(o =>
                o.UseNpgsql(configuration.GetConnectionString("Default"))
                 .UseSnakeCaseNamingConvention());

            services.AddIdentityCore<ApplicationUser>(o => o.User.RequireUniqueEmail = true)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services;
        }
    }
}