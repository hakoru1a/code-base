using Infrastructure.Authorization.Interfaces;
using Infrastructure.Authorization.Models;
using Microsoft.Extensions.Logging;
using Shared.DTOs.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Service for retrieving dynamic policy configuration from JWT claims only
    /// Priority: JWT Claims > Hardcoded Defaults
    /// </summary>
    public class PolicyConfigurationService : IPolicyConfigurationService
    {
        private readonly ILogger<PolicyConfigurationService> _logger;

        // Hardcoded defaults (lowest priority - only used if no other config exists)
        private readonly PolicyConfiguration _defaultConfig = new()
        {
            MaxPrice = 0m,  // Default max price for basic users
            MinPrice = null,
            AllowedCategories = null,
            ApprovalLimit = 0m  // Default approval limit
        };

        public PolicyConfigurationService(ILogger<PolicyConfigurationService> logger)
        {
            _logger = logger;
        }

        public PolicyConfiguration GetRoleConfiguration(string role)
        {
            // Policy configuration is always read from JWT claims, not from appsettings
            // This method returns empty configuration to maintain interface compatibility
            _logger.LogDebug("Policy configuration for role {Role} will be read from JWT claims only", role);
            return PolicyConfiguration.Empty();
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
            // Get policy configuration directly from JWT claims only
            var config = GetClaimsConfiguration(user);

            _logger.LogDebug(
                "Policy config for user {UserId}: MaxPrice={MaxPrice}, MinPrice={MinPrice}, Categories={Categories}",
                user.UserId,
                config.MaxPrice?.ToString("N0") ?? "None",
                config.MinPrice?.ToString("N0") ?? "None",
                config.AllowedCategories != null ? string.Join(", ", config.AllowedCategories) : "All");

            return config;
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

