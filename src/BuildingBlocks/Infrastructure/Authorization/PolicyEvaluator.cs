using Infrastructure.Authorization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    public class PolicyEvaluator : IPolicyEvaluator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _policyRegistry;

        public PolicyEvaluator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _policyRegistry = new Dictionary<string, Type>();
        }

        public void RegisterPolicy<TPolicy>(string policyName) where TPolicy : IPolicy
        {
            _policyRegistry[policyName] = typeof(TPolicy);
        }

        public void RegisterPolicy(Type policyType, string policyName)
        {
            if (!typeof(IPolicy).IsAssignableFrom(policyType))
            {
                throw new ArgumentException($"Type {policyType.Name} does not implement IPolicy", nameof(policyType));
            }

            _policyRegistry[policyName] = policyType;
        }

        public async Task<PolicyEvaluationResult> EvaluateAsync(
            string policyName,
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            if (!_policyRegistry.TryGetValue(policyName, out var policyType))
            {
                return PolicyEvaluationResult.Deny($"Policy '{policyName}' not found");
            }

            var policy = _serviceProvider.GetService(policyType) as IPolicy;
            if (policy == null)
            {
                return PolicyEvaluationResult.Deny($"Policy '{policyName}' could not be instantiated");
            }

            return await policy.EvaluateAsync(user, context);
        }

        public async Task<PolicyEvaluationResult> EvaluateAllAsync(
            IEnumerable<string> policyNames,
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            foreach (var policyName in policyNames)
            {
                var result = await EvaluateAsync(policyName, user, context);
                if (!result.IsAllowed)
                {
                    return result;
                }
            }

            return PolicyEvaluationResult.Allow("All policies passed");
        }

        public async Task<PolicyEvaluationResult> EvaluateAnyAsync(
            IEnumerable<string> policyNames,
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            var reasons = new List<string>();

            foreach (var policyName in policyNames)
            {
                var result = await EvaluateAsync(policyName, user, context);
                if (result.IsAllowed)
                {
                    return result;
                }
                reasons.Add($"{policyName}: {result.Reason}");
            }

            return PolicyEvaluationResult.Deny(
                $"None of the policies passed. Reasons: {string.Join("; ", reasons)}");
        }
    }
}

