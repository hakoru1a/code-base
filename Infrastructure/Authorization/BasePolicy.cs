using Infrastructure.Authorization.Interfaces;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Base abstract policy with common functionality
    /// </summary>
    public abstract class BasePolicy : IPolicy
    {
        public abstract string PolicyName { get; }

        public abstract Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context);

        /// <summary>
        /// Helper method to check if user has required role
        /// </summary>
        protected bool HasRole(UserClaimsContext user, string role)
        {
            return user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Helper method to check if user has any of the required roles
        /// </summary>
        protected bool HasAnyRole(UserClaimsContext user, params string[] roles)
        {
            return roles.Any(role => HasRole(user, role));
        }

        /// <summary>
        /// Helper method to check if user has all required roles
        /// </summary>
        protected bool HasAllRoles(UserClaimsContext user, params string[] roles)
        {
            return roles.All(role => HasRole(user, role));
        }

        /// <summary>
        /// Helper method to check if user has required permission
        /// </summary>
        protected bool HasPermission(UserClaimsContext user, string permission)
        {
            return user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Helper method to get context value
        /// </summary>
        protected T? GetContextValue<T>(Dictionary<string, object> context, string key)
        {
            if (context.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }
    }

    /// <summary>
    /// Base abstract policy with strongly-typed context
    /// </summary>
    public abstract class BasePolicy<TContext> : BasePolicy, IPolicy<TContext> where TContext : class
    {
        public override async Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Try to extract strongly-typed context
            if (context.TryGetValue("TypedContext", out var obj) && obj is TContext typedContext)
            {
                return await EvaluateAsync(user, typedContext);
            }

            // Try to convert dictionary to typed context
            try
            {
                var converted = ConvertToTypedContext(context);
                if (converted != null)
                {
                    return await EvaluateAsync(user, converted);
                }
            }
            catch (Exception ex)
            {
                return PolicyEvaluationResult.Deny($"Failed to convert context: {ex.Message}");
            }

            return PolicyEvaluationResult.Deny("Invalid context type");
        }

        public abstract Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, TContext context);

        /// <summary>
        /// Override this to provide custom context conversion logic
        /// </summary>
        protected virtual TContext? ConvertToTypedContext(Dictionary<string, object> context)
        {
            return null;
        }
    }
}

