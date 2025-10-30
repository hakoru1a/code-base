using Infrastructure.Authorization.Interfaces;
using Microsoft.AspNetCore.Http;
using Shared.DTOs.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Default implementation of IUserContextAccessor
    /// </summary>
    public class UserContextAccessor : IUserContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserClaimsContext GetCurrentUserContext()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
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

            return ExtractUserContext(user);
        }

        private UserClaimsContext ExtractUserContext(ClaimsPrincipal user)
        {
            var context = new UserClaimsContext
            {
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst("preferred_username")?.Value
                    ?? "anonymous",
                Roles = new List<string>(),
                Claims = new Dictionary<string, string>(),
                Permissions = new List<string>(),
                CustomAttributes = new Dictionary<string, object>()
            };

            // Extract roles from standard claims
            context.Roles.AddRange(user.FindAll(ClaimTypes.Role).Select(c => c.Value));

            // Extract roles from Keycloak realm_access
            var realmAccessClaim = user.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmAccessClaim))
            {
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
                catch
                {
                    // Ignore parsing errors
                }
            }

            // Extract resource_access roles (client-specific roles)
            var resourceAccessClaim = user.FindFirst("resource_access")?.Value;
            if (!string.IsNullOrEmpty(resourceAccessClaim))
            {
                try
                {
                    var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(resourceAccessClaim);
                    if (resourceAccess != null)
                    {
                        foreach (var resource in resourceAccess)
                        {
                            if (resource.Value.TryGetProperty("roles", out var rolesElement))
                            {
                                var roles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                                if (roles != null)
                                {
                                    context.Roles.AddRange(roles.Select(r => $"{resource.Key}:{r}"));
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            // Extract all claims
            foreach (var claim in user.Claims)
            {
                context.Claims[claim.Type] = claim.Value;

                // Extract permissions from custom claim
                if (claim.Type == "permissions" || claim.Type == "scope")
                {
                    context.Permissions.AddRange(claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
            }

            // Extract custom attributes (e.g., department, location, etc.)
            var department = user.FindFirst("department")?.Value;
            if (!string.IsNullOrEmpty(department))
            {
                context.CustomAttributes["department"] = department;
            }

            var location = user.FindFirst("location")?.Value;
            if (!string.IsNullOrEmpty(location))
            {
                context.CustomAttributes["location"] = location;
            }

            var clearanceLevel = user.FindFirst("clearance_level")?.Value;
            if (!string.IsNullOrEmpty(clearanceLevel) && int.TryParse(clearanceLevel, out var level))
            {
                context.CustomAttributes["clearance_level"] = level;
            }

            return context;
        }
    }
}

