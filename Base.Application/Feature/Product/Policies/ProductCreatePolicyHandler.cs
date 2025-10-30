using Infrastructure.Authorization;
using Shared.DTOs.Authorization;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Policy for product creation based on role and business rules
    /// </summary>
    public class ProductCreatePolicyHandler : BasePolicy
    {
        public const string POLICY_NAME = "PRODUCT_CREATE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Only admins and managers can create products
            if (HasAnyRole(user, "admin", "manager", "product_manager"))
            {
                // Additional validation: check if user has permission
                if (HasPermission(user, "product:create"))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "User has required role and permission"));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User has required role but missing 'product:create' permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role (admin, manager, or product_manager)"));
        }
    }
}
