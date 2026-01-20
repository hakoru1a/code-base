using System.Security.Claims;
using System.Text.Json;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// Helper class để extract roles từ Keycloak JWT claims
    /// Centralized logic để tránh code duplication
    /// </summary>
    public static class KeycloakClaimsHelper
    {
        /// <summary>
        /// Extract realm roles từ realm_access claim
        /// </summary>
        /// <param name="claims">JWT claims</param>
        /// <returns>List of realm roles</returns>
        public static List<string> ExtractRealmRoles(IEnumerable<Claim> claims)
        {
            var roles = new List<string>();
            var realmAccessClaim = claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;

            if (string.IsNullOrEmpty(realmAccessClaim))
                return roles;

            try
            {
                var realmAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(realmAccessClaim);
                if (realmAccess != null && realmAccess.TryGetValue("roles", out var rolesElement))
                {
                    var realmRoles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                    if (realmRoles != null)
                    {
                        roles.AddRange(realmRoles);
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors - don't let them break authentication
            }

            return roles;
        }

        /// <summary>
        /// Extract resource roles từ resource_access claim
        /// </summary>
        /// <param name="claims">JWT claims</param>
        /// <param name="clientId">Optional: chỉ extract roles từ client này. Nếu null thì extract tất cả</param>
        /// <param name="addPrefix">Nếu true, thêm prefix "{clientId}:" cho mỗi role</param>
        /// <returns>List of resource roles</returns>
        public static List<string> ExtractResourceRoles(
            IEnumerable<Claim> claims,
            string? clientId = null,
            bool addPrefix = false)
        {
            var roles = new List<string>();
            var resourceAccessClaim = claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;

            if (string.IsNullOrEmpty(resourceAccessClaim))
                return roles;

            try
            {
                var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resourceAccessClaim);
                if (resourceAccess == null)
                    return roles;

                // Nếu có clientId, chỉ extract từ client đó
                if (!string.IsNullOrEmpty(clientId))
                {
                    if (resourceAccess.TryGetValue(clientId, out var clientRoles))
                    {
                        if (clientRoles.TryGetProperty("roles", out var rolesElement))
                        {
                            var resourceRoles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                            if (resourceRoles != null)
                            {
                                roles.AddRange(addPrefix
                                    ? resourceRoles.Select(r => $"{clientId}:{r}")
                                    : resourceRoles);
                            }
                        }
                    }
                }
                else
                {
                    // Extract từ tất cả clients
                    foreach (var resource in resourceAccess)
                    {
                        if (resource.Value.TryGetProperty("roles", out var rolesElement))
                        {
                            var resourceRoles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                            if (resourceRoles != null)
                            {
                                roles.AddRange(addPrefix
                                    ? resourceRoles.Select(r => $"{resource.Key}:{r}")
                                    : resourceRoles);
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors - don't let them break authentication
            }

            return roles;
        }

        /// <summary>
        /// Extract tất cả roles từ JWT claims (realm + resource + standard)
        /// </summary>
        /// <param name="claims">JWT claims</param>
        /// <param name="clientId">Optional: chỉ extract resource roles từ client này</param>
        /// <param name="addResourcePrefix">Nếu true, thêm prefix cho resource roles</param>
        /// <returns>List of all roles</returns>
        public static List<string> ExtractAllRoles(
            IEnumerable<Claim> claims,
            string? clientId = null,
            bool addResourcePrefix = false)
        {
            var roles = new List<string>();

            // 1. Realm roles
            roles.AddRange(ExtractRealmRoles(claims));

            // 2. Resource roles
            roles.AddRange(ExtractResourceRoles(claims, clientId, addResourcePrefix));

            // 3. Standard role claims (đã được map vào ClaimTypes.Role)
            roles.AddRange(claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value));

            return roles.Distinct().Where(r => !string.IsNullOrEmpty(r)).ToList();
        }

        /// <summary>
        /// Extract realm roles từ ClaimsPrincipal (wrapper method)
        /// </summary>
        public static List<string> ExtractRealmRolesFromPrincipal(ClaimsPrincipal? user)
        {
            if (user == null)
                return new List<string>();

            return ExtractRealmRoles(user.Claims);
        }

        /// <summary>
        /// Extract resource roles từ ClaimsPrincipal (wrapper method)
        /// </summary>
        public static List<string> ExtractResourceRolesFromPrincipal(
            ClaimsPrincipal? user,
            string? clientId = null,
            bool addPrefix = false)
        {
            if (user == null)
                return new List<string>();

            return ExtractResourceRoles(user.Claims, clientId, addPrefix);
        }
    }
}
