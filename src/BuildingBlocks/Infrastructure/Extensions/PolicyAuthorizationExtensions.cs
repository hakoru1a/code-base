using Infrastructure.Authorization;
using Infrastructure.Authorization.Interfaces;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for configuring PBAC (Policy-Based Access Control)
    /// </summary>
    public static class PolicyAuthorizationExtensions
    {
        /// <summary>
        /// Add PBAC services with auto-discovery of policies from assemblies
        /// </summary>
        /// <example>
        /// services.AddPolicyBasedAuthorization(registry => {
        ///     registry.ScanAssemblies(typeof(ProductViewPolicy).Assembly);
        /// });
        /// </example>
        public static IServiceCollection AddPolicyBasedAuthorization(
            this IServiceCollection services,
            Action<PolicyRegistry> configurePolicies)
        {
            // Add required services
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextAccessor, UserContextAccessor>();

            // Build policy registry
            var policyRegistry = new PolicyRegistry();
            configurePolicies.Invoke(policyRegistry);
            
            var policies = policyRegistry.GetPolicies();

            // Register all discovered policies
            foreach (var (_, policyType) in policies)
            {
                services.AddScoped(policyType);
            }

            // Register policy evaluator with registry
            services.AddSingleton<IPolicyEvaluator>(sp => 
                new PolicyEvaluator(sp, policies));

            return services;
        }

        /// <summary>
        /// Use policy authorization middleware
        /// Must be called AFTER UseAuthentication() and UseAuthorization()
        /// </summary>
        public static IApplicationBuilder UsePolicyAuthorization(this IApplicationBuilder app)
        {
            app.UseMiddleware<PolicyAuthorizationMiddleware>();
            return app;
        }
    }
}

