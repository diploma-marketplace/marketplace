using System.Reflection;
using FutureShopBackend.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FutureShopBackend.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDI(this IServiceCollection services)
        {
            RegisterContractProxy<IAuthService>(services);
            RegisterContractProxy<IUserService>(services);
            RegisterContractProxy<ISellerService>(services);
            RegisterContractProxy<ICategoryService>(services);
            RegisterContractProxy<IBrandService>(services);
            RegisterContractProxy<IProductService>(services);
            RegisterContractProxy<ICartService>(services);
            RegisterContractProxy<IWishlistService>(services);
            RegisterContractProxy<IOrderService>(services);
            RegisterContractProxy<IPaymentService>(services);
            RegisterContractProxy<IReviewService>(services);
            RegisterContractProxy<IConversationService>(services);
            RegisterContractProxy<IReportService>(services);
            RegisterContractProxy<IAdminService>(services);
            return services;
        }

        private static void RegisterContractProxy<TService>(IServiceCollection services)
            where TService : class
        {
            services.TryAddScoped(_ => DispatchProxy.Create<TService, UnimplementedContractServiceProxy>());
        }
    }

    /// <summary>
    /// Fallback DispatchProxy allowing controllers and model validation to execute before concrete
    /// infrastructure services are registered in subsequent sprints.
    /// </summary>
    public class UnimplementedContractServiceProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            throw new NotImplementedException(
                $"Service contract method '{targetMethod?.DeclaringType?.Name}.{targetMethod?.Name}' is defined in the API contract and awaiting sprint implementation.");
        }
    }
}
