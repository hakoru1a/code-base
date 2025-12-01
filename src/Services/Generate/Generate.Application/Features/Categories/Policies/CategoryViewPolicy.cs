using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;

namespace Generate.Application.Features.Categories.Policies
{
    /// <summary>
    /// Policy for category viewing - all authenticated users allowed
    /// </summary>
    [Policy("CATEGORY:VIEW", Description = "View categories")]
    public class CategoryViewPolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can view categories
            if (IsAuthenticated(user))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "User is authenticated"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to view categories"));
        }
    }
}

