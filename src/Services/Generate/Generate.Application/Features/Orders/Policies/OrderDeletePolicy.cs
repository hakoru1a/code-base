using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Orders.Policies
{
    /// <summary>
    /// Policy for order deletion - only admins and managers allowed
    /// </summary>
    [Policy("ORDER:DELETE", Description = "Delete orders")]
    public class OrderDeletePolicy : BasePolicy
    {
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

