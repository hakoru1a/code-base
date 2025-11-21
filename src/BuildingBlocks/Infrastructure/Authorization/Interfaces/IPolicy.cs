using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization.Interfaces
{
    /// <summary>
    /// Base interface for all policies
    /// Policies should be marked with [Policy("NAME")] attribute
    /// </summary>
    public interface IPolicy
    {
        /// <summary>
        /// Evaluate if user is allowed based on context
        /// </summary>
        Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, Dictionary<string, object> context);
    }
}

