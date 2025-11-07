using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Order.Policies
{
    /// <summary>
    /// Policy for order viewing based on role
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class OrderViewPolicy : BasePolicy
    {
        public const string POLICY_NAME = "ORDER:VIEW";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can view their own orders
            // Admins and managers can view all orders
            if (user.IsAuthenticated)
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    PolicyConstants.Messages.UserHasRequiredRoleAndPermission));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to view orders"));
        }
    }
}

