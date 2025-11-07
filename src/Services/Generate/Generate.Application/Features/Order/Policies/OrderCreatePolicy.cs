using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Order.Policies
{
    /// <summary>
    /// Policy for order creation based on role
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class OrderCreatePolicy : BasePolicy
    {
        public const string POLICY_NAME = "ORDER:CREATE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can create orders
            if (user.IsAuthenticated)
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Authenticated user can create orders"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to create orders"));
        }
    }
}

