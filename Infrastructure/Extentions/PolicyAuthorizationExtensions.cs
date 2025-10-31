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
    /// Provides a clean, enterprise-grade API for registering policies and middleware
    /// </summary>
    public static class PolicyAuthorizationExtensions
    {
        /// <summary>
        /// Add PBAC services to the service collection
        /// 
        /// REGISTRATION FLOW (simplified):
        /// 1. Register all infrastructure services (HttpContextAccessor, UserContextAccessor, ConfigService)
        /// 2. Create PolicyRegistry and let user configure policies via callback
        /// 3. Register PolicyEvaluator as Singleton with all policies pre-registered
        /// 
        /// USAGE:
        /// services.AddPolicyBasedAuthorization(registry => {
        ///     registry.AddPolicy&lt;ProductViewPolicy&gt;("PRODUCT:VIEW");
        ///     registry.AddPolicy&lt;ProductCreatePolicy&gt;("PRODUCT:CREATE");
        /// });
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configurePolicies">Optional callback to configure policies</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddPolicyBasedAuthorization(
            this IServiceCollection services,
            Action<PolicyRegistry>? configurePolicies = null)
        {
            // STEP 1: Register infrastructure services
            // These services are needed by policies and middleware
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextAccessor, UserContextAccessor>();
            services.AddSingleton<IPolicyConfigurationService, PolicyConfigurationService>();

            // STEP 2: Create policy registry and let user register policies
            var policyRegistry = new PolicyRegistry(services);
            configurePolicies?.Invoke(policyRegistry);

            // STEP 3: Register PolicyEvaluator with all policies
            // This is done in ONE PLACE to avoid confusion and circular dependencies
            services.AddSingleton<PolicyEvaluator>(sp =>
            {
                var evaluator = new PolicyEvaluator(sp);

                // Register all policies that were added to the registry
                foreach (var (policyName, policyType) in policyRegistry.GetRegisteredPolicies())
                {
                    // Use non-generic method to avoid reflection
                    evaluator.RegisterPolicy(policyType, policyName);
                }

                return evaluator;
            });

            // Register interface pointing to the same singleton instance
            services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());

            return services;
        }

        /// <summary>
        /// Use policy authorization middleware
        /// 
        /// IMPORTANT: Must be called AFTER UseAuthentication() and UseAuthorization()
        /// in your middleware pipeline to ensure User claims are available
        /// 
        /// USAGE in Program.cs:
        /// app.UseAuthentication();
        /// app.UseAuthorization();
        /// app.UsePolicyAuthorization();  // &lt;-- Add this after auth middleware
        /// </summary>
        public static IApplicationBuilder UsePolicyAuthorization(this IApplicationBuilder app)
        {
            app.UseMiddleware<PolicyAuthorizationMiddleware>();
            return app;
        }
    }

    /// <summary>
    /// Registry for collecting policy registrations
    /// Provides a fluent API for policy registration during startup
    /// 
    /// DESIGN NOTES:
    /// - Does NOT create policy instances during registration (no side effects)
    /// - Only tracks policy types and names for later registration
    /// - All policies are registered as Scoped services (per-request lifecycle)
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
        /// 
        /// RECOMMENDED APPROACH for enterprise applications:
        /// - Explicit names prevent confusion
        /// - No hidden behavior or instance creation
        /// - Works with policies that have dependencies in constructor
        /// 
        /// EXAMPLE:
        /// registry.AddPolicy&lt;ProductViewPolicy&gt;("PRODUCT:VIEW");
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

            // Register policy as scoped service in DI container
            // Scoped = one instance per HTTP request
            _services.AddScoped<TPolicy>();

            // Track policy for later registration with PolicyEvaluator
            _policies.Add((policyName, typeof(TPolicy)));

            return this;
        }

        /// <summary>
        /// Register a policy with automatic name resolution from PolicyName property
        /// 
        /// WARNING: This method creates a temporary instance to read PolicyName.
        /// Only use if:
        /// - Policy has parameterless constructor
        /// - Policy constructor has no side effects
        /// 
        /// FOR ENTERPRISE: Prefer AddPolicy&lt;T&gt;(string name) for clarity
        /// 
        /// EXAMPLE:
        /// registry.AddPolicy&lt;ProductViewPolicy&gt;();  // reads PolicyName from instance
        /// </summary>
        /// <typeparam name="TPolicy">The policy implementation type</typeparam>
        /// <returns>This registry for method chaining</returns>
        public PolicyRegistry AddPolicy<TPolicy>() where TPolicy : class, IPolicy, new()
        {
            // Create temporary instance ONLY to read PolicyName property
            // This is the ONLY place we create instances during registration
            var tempPolicy = new TPolicy();
            var policyName = tempPolicy.PolicyName;

            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new InvalidOperationException(
                    $"Policy {typeof(TPolicy).Name} has empty PolicyName. " +
                    $"Use AddPolicy<{typeof(TPolicy).Name}>(\"NAME\") instead.");
            }

            return AddPolicy<TPolicy>(policyName);
        }

        /// <summary>
        /// Get all registered policies
        /// Used internally by AddPolicyBasedAuthorization to register policies with PolicyEvaluator
        /// </summary>
        internal IReadOnlyList<(string policyName, Type policyType)> GetRegisteredPolicies()
        {
            return _policies.AsReadOnly();
        }
    }
}

