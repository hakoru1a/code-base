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
        /// </summary>
        public static IServiceCollection AddPolicyBasedAuthorization(
            this IServiceCollection services,
            Action<PolicyRegistry>? configurePolicies = null)
        {
            // Register IHttpContextAccessor if not already registered
            services.AddHttpContextAccessor();

            // Register the policy evaluator as singleton
            services.AddSingleton<PolicyEvaluator>();

            // Register IPolicyEvaluator interface pointing to the same instance
            services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());

            // Register UserContextAccessor as scoped (per request)
            services.AddScoped<IUserContextAccessor, UserContextAccessor>();

            // Register PolicyConfigurationService for dynamic config
            services.AddSingleton<IPolicyConfigurationService, PolicyConfigurationService>();

            // Configure policy registry if provided
            if (configurePolicies != null)
            {
                var registry = new PolicyRegistry(services);
                configurePolicies(registry);
                
                // Build the evaluator with registered policies
                registry.BuildEvaluator();
            }

            return services;
        }

        /// <summary>
        /// Use policy authorization middleware
        /// Must be called after UseAuthentication() and UseAuthorization()
        /// </summary>
        public static IApplicationBuilder UsePolicyAuthorization(this IApplicationBuilder app)
        {
            app.UseMiddleware<PolicyAuthorizationMiddleware>();
            return app;
        }
    }

    /// <summary>
    /// Registry for registering policies
    /// Provides a fluent API for policy registration
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
        /// Register a policy with explicit policy name
        /// </summary>
        /// <typeparam name="TPolicy">The policy implementation type</typeparam>
        /// <param name="policyName">The policy name (e.g., "PRODUCT:VIEW")</param>
        /// <returns>This registry for method chaining</returns>
        public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new ArgumentException("Policy name cannot be empty", nameof(policyName));
            }

            // Register policy as scoped service (per request)
            _services.AddScoped<TPolicy>();
            
            // Track policy for registration with evaluator
            _policies.Add((policyName, typeof(TPolicy)));

            return this;
        }

        /// <summary>
        /// Register a policy with automatic name resolution from PolicyName property
        /// Requires the policy to have a parameterless constructor
        /// </summary>
        /// <typeparam name="TPolicy">The policy implementation type</typeparam>
        /// <returns>This registry for method chaining</returns>
        public PolicyRegistry AddPolicy<TPolicy>() where TPolicy : class, IPolicy, new()
        {
            // Create temporary instance to get policy name
            var tempPolicy = new TPolicy();
            return AddPolicy<TPolicy>(tempPolicy.PolicyName);
        }

        /// <summary>
        /// Internal method to build and configure the policy evaluator
        /// Called automatically after all policies are registered
        /// </summary>
        internal void BuildEvaluator()
        {
            if (!_policies.Any())
            {
                return;
            }

            // Replace the policy evaluator registration with one that has all policies registered
            _services.AddSingleton<PolicyEvaluator>(sp =>
            {
                var evaluator = new PolicyEvaluator(sp);
                
                // Register all policies with the evaluator
                foreach (var (policyName, policyType) in _policies)
                {
                    // Use reflection to call generic RegisterPolicy method
                    var method = evaluator.GetType()
                        .GetMethod(nameof(PolicyEvaluator.RegisterPolicy))
                        ?.MakeGenericMethod(policyType);
                    
                    method?.Invoke(evaluator, new object[] { policyName });
                }

                return evaluator;
            });
        }
    }
}

