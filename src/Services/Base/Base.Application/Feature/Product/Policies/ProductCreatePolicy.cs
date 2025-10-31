using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Policy for product creation based on role and business rules
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class ProductCreatePolicy : BasePolicy
    {
        public const string POLICY_NAME = "PRODUCT:CREATE";
        public override string PolicyName => POLICY_NAME;

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
                        PolicyConstants.Messages.UserHasRequiredRoleAndPermission));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    PolicyConstants.Messages.UserHasRoleButMissingProductCreatePermission));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                PolicyConstants.Messages.UserDoesNotHaveRequiredRoleForProductCreate));
        }
    }
}

