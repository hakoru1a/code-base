namespace Shared.DTOs.Authorization
{
    /// <summary>
    /// Base policy requirement for PBAC
    /// </summary>
    public class PolicyRequirement
    {
        public string PolicyName { get; set; } = string.Empty;
        public Dictionary<string, object> Context { get; set; } = new();
    }
    
    /// <summary>
    /// Policy evaluation result
    /// </summary>
    public class PolicyEvaluationResult
    {
        public bool IsAllowed { get; set; }
        public string? Reason { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        
        public static PolicyEvaluationResult Allow(string? reason = null)
        {
            return new PolicyEvaluationResult 
            { 
                IsAllowed = true, 
                Reason = reason ?? "Access granted" 
            };
        }
        
        public static PolicyEvaluationResult Deny(string reason)
        {
            return new PolicyEvaluationResult 
            { 
                IsAllowed = false, 
                Reason = reason 
            };
        }
    }
    
    /// <summary>
    /// User claims context for policy evaluation
    /// </summary>
    public class UserClaimsContext
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public Dictionary<string, object> CustomAttributes { get; set; } = new();
        
        /// <summary>
        /// Check if user is authenticated (has UserId)
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
    }
}

