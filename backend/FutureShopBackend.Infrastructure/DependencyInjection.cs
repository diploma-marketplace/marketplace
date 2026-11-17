using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Infrastructure.Auth;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
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

            // bcrypt замість стандартного PBKDF2 (реєструється після Identity, тому перекриває його)
            services.AddScoped<IPasswordHasher<ApplicationUser>, BcryptPasswordHasher>();

            // Redis -сховище сесій (у cookie лише ключ сесії)
            services.AddStackExchangeRedisCache(o =>
                o.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379");
            services.AddSingleton<RedisTicketStore>();

            // Current user context accessor for RBAC and resource ownership verification
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}