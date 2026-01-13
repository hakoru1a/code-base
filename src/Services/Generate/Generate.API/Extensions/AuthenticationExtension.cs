using Generate.Application.Features.Products.Policies;
using Infrastructure.Extensions;

namespace Generate.API.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Simple JWT Authentication for microservice (Gateway handles full Keycloak flow)
            // This only validates JWT tokens forwarded from Gateway
            services.AddJwtAuthentication(configuration);
            services.AddBasicAuthorization();

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
