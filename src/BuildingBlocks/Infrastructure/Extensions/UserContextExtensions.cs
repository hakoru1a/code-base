using Contracts.Identity;
using Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shared.DTOs.Authorization;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for registering user context services
    /// Replaces the complex UserContextAccessor + ClaimsPrincipalExtensions setup
    /// </summary>
    public static class UserContextExtensions
    {
        /// <summary>
        /// Add unified user context service
        /// This replaces UserContextAccessor and simplifies user context access
        /// </summary>
        public static IServiceCollection AddUserContextService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContextService<UserClaimsContext>, UserContextService>();
            return services;
        }
    }
}