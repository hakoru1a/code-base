using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Product.Policies
{
    /// <summary>
    /// Policy for product viewing based on role
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class ProductViewPolicy : BasePolicy
    {
        public const string POLICY_NAME = "PRODUCT:VIEW";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can view products
            if (user.IsAuthenticated)
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    PolicyConstants.Messages.UserHasRequiredRoleAndPermission));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to view products"));
        }
    }
}

