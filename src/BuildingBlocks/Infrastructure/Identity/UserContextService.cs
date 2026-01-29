using Contracts.Identity;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Shared.DTOs.Authorization;
using System.Security.Claims;

namespace Infrastructure.Identity
{
    /// <summary>
    /// Unified implementation of IUserContextService
    /// Consolidates all user context logic into a single, simple service
    /// </summary>
    public class UserContextService : IUserContextService<UserClaimsContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserClaimsContext GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                return CreateAnonymousContext();
            }

            return new UserClaimsContext
            {
                UserId = GetUserId(user),
                Roles = ExtractAllRoles(user),
                Claims = ExtractAllClaims(user),
                Permissions = ExtractPermissions(user),
                CustomAttributes = ExtractCustomAttributes(user)
            };
        }

        public string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return GetUserId(user);
        }

        public bool HasRole(string role)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.IsInRole(role) ?? false;
        }

        public bool HasAnyRole(params string[] roles)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;

            return roles.Any(role => user.IsInRole(role));
        }

        public bool IsAuthenticated()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated ?? false;
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

        private static string GetUserId(ClaimsPrincipal? user)
        {
            if (user == null) return "anonymous";

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("preferred_username")?.Value
                ?? "anonymous";
        }

        private static List<string> ExtractAllRoles(ClaimsPrincipal user)
        {
            var roles = new List<string>();

            // 1. Standard roles (already mapped to ClaimTypes.Role by JWT middleware)
            var standardRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            roles.AddRange(standardRoles);

            // 2. Extract realm roles directly from claims (backup)
            var realmRoles = KeycloakClaimsHelper.ExtractRealmRolesFromPrincipal(user);
            roles.AddRange(realmRoles);

            // 3. Extract resource roles with prefix for namespace isolation
            var resourceRoles = KeycloakClaimsHelper.ExtractResourceRolesFromPrincipal(
                user, clientId: null, addPrefix: true);
            roles.AddRange(resourceRoles);

            // Remove duplicates and return
            return roles.Distinct().ToList();
        }

        private static Dictionary<string, string> ExtractAllClaims(ClaimsPrincipal user)
        {
            var claimsDict = new Dictionary<string, string>();

            foreach (var claim in user.Claims)
            {
                // Avoid duplicate keys - keep first occurrence
                if (!claimsDict.ContainsKey(claim.Type))
                {
                    claimsDict[claim.Type] = claim.Value;
                }
            }

            return claimsDict;
        }

        private static List<string> ExtractPermissions(ClaimsPrincipal user)
        {
            var permissions = new List<string>();

            // Extract permissions from custom claims
            foreach (var claim in user.Claims)
            {
                if (claim.Type == "permissions" || claim.Type == "scope")
                {
                    var perms = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    permissions.AddRange(perms);
                }
            }

            // Remove duplicates
            return permissions.Distinct().ToList();
        }

        private static Dictionary<string, object> ExtractCustomAttributes(ClaimsPrincipal user)
        {
            var attributes = new Dictionary<string, object>();

            // Extract common custom attributes
            ExtractStringAttribute(user, attributes, "department");
            ExtractStringAttribute(user, attributes, "location");
            ExtractStringAttribute(user, attributes, "region");
            ExtractStringAttribute(user, attributes, "team");
            ExtractStringAttribute(user, attributes, "cost_center");
            ExtractStringAttribute(user, attributes, "avatar_url");

            // Extract integer attributes
            ExtractIntAttribute(user, attributes, "clearance_level");

            // Extract any attribute starting with "attr:" or "custom:"
            foreach (var claim in user.Claims)
            {
                if (claim.Type.StartsWith("attr:") || claim.Type.StartsWith("custom:"))
                {
                    var key = claim.Type.Split(':', 2)[1];
                    if (!attributes.ContainsKey(key))
                    {
                        attributes[key] = claim.Value;
                    }
                }
            }

            return attributes;
        }

        private static void ExtractStringAttribute(
            ClaimsPrincipal user,
            Dictionary<string, object> attributes,
            string attributeName)
        {
            var value = user.FindFirst(attributeName)?.Value;
            if (!string.IsNullOrEmpty(value))
            {
                attributes[attributeName] = value;
            }
        }

        private static void ExtractIntAttribute(
            ClaimsPrincipal user,
            Dictionary<string, object> attributes,
            string attributeName)
        {
            var value = user.FindFirst(attributeName)?.Value;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var intValue))
            {
                attributes[attributeName] = intValue;
            }
        }
    }
}