using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;

namespace Generate.Application.Features.Product.Policies
{
    /// <summary>
    /// Policy for product viewing - all authenticated users allowed
    /// </summary>
    [Policy("PRODUCT:VIEW", Description = "View products")]
    public class ProductViewPolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can view products
            if (IsAuthenticated(user))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "User is authenticated"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to view products"));
        }
    }
}

