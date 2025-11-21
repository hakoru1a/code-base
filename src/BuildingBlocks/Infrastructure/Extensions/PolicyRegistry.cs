using Infrastructure.Authorization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Attributes;
using System.Reflection;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Registry for policy discovery and registration
    /// </summary>
    public class PolicyRegistry
    {
        private readonly Dictionary<string, Type> _policies = new();

        /// <summary>
        /// Scan assemblies for policies marked with [Policy] attribute
        /// </summary>
        public PolicyRegistry ScanAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var policyTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IPolicy).IsAssignableFrom(t))
                    .Where(t => t.GetCustomAttribute<PolicyAttribute>() != null);

                foreach (var policyType in policyTypes)
                {
                    var attribute = policyType.GetCustomAttribute<PolicyAttribute>();
                    if (attribute != null)
                    {
                        _policies[attribute.Name] = policyType;
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Manually register a policy with explicit name
        /// Use this if you prefer not to use attributes
        /// </summary>
        public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new ArgumentException("Policy name cannot be empty", nameof(policyName));
            }

            _policies[policyName] = typeof(TPolicy);
            return this;
        }

        /// <summary>
        /// Manually register a policy using its [Policy] attribute
        /// </summary>
        public PolicyRegistry AddPolicy<TPolicy>() where TPolicy : class, IPolicy
        {
            var policyType = typeof(TPolicy);
            var attribute = policyType.GetCustomAttribute<PolicyAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"Policy {policyType.Name} does not have [Policy] attribute. " +
                    $"Use AddPolicy<{policyType.Name}>(\"NAME\") instead.");
            }

            _policies[attribute.Name] = policyType;
            return this;
        }

        /// <summary>
        /// Get all registered policies
        /// </summary>
        internal Dictionary<string, Type> GetPolicies() => new(_policies);
    }
}
