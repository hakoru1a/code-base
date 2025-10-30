using Microsoft.AspNetCore.Http;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization.Interfaces
{
    /// <summary>
    /// Service to extract user context from HTTP context
    /// </summary>
    public interface IUserContextAccessor
    {
        /// <summary>
        /// Get current user context from HTTP context
        /// </summary>
        UserClaimsContext GetCurrentUserContext();
    }
}

