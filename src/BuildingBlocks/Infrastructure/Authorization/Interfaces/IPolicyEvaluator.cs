using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization.Interfaces
{
    /// <summary>
    /// Service to evaluate policies
    /// </summary>
    public interface IPolicyEvaluator
    {
        /// <summary>
        /// Evaluate a policy with given context
        /// </summary>
        Task<PolicyEvaluationResult> EvaluateAsync(
            string policyName, 
            UserClaimsContext user, 
            Dictionary<string, object> context);
        
        /// <summary>
        /// Evaluate multiple policies (all must pass)
        /// </summary>
        Task<PolicyEvaluationResult> EvaluateAllAsync(
            IEnumerable<string> policyNames, 
            UserClaimsContext user, 
            Dictionary<string, object> context);
        
        /// <summary>
        /// Evaluate multiple policies (at least one must pass)
        /// </summary>
        Task<PolicyEvaluationResult> EvaluateAnyAsync(
            IEnumerable<string> policyNames, 
            UserClaimsContext user, 
            Dictionary<string, object> context);
    }
}

