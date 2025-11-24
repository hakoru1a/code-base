using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;

namespace Generate.Application.Features.Order.Policies
{
    /// <summary>
    /// Policy for order viewing - all authenticated users allowed
    /// </summary>
    [Policy("ORDER:VIEW", Description = "View orders")]
    public class OrderViewPolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can view their own orders
            // Admins and managers can view all orders
            if (IsAuthenticated(user))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "User is authenticated"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to view orders"));
        }
    }
}

