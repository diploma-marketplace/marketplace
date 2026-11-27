using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Infrastructure.Auth;
using FutureShopBackend.Infrastructure.Persistence;
using FutureShopBackend.Infrastructure.Services;
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

            // Use bcrypt instead of default PBKDF2 (registered after Identity so it overrides the default hasher)
            services.AddScoped<IPasswordHasher<ApplicationUser>, BcryptPasswordHasher>();

            // Redis-backed session ticket store (the session cookie stores only the opaque session key)
            services.AddStackExchangeRedisCache(o =>
                o.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379");
            services.AddSingleton<RedisTicketStore>();

            // Current user context accessor for RBAC and resource ownership verification
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, ProductOwnerAuthorizationHandler>();
            services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, DirectOwnerAuthorizationHandler>();

            // User and public seller profile services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<IUserService>(sp => sp.GetRequiredService<UserService>());
            services.AddScoped<ISellerService>(sp => sp.GetRequiredService<UserService>());

            // Catalog, search, filtering, and category services
            services.AddScoped<IPhotoUploadService, PhotoUploadService>();
            services.AddScoped<CatalogService>();
            services.AddScoped<IProductService>(sp => sp.GetRequiredService<CatalogService>());
            services.AddScoped<ICategoryService>(sp => sp.GetRequiredService<CatalogService>());

            // Shopping cart (grouped by seller), wishlist, and order checkout services
            services.AddScoped<CommerceService>();
            services.AddScoped<ICartService>(sp => sp.GetRequiredService<CommerceService>());
            services.AddScoped<IWishlistService>(sp => sp.GetRequiredService<CommerceService>());
            services.AddScoped<IOrderService>(sp => sp.GetRequiredService<CommerceService>());

            // Reviews and automatic product/seller rating recalculation service
            services.AddScoped<IReviewService, ReviewService>();

            return services;
        }
    }
}