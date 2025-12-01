using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Products.Policies
{
    /// <summary>
    /// Policy for product creation - requires admin or manager role
    /// </summary>
    [Policy("PRODUCT:CREATE", Description = "Create products")]
    public class ProductCreatePolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins and managers can create products
            if (HasAnyRole(user, Roles.Admin, Roles.Manager, Roles.ProductManager))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, Permissions.Product.Create))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "User has required role and permission"));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User has role but missing product creation permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role for product creation (Admin, Manager, or ProductManager required)"));
        }
    }
}

