using Infrastructure.Authorization.Interfaces;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Base abstract policy with common functionality
    /// Use [Policy("POLICY_NAME")] attribute on derived classes
    /// </summary>
    public abstract class BasePolicy : IPolicy
    {
        /// <summary>
        /// Evaluate if user is allowed to perform the action
        /// </summary>
        public abstract Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context);

        /// <summary>
        /// Check if user has required role
        /// </summary>
        protected bool HasRole(UserClaimsContext user, string role)
        {
            return user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if user has any of the required roles
        /// </summary>
        protected bool HasAnyRole(UserClaimsContext user, params string[] roles)
        {
            return roles.Any(role => HasRole(user, role));
        }

        /// <summary>
        /// Check if user has all required roles
        /// </summary>
        protected bool HasAllRoles(UserClaimsContext user, params string[] roles)
        {
            return roles.All(role => HasRole(user, role));
        }

        /// <summary>
        /// Check if user has required permission
        /// </summary>
        protected bool HasPermission(UserClaimsContext user, string permission)
        {
            return user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get typed value from context dictionary
        /// </summary>
        protected T? GetContextValue<T>(Dictionary<string, object> context, string key)
        {
            if (context.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        protected bool IsAuthenticated(UserClaimsContext user)
        {
            return user.IsAuthenticated;
        }
    }
}

