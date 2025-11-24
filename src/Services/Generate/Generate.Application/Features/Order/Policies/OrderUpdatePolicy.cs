using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Order.Policies
{
    /// <summary>
    /// Policy for order update - users can update own orders, admins/managers can update all
    /// </summary>
    [Policy("ORDER:UPDATE", Description = "Update orders")]
    public class OrderUpdatePolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Admins and managers can update any order
            if (HasAnyRole(user, Roles.Admin, Roles.Manager))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Admin/Manager can update any order"));
            }

            // Regular users can update their own orders
            if (IsAuthenticated(user))
            {
                // TODO: Add check to verify if order belongs to user
                // For now, allow authenticated users to update
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Authenticated user can update orders"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have permission to update orders"));
        }
    }
}

