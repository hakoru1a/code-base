using Generate.Application.Features.Category.Policies;
using Generate.Application.Features.Product.Policies;
using Generate.Application.Features.Order.Policies;
using Infrastructure.Extensions;
using Shared.Identity;

namespace Generate.API.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
           // Add Keycloak Authentication (RBAC at Gateway, but also validated at Service level)
            services.AddKeycloakAuthentication(configuration);
            services.AddKeycloakAuthorization();

            // Add Policy-Based Authorization (PBAC at Service level)
            services.AddPolicyBasedAuthorization(policies =>
            {
                // Category Policies
                policies.AddPolicy<CategoryViewPolicy>(PolicyNames.Pbac.Category.View);
                policies.AddPolicy<CategoryCreatePolicy>(PolicyNames.Pbac.Category.Create);
                policies.AddPolicy<CategoryUpdatePolicy>(PolicyNames.Pbac.Category.Update);
                policies.AddPolicy<CategoryDeletePolicy>(PolicyNames.Pbac.Category.Delete);

                // Product Policies
                policies.AddPolicy<ProductViewPolicy>(PolicyNames.Pbac.Product.View);
                policies.AddPolicy<ProductCreatePolicy>(PolicyNames.Pbac.Product.Create);
                policies.AddPolicy<ProductUpdatePolicy>(PolicyNames.Pbac.Product.Update);
                policies.AddPolicy<ProductDeletePolicy>(PolicyNames.Pbac.Product.Delete);

                // Order Policies
                policies.AddPolicy<OrderViewPolicy>(PolicyNames.Pbac.Order.View);
                policies.AddPolicy<OrderCreatePolicy>(PolicyNames.Pbac.Order.Create);
                policies.AddPolicy<OrderUpdatePolicy>(PolicyNames.Pbac.Order.Update);
                policies.AddPolicy<OrderDeletePolicy>(PolicyNames.Pbac.Order.Delete);
            });
            return services;
        }
    }
}
