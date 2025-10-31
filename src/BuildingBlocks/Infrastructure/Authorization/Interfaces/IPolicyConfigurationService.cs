using Infrastructure.Authorization.Models;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization.Interfaces
{
    /// <summary>
    /// Service for retrieving dynamic policy configuration from JWT claims only
    /// Priority: JWT Claims > Hardcoded Defaults
    /// </summary>
    public interface IPolicyConfigurationService
    {
        /// <summary>
        /// Get policy configuration for a specific role (always returns empty - kept for interface compatibility)
        /// All policy configuration is read from JWT claims only
        /// </summary>
        PolicyConfiguration GetRoleConfiguration(string role);

        /// <summary>
        /// Extract policy configuration from JWT claims
        /// Claims format:
        /// - policy:max_price = "5000000"
        /// - policy:min_price = "100000"
        /// - policy:allowed_categories = "electronics,books,clothing"
        /// - policy:approval_limit = "10000000"
        /// </summary>
        PolicyConfiguration GetClaimsConfiguration(UserClaimsContext user);

        /// <summary>
        /// Get effective configuration for a user (merges role config + claims)
        /// Priority: Claims override Role Config override Defaults
        /// </summary>
        PolicyConfiguration GetEffectivePolicyConfig(UserClaimsContext user);

        /// <summary>
        /// Get hardcoded default configuration (fallback)
        /// </summary>
        PolicyConfiguration GetDefaultConfiguration();
    }
}

