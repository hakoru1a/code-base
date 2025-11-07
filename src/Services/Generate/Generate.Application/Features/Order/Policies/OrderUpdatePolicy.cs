using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Order.Policies
{
    /// <summary>
    /// Policy for order update based on role
    /// Users can update their own orders, admins/managers can update all orders
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class OrderUpdatePolicy : BasePolicy
    {
        public const string POLICY_NAME = "ORDER:UPDATE";
        public override string PolicyName => POLICY_NAME;

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
            if (user.IsAuthenticated)
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

