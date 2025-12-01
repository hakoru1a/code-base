using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Categories.Policies
{
    /// <summary>
    /// Policy for category creation - requires admin or manager role
    /// </summary>
    [Policy("CATEGORY:CREATE", Description = "Create categories")]
    public class CategoryCreatePolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins and managers can create categories
            if (HasAnyRole(user, Roles.Admin, Roles.Manager, Roles.ProductManager))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, Permissions.Product.Create))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "User has required role and permission"));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User has role but missing category creation permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role for category creation (Admin, Manager, or ProductManager required)"));
        }
    }
}

