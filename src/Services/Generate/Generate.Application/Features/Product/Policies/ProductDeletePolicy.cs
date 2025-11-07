using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Product.Policies
{
    /// <summary>
    /// Policy for product deletion - only admins can delete
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class ProductDeletePolicy : BasePolicy
    {
        public const string POLICY_NAME = "PRODUCT:DELETE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins can delete products
            if (HasRole(user, Roles.Admin))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, Permissions.Product.Delete))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        PolicyConstants.Messages.UserHasRequiredRoleAndPermission));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "Admin role detected but missing delete permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "Only administrators can delete products"));
        }
    }
}

