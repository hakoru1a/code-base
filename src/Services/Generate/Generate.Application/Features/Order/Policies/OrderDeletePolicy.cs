using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Order.Policies
{
    /// <summary>
    /// Policy for order deletion
    /// Only admins and managers can delete orders
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class OrderDeletePolicy : BasePolicy
    {
        public const string POLICY_NAME = "ORDER:DELETE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins and managers can delete orders
            if (HasAnyRole(user, Roles.Admin, Roles.Manager))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Admin/Manager can delete orders"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "Only administrators and managers can delete orders"));
        }
    }
}

