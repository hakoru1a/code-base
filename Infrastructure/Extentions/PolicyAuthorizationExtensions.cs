using Infrastructure.Authorization;
using Infrastructure.Authorization.Interfaces;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
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
        /// </summary>
        public static IServiceCollection AddPolicyBasedAuthorization(
            this IServiceCollection services,
            Action<PolicyRegistry>? configurePolicies = null)
        {
            // Register the policy evaluator as a singleton
            services.AddSingleton<PolicyEvaluator>();
            services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());

            // Register UserContextAccessor
            services.AddScoped<IUserContextAccessor, UserContextAccessor>();

            // Configure policy registry
            if (configurePolicies != null)
            {
                var registry = new PolicyRegistry(services);
                configurePolicies(registry);
            }

            return services;
        }

        /// <summary>
        /// Use policy authorization middleware
        /// </summary>
        public static IApplicationBuilder UsePolicyAuthorization(this IApplicationBuilder app)
        {
            app.UseMiddleware<PolicyAuthorizationMiddleware>();
            return app;
        }
    }

    /// <summary>
    /// Registry for registering policies
    /// </summary>
    public class PolicyRegistry
    {
        private readonly IServiceCollection _services;
        private readonly List<(string policyName, Type policyType)> _policies = new();

        public PolicyRegistry(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Register a policy
        /// </summary>
        public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
        {
            _services.AddScoped<TPolicy>();
            _policies.Add((policyName, typeof(TPolicy)));

            // Configure the evaluator after all services are built
            _services.AddSingleton<IPolicyEvaluator>(sp =>
            {
                var evaluator = sp.GetRequiredService<PolicyEvaluator>();
                foreach (var (name, type) in _policies)
                {
                    evaluator.RegisterPolicy<IPolicy>(name);
                }
                return evaluator;
            });

            return this;
        }

        /// <summary>
        /// Register a policy with automatic name resolution
        /// </summary>
        public PolicyRegistry AddPolicy<TPolicy>() where TPolicy : class, IPolicy
        {
            // Try to instantiate to get the policy name
            var tempPolicy = Activator.CreateInstance<TPolicy>();
            return AddPolicy<TPolicy>(tempPolicy.PolicyName);
        }
    }
}

