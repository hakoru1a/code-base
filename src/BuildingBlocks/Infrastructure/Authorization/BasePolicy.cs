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
            if (user == null || string.IsNullOrWhiteSpace(role))
            {
                return false;
            }
            return user.Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Check if user has any of the required roles
        /// </summary>
        protected bool HasAnyRole(UserClaimsContext user, params string[] roles)
        {
            if (user == null || roles == null || roles.Length == 0)
            {
                return false;
            }
            return roles.Any(role => HasRole(user, role));
        }

        /// <summary>
        /// Check if user has all required roles
        /// </summary>
        protected bool HasAllRoles(UserClaimsContext user, params string[] roles)
        {
            if (user == null || roles == null || roles.Length == 0)
            {
                return false;
            }
            return roles.All(role => HasRole(user, role));
        }

        /// <summary>
        /// Check if user has required permission
        /// </summary>
        protected bool HasPermission(UserClaimsContext user, string permission)
        {
            if (user == null || string.IsNullOrWhiteSpace(permission))
            {
                return false;
            }
            return user.Permissions?.Contains(permission, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Check if user has any of the required permissions
        /// </summary>
        protected bool HasAnyPermission(UserClaimsContext user, params string[] permissions)
        {
            if (user == null || permissions == null || permissions.Length == 0)
            {
                return false;
            }
            return permissions.Any(permission => HasPermission(user, permission));
        }

        /// <summary>
        /// Check if user has all required permissions
        /// </summary>
        protected bool HasAllPermissions(UserClaimsContext user, params string[] permissions)
        {
            if (user == null || permissions == null || permissions.Length == 0)
            {
                return false;
            }
            return permissions.All(permission => HasPermission(user, permission));
        }

        /// <summary>
        /// Get typed value from context dictionary
        /// Returns default(T) if key not found or type mismatch
        /// </summary>
        protected T? GetContextValue<T>(Dictionary<string, object> context, string key)
        {
            if (context == null || string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            if (context.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        /// <summary>
        /// Get typed value from context dictionary with default value
        /// Returns provided default if key not found or type mismatch
        /// </summary>
        protected T GetContextValue<T>(Dictionary<string, object> context, string key, T defaultValue)
        {
            if (context == null || string.IsNullOrWhiteSpace(key))
            {
                return defaultValue;
            }

            if (context.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get required typed value from context dictionary
        /// Throws ArgumentException if key not found or type mismatch
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when key not found or type mismatch</exception>
        protected T GetRequiredContextValue<T>(Dictionary<string, object> context, string key)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Context cannot be null");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            if (!context.TryGetValue(key, out var value))
            {
                throw new ArgumentException(
                    $"Required context key '{key}' not found. Available keys: {string.Join(", ", context.Keys)}",
                    nameof(key));
            }

            if (value is not T typedValue)
            {
                throw new ArgumentException(
                    $"Context key '{key}' has wrong type. Expected {typeof(T).Name}, but got {value?.GetType().Name ?? "null"}",
                    nameof(key));
            }

            return typedValue;
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

