using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Categories.Policies
{
    /// <summary>
    /// Policy for category deletion - only admins allowed
    /// </summary>
    [Policy("CATEGORY:DELETE", Description = "Delete categories")]
    public class CategoryDeletePolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins can delete categories
            if (HasRole(user, Roles.Admin))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, Permissions.Product.Delete))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "User has required role and permission"));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "Admin role detected but missing delete permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "Only administrators can delete categories"));
        }
    }
}

