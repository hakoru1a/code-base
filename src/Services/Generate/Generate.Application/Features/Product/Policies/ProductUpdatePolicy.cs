using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Product.Policies
{
    /// <summary>
    /// Policy for product update - requires admin or manager role
    /// </summary>
    [Policy("PRODUCT:UPDATE", Description = "Update products")]
    public class ProductUpdatePolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins and managers can update products
            if (HasAnyRole(user, Roles.Admin, Roles.Manager, Roles.ProductManager))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, Permissions.Product.Update))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "User has required role and permission"));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User has role but missing product update permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role for product update (Admin, Manager, or ProductManager required)"));
        }
    }
}

