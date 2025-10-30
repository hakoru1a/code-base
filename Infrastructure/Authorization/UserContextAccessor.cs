using Infrastructure.Authorization.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Default implementation of IUserContextAccessor
    /// Provides access to current user context from HTTP context
    /// </summary>
    public class UserContextAccessor : IUserContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get current user context from HTTP context
        /// Returns anonymous context if user is not authenticated
        /// </summary>
        public UserClaimsContext GetCurrentUserContext()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user.ToUserClaimsContext();
        }
    }
}

