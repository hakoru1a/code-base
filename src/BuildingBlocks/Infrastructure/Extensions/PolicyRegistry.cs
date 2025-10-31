using Infrastructure.Authorization.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Registry for collecting policy registrations during startup
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
        /// Example: registry.AddPolicy&lt;ProductViewPolicy&gt;("PRODUCT:VIEW");
        /// </summary>
        public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new ArgumentException("Policy name cannot be empty", nameof(policyName));
            }

            _services.AddScoped<TPolicy>();
            _policies.Add((policyName, typeof(TPolicy)));

            return this;
        }

        /// <summary>
        /// Register a policy with automatic name resolution from PolicyName property
        /// Requires parameterless constructor
        /// </summary>
        public PolicyRegistry AddPolicy<TPolicy>() where TPolicy : class, IPolicy, new()
        {
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

        internal IReadOnlyList<(string policyName, Type policyType)> GetRegisteredPolicies()
        {
            return _policies.AsReadOnly();
        }
    }
}
