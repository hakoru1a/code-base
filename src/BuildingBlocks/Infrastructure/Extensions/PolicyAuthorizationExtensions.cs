using Infrastructure.Authorization;
using Infrastructure.Authorization.Interfaces;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            // Build policy registry (logger will be available at runtime)
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
            {
                var logger = sp.GetService<ILogger<PolicyEvaluator>>();
                var evaluator = new PolicyEvaluator(sp, policies, logger);
                
                // Log registered policies count at startup
                var registryLogger = sp.GetService<ILogger<PolicyRegistry>>();
                registryLogger?.LogInformation(
                    "Policy-Based Authorization initialized with {Count} registered policies: {Policies}",
                    policies.Count,
                    string.Join(", ", policies.Keys));
                
                return evaluator;
            });

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

