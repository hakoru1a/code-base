using Generate.Application.Features.Products.Policies;
using Infrastructure.Extensions;

namespace Generate.API.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Keycloak Authentication (RBAC at Gateway, but also validated at Service level)
            services.AddKeycloakAuthentication(configuration);
            services.AddKeycloakAuthorization();

            // Add Policy-Based Authorization (PBAC at Service level) with auto-discovery
            services.AddPolicyBasedAuthorization(registry =>
            {
                // Auto-discover all policies marked with [Policy] attribute from Application assembly
                registry.ScanAssemblies(typeof(ProductViewPolicy).Assembly);
            });

            return services;
        }
    }
}
