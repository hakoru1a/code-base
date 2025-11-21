using Infrastructure.Authorization.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Evaluates policies by name
    /// Policies are registered via dependency injection
    /// </summary>
    public class PolicyEvaluator : IPolicyEvaluator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _policyRegistry;
        private readonly ILogger<PolicyEvaluator>? _logger;

        public PolicyEvaluator(
            IServiceProvider serviceProvider, 
            Dictionary<string, Type> policyRegistry,
            ILogger<PolicyEvaluator>? logger = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
            _logger = logger;
        }

        public async Task<PolicyEvaluationResult> EvaluateAsync(
            string policyName,
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(policyName))
            {
                var error = "Policy name cannot be null or empty";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            if (user == null)
            {
                var error = "User context cannot be null";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            context ??= new Dictionary<string, object>();

            _logger?.LogDebug(
                "Evaluating policy '{PolicyName}' for user '{UserId}'",
                policyName, user.UserId);

            // Check if policy exists
            if (!_policyRegistry.TryGetValue(policyName, out var policyType))
            {
                var error = $"Policy '{policyName}' not found";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            // Resolve policy from DI
            var policy = _serviceProvider.GetService(policyType) as IPolicy;
            if (policy == null)
            {
                var error = $"Policy '{policyName}' (type: {policyType.FullName}) could not be instantiated from service provider";
                _logger?.LogError(error);
                return PolicyEvaluationResult.Deny(error);
            }

            // Evaluate policy with error handling
            try
            {
                var result = await policy.EvaluateAsync(user, context);
                
                _logger?.LogDebug(
                    "Policy '{PolicyName}' evaluation result: {Result} for user '{UserId}'. Reason: {Reason}",
                    policyName, 
                    result.IsAllowed ? "ALLOWED" : "DENIED",
                    user.UserId,
                    result.Reason);

                return result;
            }
            catch (Exception ex)
            {
                var error = $"Policy evaluation failed for '{policyName}': {ex.Message}";
                _logger?.LogError(ex, 
                    "Error evaluating policy '{PolicyName}' for user '{UserId}'",
                    policyName, user.UserId);
                return PolicyEvaluationResult.Deny(error);
            }
        }

        public async Task<PolicyEvaluationResult> EvaluateAllAsync(
            IEnumerable<string> policyNames,
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Input validation
            if (policyNames == null)
            {
                var error = "Policy names cannot be null";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            if (user == null)
            {
                var error = "User context cannot be null";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            context ??= new Dictionary<string, object>();

            var policyList = policyNames.ToList();
            if (policyList.Count == 0)
            {
                var error = "At least one policy name must be provided";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            _logger?.LogDebug(
                "Evaluating {Count} policies (ALL must pass) for user '{UserId}'",
                policyList.Count, user.UserId);

            foreach (var policyName in policyList)
            {
                var result = await EvaluateAsync(policyName, user, context);
                if (!result.IsAllowed)
                {
                    _logger?.LogDebug(
                        "Policy '{PolicyName}' failed in EvaluateAll, stopping evaluation",
                        policyName);
                    return result;
                }
            }

            _logger?.LogDebug(
                "All {Count} policies passed for user '{UserId}'",
                policyList.Count, user.UserId);

            return PolicyEvaluationResult.Allow("All policies passed");
        }

        public async Task<PolicyEvaluationResult> EvaluateAnyAsync(
            IEnumerable<string> policyNames,
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Input validation
            if (policyNames == null)
            {
                var error = "Policy names cannot be null";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            if (user == null)
            {
                var error = "User context cannot be null";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            context ??= new Dictionary<string, object>();

            var policyList = policyNames.ToList();
            if (policyList.Count == 0)
            {
                var error = "At least one policy name must be provided";
                _logger?.LogWarning(error);
                return PolicyEvaluationResult.Deny(error);
            }

            _logger?.LogDebug(
                "Evaluating {Count} policies (ANY must pass) for user '{UserId}'",
                policyList.Count, user.UserId);

            var reasons = new List<string>();

            foreach (var policyName in policyList)
            {
                var result = await EvaluateAsync(policyName, user, context);
                if (result.IsAllowed)
                {
                    _logger?.LogDebug(
                        "Policy '{PolicyName}' passed in EvaluateAny, stopping evaluation",
                        policyName);
                    return result;
                }
                reasons.Add($"{policyName}: {result.Reason}");
            }

            var denyReason = $"None of the policies passed. Reasons: {string.Join("; ", reasons)}";
            _logger?.LogDebug(
                "None of the {Count} policies passed for user '{UserId}'",
                policyList.Count, user.UserId);

            return PolicyEvaluationResult.Deny(denyReason);
        }
    }
}

