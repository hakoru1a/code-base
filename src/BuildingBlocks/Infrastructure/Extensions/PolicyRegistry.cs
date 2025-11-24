using Infrastructure.Authorization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PolicyRegistry>? _logger;

        public PolicyRegistry(ILogger<PolicyRegistry>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Scan assemblies for policies marked with [Policy] attribute
        /// </summary>
        public PolicyRegistry ScanAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new ArgumentException("At least one assembly must be provided", nameof(assemblies));
            }

            var discoveredCount = 0;
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
                        RegisterPolicy(attribute.Name, policyType);
                        discoveredCount++;
                    }
                }
            }

            _logger?.LogInformation(
                "Policy registry: Discovered {Count} policies from {AssemblyCount} assembly(ies)",
                discoveredCount, assemblies.Length);

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

            RegisterPolicy(policyName, typeof(TPolicy));
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

            RegisterPolicy(attribute.Name, policyType);
            return this;
        }

        /// <summary>
        /// Register a policy with duplicate name validation
        /// </summary>
        private void RegisterPolicy(string policyName, Type policyType)
        {
            if (_policies.TryGetValue(policyName, out var existingType))
            {
                var errorMessage =
                    $"Duplicate policy name '{policyName}' detected. " +
                    $"Already registered: {existingType.FullName}, " +
                    $"Attempting to register: {policyType.FullName}";

                _logger?.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            _policies[policyName] = policyType;
            _logger?.LogDebug("Registered policy '{PolicyName}' -> {PolicyType}",
                policyName, policyType.FullName);
        }

        /// <summary>
        /// Get all registered policies
        /// </summary>
        internal Dictionary<string, Type> GetPolicies() => new(_policies);

        /// <summary>
        /// Get count of registered policies
        /// </summary>
        public int Count => _policies.Count;

        /// <summary>
        /// Get all registered policy names
        /// </summary>
        public IEnumerable<string> GetPolicyNames() => _policies.Keys;
    }
}
