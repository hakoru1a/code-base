using Infrastructure.Authorization.Interfaces;
using Infrastructure.Authorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Service for retrieving dynamic policy configuration
    /// Supports loading from: JWT Claims, Configuration File, Hardcoded Defaults
    /// </summary>
    public class PolicyConfigurationService : IPolicyConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PolicyConfigurationService> _logger;

        // Hardcoded defaults (lowest priority - only used if no other config exists)
        private readonly PolicyConfiguration _defaultConfig = new()
        {
            MaxPrice = 5_000_000m,  // Default max price for basic users
            MinPrice = null,
            AllowedCategories = null,
            ApprovalLimit = 10_000_000m  // Default approval limit
        };

        public PolicyConfigurationService(
            IConfiguration configuration,
            ILogger<PolicyConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public PolicyConfiguration GetRoleConfiguration(string role)
        {
            // Try to get role-specific config from appsettings.json
            // Example: "PolicyConfig:Roles:basic_user:MaxPrice": 5000000
            var section = _configuration.GetSection($"PolicyConfig:Roles:{role}");

            if (!section.Exists())
            {
                _logger.LogDebug("No configuration found for role: {Role}", role);
                return PolicyConfiguration.Empty();
            }

            var config = new PolicyConfiguration();

            // Parse MaxPrice
            if (decimal.TryParse(section["MaxPrice"], out var maxPrice))
            {
                config.MaxPrice = maxPrice;
            }

            // Parse MinPrice
            if (decimal.TryParse(section["MinPrice"], out var minPrice))
            {
                config.MinPrice = minPrice;
            }

            // Parse AllowedCategories (comma-separated)
            var categories = section["AllowedCategories"];
            if (!string.IsNullOrEmpty(categories))
            {
                config.AllowedCategories = categories
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();
            }

            // Parse ApprovalLimit
            if (decimal.TryParse(section["ApprovalLimit"], out var approvalLimit))
            {
                config.ApprovalLimit = approvalLimit;
            }

            _logger.LogDebug(
                "Loaded configuration for role {Role}: MaxPrice={MaxPrice}, AllowedCategories={Categories}",
                role,
                config.MaxPrice?.ToString("N0") ?? "None",
                config.AllowedCategories != null ? string.Join(", ", config.AllowedCategories) : "None");

            return config;
        }

        public PolicyConfiguration GetClaimsConfiguration(UserClaimsContext user)
        {
            var config = new PolicyConfiguration();
            var claimsUsed = new List<string>();

            // Extract policy configuration from JWT claims
            // Keycloak can inject these claims via mappers

            // 1. Max Price from claim "policy:max_price" or "max_price"
            if (TryGetDecimalClaim(user, "policy:max_price", out var maxPrice) ||
                TryGetDecimalClaim(user, "max_price", out maxPrice))
            {
                config.MaxPrice = maxPrice;
                claimsUsed.Add($"max_price={maxPrice:N0}");
            }

            // 2. Min Price from claim "policy:min_price" or "min_price"
            if (TryGetDecimalClaim(user, "policy:min_price", out var minPrice) ||
                TryGetDecimalClaim(user, "min_price", out minPrice))
            {
                config.MinPrice = minPrice;
                claimsUsed.Add($"min_price={minPrice:N0}");
            }

            // 3. Allowed Categories from claim "policy:allowed_categories" or "allowed_categories"
            if (TryGetStringClaim(user, "policy:allowed_categories", out var categoriesStr) ||
                TryGetStringClaim(user, "allowed_categories", out categoriesStr))
            {
                config.AllowedCategories = categoriesStr
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();
                claimsUsed.Add($"allowed_categories={categoriesStr}");
            }

            // 4. Approval Limit from claim "policy:approval_limit" or "approval_limit"
            if (TryGetDecimalClaim(user, "policy:approval_limit", out var approvalLimit) ||
                TryGetDecimalClaim(user, "approval_limit", out approvalLimit))
            {
                config.ApprovalLimit = approvalLimit;
                claimsUsed.Add($"approval_limit={approvalLimit:N0}");
            }

            // 5. Custom attributes - any claim starting with "policy:"
            config.CustomAttributes = new Dictionary<string, object>();
            foreach (var claim in user.Claims.Where(c => c.Key.StartsWith("policy:")))
            {
                var key = claim.Key.Substring("policy:".Length);
                config.CustomAttributes[key] = claim.Value;
            }

            if (claimsUsed.Any())
            {
                _logger.LogInformation(
                    "Loaded policy configuration from JWT claims for user {UserId}: {Claims}",
                    user.UserId,
                    string.Join(", ", claimsUsed));
            }

            return config;
        }

        public PolicyConfiguration GetEffectivePolicyConfig(UserClaimsContext user)
        {
            // Start with hardcoded defaults (lowest priority)
            var effectiveConfig = _defaultConfig;

            // Merge with role-based configuration from appsettings.json (medium priority)
            foreach (var role in user.Roles)
            {
                var roleConfig = GetRoleConfiguration(role);
                effectiveConfig = effectiveConfig.MergeWith(roleConfig);
            }

            // Merge with JWT claims configuration (highest priority)
            var claimsConfig = GetClaimsConfiguration(user);
            effectiveConfig = effectiveConfig.MergeWith(claimsConfig);

            _logger.LogDebug(
                "Effective policy config for user {UserId}: MaxPrice={MaxPrice}, MinPrice={MinPrice}, Categories={Categories}",
                user.UserId,
                effectiveConfig.MaxPrice?.ToString("N0") ?? "None",
                effectiveConfig.MinPrice?.ToString("N0") ?? "None",
                effectiveConfig.AllowedCategories != null ? string.Join(", ", effectiveConfig.AllowedCategories) : "All");

            return effectiveConfig;
        }

        public PolicyConfiguration GetDefaultConfiguration()
        {
            return _defaultConfig;
        }

        #region Helper Methods

        private bool TryGetDecimalClaim(UserClaimsContext user, string claimKey, out decimal value)
        {
            value = 0;
            if (user.Claims.TryGetValue(claimKey, out var claimValue))
            {
                if (decimal.TryParse(claimValue, out value))
                {
                    return true;
                }

                _logger.LogWarning(
                    "Failed to parse claim {ClaimKey}='{ClaimValue}' as decimal for user {UserId}",
                    claimKey, claimValue, user.UserId);
            }
            return false;
        }

        private bool TryGetStringClaim(UserClaimsContext user, string claimKey, out string value)
        {
            value = string.Empty;
            if (user.Claims.TryGetValue(claimKey, out var claimValue) && !string.IsNullOrEmpty(claimValue))
            {
                value = claimValue;
                return true;
            }
            return false;
        }

        #endregion
    }
}

