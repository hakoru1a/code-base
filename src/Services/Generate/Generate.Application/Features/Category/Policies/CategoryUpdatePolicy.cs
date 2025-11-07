using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Category.Policies
{
    /// <summary>
    /// Policy for category update based on role and business rules
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class CategoryUpdatePolicy : BasePolicy
    {
        public const string POLICY_NAME = "CATEGORY:UPDATE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins and managers can update categories
            if (HasAnyRole(user, Roles.Admin, Roles.Manager, Roles.ProductManager))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, Permissions.Product.Update))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        PolicyConstants.Messages.UserHasRequiredRoleAndPermission));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User has role but missing category update permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role for category update (Admin, Manager, or ProductManager required)"));
        }
    }
}

