using Shared.DTOs.Authorization;

namespace Contracts.Identity
{
    /// <summary>
    /// Unified service for accessing user context across Gateway and Services
    /// Replaces the complex chain of UserClaimsCache + ClaimsPrincipalExtensions + UserContextAccessor
    /// </summary>
    public interface IUserContextService
    {
        /// <summary>
        /// Get current user context from HTTP context
        /// Returns anonymous context if user is not authenticated
        /// </summary>
        UserClaimsContext GetCurrentUser();

        /// <summary>
        /// Get user ID from current context
        /// Returns "anonymous" if user is not authenticated
        /// </summary>
        string GetCurrentUserId();

        /// <summary>
        /// Check if current user has specific role
        /// </summary>
        bool HasRole(string role);

        /// <summary>
        /// Check if current user has any of the specified roles
        /// </summary>
        bool HasAnyRole(params string[] roles);

        /// <summary>
        /// Check if current user is authenticated
        /// </summary>
        bool IsAuthenticated();
    }
}