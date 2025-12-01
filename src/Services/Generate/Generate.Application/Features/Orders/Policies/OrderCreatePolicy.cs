using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;

namespace Generate.Application.Features.Orders.Policies
{
    /// <summary>
    /// Policy for order creation - all authenticated users allowed
    /// </summary>
    [Policy("ORDER:CREATE", Description = "Create orders")]
    public class OrderCreatePolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can create orders
            if (IsAuthenticated(user))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Authenticated user can create orders"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to create orders"));
        }
    }
}

