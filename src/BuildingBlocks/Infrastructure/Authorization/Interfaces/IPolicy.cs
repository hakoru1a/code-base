using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization.Interfaces
{
    /// <summary>
    /// Base interface for all policies
    /// </summary>
    public interface IPolicy
    {
        string PolicyName { get; }
        Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, Dictionary<string, object> context);
    }
    
    /// <summary>
    /// Generic policy interface for strongly-typed context
    /// </summary>
    public interface IPolicy<TContext> : IPolicy where TContext : class
    {
        Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, TContext context);
    }
}

