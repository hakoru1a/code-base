using Shared.DTOs.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal to extract UserClaimsContext
    /// Centralized place for all user context extraction logic
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extract UserClaimsContext from ClaimsPrincipal
        /// Handles Keycloak JWT token structure with realm_access and resource_access
        /// </summary>
        public static UserClaimsContext ToUserClaimsContext(this ClaimsPrincipal? user)
        {
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                return CreateAnonymousContext();
            }

            var context = new UserClaimsContext
            {
                UserId = ExtractUserId(user),
                Roles = new List<string>(),
                Claims = new Dictionary<string, string>(),
                Permissions = new List<string>(),
                CustomAttributes = new Dictionary<string, object>()
            };

            // Extract roles from multiple sources
            ExtractRoles(user, context);

            // Extract all claims
            ExtractClaims(user, context);

            // Extract custom attributes
            ExtractCustomAttributes(user, context);

            return context;
        }

        private static UserClaimsContext CreateAnonymousContext()
        {
            return new UserClaimsContext
            {
                UserId = "anonymous",
                Roles = new List<string>(),
                Claims = new Dictionary<string, string>(),
                Permissions = new List<string>(),
                CustomAttributes = new Dictionary<string, object>()
            };
        }

        private static string ExtractUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("preferred_username")?.Value
                ?? "anonymous";
        }

        private static void ExtractRoles(ClaimsPrincipal user, UserClaimsContext context)
        {
            // 1. Extract roles from standard claims
            var standardRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            context.Roles.AddRange(standardRoles);

            // 2. Extract roles from Keycloak realm_access
            ExtractRealmRoles(user, context);

            // 3. Extract resource_access roles (client-specific roles)
            ExtractResourceRoles(user, context);

            // Remove duplicates
            context.Roles = context.Roles.Distinct().ToList();
        }

        private static void ExtractRealmRoles(ClaimsPrincipal user, UserClaimsContext context)
        {
            var realmAccessClaim = user.FindFirst("realm_access")?.Value;
            if (string.IsNullOrEmpty(realmAccessClaim))
                return;

            try
            {
                var realmAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(realmAccessClaim);
                if (realmAccess != null && realmAccess.TryGetValue("roles", out var rolesElement))
                {
                    var roles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                    if (roles != null)
                    {
                        context.Roles.AddRange(roles);
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors - don't let them break authentication
            }
        }

        private static void ExtractResourceRoles(ClaimsPrincipal user, UserClaimsContext context)
        {
            var resourceAccessClaim = user.FindFirst("resource_access")?.Value;
            if (string.IsNullOrEmpty(resourceAccessClaim))
                return;

            try
            {
                var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resourceAccessClaim);
                if (resourceAccess == null)
                    return;

                foreach (var resource in resourceAccess)
                {
                    if (resource.Value.TryGetProperty("roles", out var rolesElement))
                    {
                        var roles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                        if (roles != null)
                        {
                            // Add roles with resource prefix for namespace isolation
                            context.Roles.AddRange(roles.Select(r => $"{resource.Key}:{r}"));
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors
            }
        }

        private static void ExtractClaims(ClaimsPrincipal user, UserClaimsContext context)
        {
            foreach (var claim in user.Claims)
            {
                // Avoid duplicate keys - keep first occurrence
                if (!context.Claims.ContainsKey(claim.Type))
                {
                    context.Claims[claim.Type] = claim.Value;
                }

                // Extract permissions from custom claim
                if (claim.Type == "permissions" || claim.Type == "scope")
                {
                    var permissions = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    context.Permissions.AddRange(permissions);
                }
            }

            // Remove duplicate permissions
            context.Permissions = context.Permissions.Distinct().ToList();
        }

        private static void ExtractCustomAttributes(ClaimsPrincipal user, UserClaimsContext context)
        {
            // Extract common custom attributes
            ExtractStringAttribute(user, context, "department");
            ExtractStringAttribute(user, context, "location");
            ExtractStringAttribute(user, context, "region");
            ExtractStringAttribute(user, context, "team");
            ExtractStringAttribute(user, context, "cost_center");

            // Extract integer attributes
            ExtractIntAttribute(user, context, "clearance_level");

            // Extract any attribute starting with "attr:" or "custom:"
            foreach (var claim in user.Claims)
            {
                if (claim.Type.StartsWith("attr:") || claim.Type.StartsWith("custom:"))
                {
                    var key = claim.Type.Split(':', 2)[1];
                    if (!context.CustomAttributes.ContainsKey(key))
                    {
                        context.CustomAttributes[key] = claim.Value;
                    }
                }
            }
        }

        private static void ExtractStringAttribute(
            ClaimsPrincipal user,
            UserClaimsContext context,
            string attributeName)
        {
            var value = user.FindFirst(attributeName)?.Value;
            if (!string.IsNullOrEmpty(value))
            {
                context.CustomAttributes[attributeName] = value;
            }
        }

        private static void ExtractIntAttribute(
            ClaimsPrincipal user,
            UserClaimsContext context,
            string attributeName)
        {
            var value = user.FindFirst(attributeName)?.Value;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var intValue))
            {
                context.CustomAttributes[attributeName] = intValue;
            }
        }
    }
}

