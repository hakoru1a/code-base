using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Configurations;
using System.Security.Claims;
using System.Text.Json;
using Serilog;
using System.Linq;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for configuring Keycloak authentication
    /// </summary>
    public static class KeycloakAuthenticationExtensions
    {
        /// <summary>
        /// Add Keycloak JWT authentication for RBAC at Gateway level
        /// </summary>
        public static IServiceCollection AddKeycloakAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var keycloakSettings = configuration
                .GetSection(KeycloakSettings.SectionName)
                .Get<KeycloakSettings>();

            if (keycloakSettings == null)
            {
                throw new InvalidOperationException(
                    "Keycloak settings not found in configuration");
            }

            services.AddSingleton(keycloakSettings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = $"{keycloakSettings.Authority}/realms/{keycloakSettings.Realm}";
                options.Audience = keycloakSettings.ClientId;
                options.RequireHttpsMetadata = keycloakSettings.RequireHttpsMetadata;
                options.MetadataAddress = keycloakSettings.MetadataAddress;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = keycloakSettings.ValidateIssuer,
                    ValidateAudience = keycloakSettings.ValidateAudience,
                    ValidateLifetime = keycloakSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = $"{keycloakSettings.Authority}/realms/{keycloakSettings.Realm}",
                    ValidAudiences = new[] { keycloakSettings.ClientId, "account" },
                    ClockSkew = TimeSpan.FromMinutes(5),
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        var hasToken = !string.IsNullOrEmpty(context.Token);
                        var tokenPreview = hasToken && context.Token!.Length > 20
                            ? context.Token.Substring(0, 20) + "..."
                            : context.Token ?? "null";

                        Log.Information(
                            "[JWT] OnMessageReceived - Path: {Path}, Method: {Method}\n" +
                            "  Authorization Header: {AuthHeader}\n" +
                            "  Token Present: {HasToken}\n" +
                            "  Token Preview: {TokenPreview}",
                            context.Request.Path,
                            context.Request.Method,
                            string.IsNullOrEmpty(authHeader) ? "NOT FOUND" : authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...",
                            hasToken,
                            tokenPreview);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Extract and map roles from Keycloak token
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            MapKeycloakRoles(identity, keycloakSettings);
                            Log.Information(
                                "[JWT] OnTokenValidated - User: {Username}, IsAuthenticated: {IsAuthenticated}, Roles: {Roles}",
                                identity.Name,
                                identity.IsAuthenticated,
                                string.Join(", ", identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)));
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        Log.Error(
                            context.Exception,
                            "[JWT] OnAuthenticationFailed - Path: {Path}, Method: {Method}\n" +
                            "  Error: {Error}\n" +
                            "  InnerException: {InnerException}\n" +
                            "  Authorization Header: {AuthHeader}",
                            context.Request.Path,
                            context.Request.Method,
                            context.Exception?.Message ?? "Unknown error",
                            context.Exception?.InnerException?.Message ?? "None",
                            string.IsNullOrEmpty(authHeader) ? "NOT FOUND" : authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        Log.Warning(
                            "[JWT] OnChallenge - Path: {Path}, Method: {Method}\n" +
                            "  Error: {Error}\n" +
                            "  ErrorDescription: {ErrorDescription}\n" +
                            "  ErrorUri: {ErrorUri}\n" +
                            "  Authorization Header: {AuthHeader}\n" +
                            "  AuthenticateResult: {AuthenticateResult}",
                            context.Request.Path,
                            context.Request.Method,
                            context.Error ?? "null",
                            context.ErrorDescription ?? "null",
                            context.ErrorUri ?? "null",
                            string.IsNullOrEmpty(authHeader) ? "NOT FOUND" : authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...",
                            context.AuthenticateFailure?.Message ?? "null");
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Add authorization policies for RBAC at Gateway level
        /// These policies provide coarse-grained access control based on roles
        /// For fine-grained control, use PBAC at service level
        /// </summary>
        public static IServiceCollection AddKeycloakAuthorization(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // ===== ROLE-BASED POLICIES (RBAC) =====
                // These provide coarse-grained access control at the gateway/controller level

                options.AddPolicy(Shared.Identity.PolicyNames.Rbac.AdminOnly, policy =>
                    policy.RequireRole(Shared.Identity.Roles.Admin));

                options.AddPolicy(Shared.Identity.PolicyNames.Rbac.ManagerOrAdmin, policy =>
                    policy.RequireRole(
                        Shared.Identity.Roles.Admin,
                        Shared.Identity.Roles.Manager,
                        Shared.Identity.Roles.ProductManager));

                options.AddPolicy(Shared.Identity.PolicyNames.Rbac.AuthenticatedUser, policy =>
                    policy.RequireAuthenticatedUser());

                options.AddPolicy(Shared.Identity.PolicyNames.Rbac.PremiumUser, policy =>
                    policy.RequireRole(
                        Shared.Identity.Roles.PremiumUser,
                        Shared.Identity.Roles.Admin));

                options.AddPolicy(Shared.Identity.PolicyNames.Rbac.BasicUser, policy =>
                    policy.RequireRole(
                        Shared.Identity.Roles.BasicUser,
                        Shared.Identity.Roles.PremiumUser,
                        Shared.Identity.Roles.Admin));

                // ===== HYBRID POLICIES (Role + Permission) =====
                // These combine roles and permissions for flexible access control
                // Uses helper method for maintainability and consistency
                // 
                // Logic: User can access if they have the required permission OR have one of the allowed roles
                // This provides flexibility: fine-grained control via permissions, quick access via roles

                // CanViewProducts: Permission "product:view" OR roles "admin"/"manager"
                AddHybridPolicy(
                    options,
                    Shared.Identity.PolicyNames.Hybrid.Product.CanView,
                    Shared.Identity.Permissions.Product.View,
                    Shared.Identity.Roles.Admin,
                    Shared.Identity.Roles.Manager);

                // CanCreateProducts: Permission "product:create" OR roles "admin"/"product_manager"
                AddHybridPolicy(
                    options,
                    Shared.Identity.PolicyNames.Hybrid.Product.CanCreate,
                    Shared.Identity.Permissions.Product.Create,
                    Shared.Identity.Roles.Admin,
                    Shared.Identity.Roles.ProductManager);

                // CanUpdateProducts: Permission "product:update" OR roles "admin"/"product_manager"
                AddHybridPolicy(
                    options,
                    Shared.Identity.PolicyNames.Hybrid.Product.CanUpdate,
                    Shared.Identity.Permissions.Product.Update,
                    Shared.Identity.Roles.Admin,
                    Shared.Identity.Roles.ProductManager);

                // CanDeleteProducts: Permission "product:delete" OR role "admin" (more restrictive)
                AddHybridPolicy(
                    options,
                    Shared.Identity.PolicyNames.Hybrid.Product.CanDelete,
                    Shared.Identity.Permissions.Product.Delete,
                    Shared.Identity.Roles.Admin);

                // CanViewCategories: Permission "category:view" OR roles "admin"/"manager"
                AddHybridPolicy(
                    options,
                    Shared.Identity.PolicyNames.Hybrid.Category.CanView,
                    Shared.Identity.Permissions.Category.View,
                    Shared.Identity.Roles.Admin,
                    Shared.Identity.Roles.Manager);
            });

            return services;
        }

        /// <summary>
        /// Helper method to create flexible hybrid policies that combine permissions and roles.
        /// This allows for flexible access control where users can access via either:
        /// 1. Having the required permission (PBAC) - OR
        /// 2. Having one of the specified roles (RBAC)
        /// 
        /// Usage examples:
        /// <code>
        /// // Policy with permission and roles
        /// AddHybridPolicy(options, PolicyNames.Hybrid.Product.CanView, 
        ///     Permissions.Product.View, Roles.Admin, Roles.Manager);
        /// 
        /// // Policy with only roles (no permission check)
        /// AddHybridPolicy(options, "CanManageSystem", 
        ///     requiredPermission: null, Roles.Admin);
        /// 
        /// // Policy with only permission (no roles)
        /// AddHybridPolicy(options, "CanViewReports", 
        ///     Permissions.User.View);
        /// </code>
        /// </summary>
        /// <param name="options">Authorization options to add the policy to</param>
        /// <param name="policyName">Name of the policy (should use constants from PolicyNames.Hybrid)</param>
        /// <param name="requiredPermission">Required permission (null to skip permission check)</param>
        /// <param name="allowedRoles">Roles that can access without the permission (params array)</param>
        /// <example>
        /// <code>
        /// // User can access if they have "product:view" permission OR have "admin"/"manager" role
        /// AddHybridPolicy(options, PolicyNames.Hybrid.Product.CanView,
        ///     Permissions.Product.View, Roles.Admin, Roles.Manager);
        /// </code>
        /// </example>
        private static void AddHybridPolicy(
            AuthorizationOptions options,
            string policyName,
            string? requiredPermission = null,
            params string[] allowedRoles)
        {
            options.AddPolicy(policyName, policy =>
                policy.RequireAssertion(context =>
                {
                    // Check if user has required permission (PBAC)
                    bool hasPermission = false;
                    if (!string.IsNullOrEmpty(requiredPermission))
                    {
                        // Debug: Log all claims
                        var allClaims = context.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                        Log.Debug(
                            "[POLICY DEBUG] Policy: {PolicyName}, RequiredPermission: {RequiredPermission}\n" +
                            "  User: {Username}, IsAuthenticated: {IsAuthenticated}\n" +
                            "  All Claims ({Count}): {Claims}",
                            policyName,
                            requiredPermission,
                            context.User.Identity?.Name ?? "Anonymous",
                            context.User.Identity?.IsAuthenticated ?? false,
                            allClaims.Count,
                            string.Join(" | ", allClaims));

                        // Debug: Log permission claims specifically
                        var permissionClaims = context.User.Claims
                            .Where(c => c.Type == "permissions")
                            .Select(c => c.Value)
                            .ToList();
                        Log.Debug(
                            "[POLICY DEBUG] Permission Claims Found: {Count}\n" +
                            "  Permission Values: {PermissionValues}",
                            permissionClaims.Count,
                            string.Join(" | ", permissionClaims));

                        // Check permission
                        hasPermission = context.User.HasClaim(c =>
                            c.Type == "permissions" &&
                            c.Value.Contains(requiredPermission, StringComparison.OrdinalIgnoreCase));

                        Log.Debug(
                            "[POLICY DEBUG] Permission Check Result: {HasPermission}\n" +
                            "  Required: {RequiredPermission}\n" +
                            "  Found in Claims: {Found}",
                            hasPermission,
                            requiredPermission,
                            hasPermission ? "YES" : "NO");
                    }

                    // Check if user has any of the allowed roles (RBAC)
                    bool hasRole = allowedRoles.Length > 0 &&
                        allowedRoles.Any(role => context.User.IsInRole(role));

                    // Debug: Log role check
                    if (allowedRoles.Length > 0)
                    {
                        var userRoles = context.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();
                        Log.Debug(
                            "[POLICY DEBUG] Role Check\n" +
                            "  Allowed Roles: {AllowedRoles}\n" +
                            "  User Roles: {UserRoles}\n" +
                            "  Has Role: {HasRole}",
                            string.Join(", ", allowedRoles),
                            string.Join(", ", userRoles),
                            hasRole);
                    }

                    // Grant access if user has permission OR has one of the allowed roles
                    bool result = hasPermission || hasRole;
                    Log.Debug(
                        "[POLICY DEBUG] Final Result for Policy '{PolicyName}': {Result}\n" +
                        "  HasPermission: {HasPermission}, HasRole: {HasRole}",
                        policyName,
                        result ? "ALLOWED" : "DENIED",
                        hasPermission,
                        hasRole);

                    return result;
                }));
        }

        /// <summary>
        /// Map Keycloak roles to ClaimsIdentity
        /// </summary>
        private static void MapKeycloakRoles(ClaimsIdentity identity, KeycloakSettings settings)
        {
            // Extract realm roles
            var realmAccessClaim = identity.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                try
                {
                    var realmAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        realmAccessClaim.Value);

                    if (realmAccess != null && realmAccess.TryGetValue("roles", out var rolesElement))
                    {
                        var roles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
                        if (roles != null)
                        {
                            foreach (var role in roles)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to parse realm_access");
                }
            }

            // Extract resource (client) roles if enabled
            if (settings.UseResourceRoles)
            {
                var resourceAccessClaim = identity.FindFirst("resource_access");
                if (resourceAccessClaim != null)
                {
                    try
                    {
                        var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                            resourceAccessClaim.Value);

                        if (resourceAccess != null)
                        {
                            // Add client-specific roles
                            if (resourceAccess.TryGetValue(settings.ClientId, out var clientRoles))
                            {
                                if (clientRoles.TryGetProperty("roles", out var rolesElement))
                                {
                                    var roles = JsonSerializer.Deserialize<List<string>>(
                                        rolesElement.GetRawText());

                                    if (roles != null)
                                    {
                                        foreach (var role in roles)
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to parse resource_access");
                    }
                }
            }

            // Extract permissions from scope
            var scopeClaim = identity.FindFirst("scope");
            if (scopeClaim != null)
            {
                identity.AddClaim(new Claim("permissions", scopeClaim.Value));
            }
        }
    }
}

