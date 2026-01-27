using Infrastructure.Extensions;

namespace TLBIOMASS.API.Extensions
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
            });

            return services;
        }
    }
}
