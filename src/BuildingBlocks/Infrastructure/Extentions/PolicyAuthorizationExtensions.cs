using Infrastructure.Authorization;
using Infrastructure.Authorization.Interfaces;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extentions
{
    /// <summary>
    /// Extension methods for configuring PBAC (Policy-Based Access Control)
    /// </summary>
    public static class PolicyAuthorizationExtensions
    {
        /// <summary>
        /// Add PBAC services to the service collection
        /// Usage: services.AddPolicyBasedAuthorization(registry => {
        ///     registry.AddPolicy&lt;ProductViewPolicy&gt;("PRODUCT:VIEW");
        /// });
        /// </summary>
        public static IServiceCollection AddPolicyBasedAuthorization(
            this IServiceCollection services,
            Action<PolicyRegistry>? configurePolicies = null)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextAccessor, UserContextAccessor>();
            services.AddSingleton<IPolicyConfigurationService, PolicyConfigurationService>();

            var policyRegistry = new PolicyRegistry(services);
            configurePolicies?.Invoke(policyRegistry);

            services.AddSingleton<PolicyEvaluator>(sp =>
            {
                var evaluator = new PolicyEvaluator(sp);

                foreach (var (policyName, policyType) in policyRegistry.GetRegisteredPolicies())
                {
                    evaluator.RegisterPolicy(policyType, policyName);
                }

                return evaluator;
            });

            services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());

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

