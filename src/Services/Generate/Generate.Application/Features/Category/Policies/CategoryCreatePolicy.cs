using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Category.Policies
{
    /// <summary>
    /// Policy for category creation based on role and business rules
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class CategoryCreatePolicy : BasePolicy
    {
        public const string POLICY_NAME = "CATEGORY:CREATE";
        public override string PolicyName => POLICY_NAME;

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
                        PolicyConstants.Messages.UserHasRequiredRoleAndPermission));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User has role but missing category creation permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role for category creation (Admin, Manager, or ProductManager required)"));
        }
    }
}

