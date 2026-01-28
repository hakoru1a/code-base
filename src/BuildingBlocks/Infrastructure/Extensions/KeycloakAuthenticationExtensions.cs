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
using Infrastructure.Helpers;

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
                    ValidAudiences = new[] {
                        keycloakSettings.ClientId,
                        "account"  // Default Keycloak audience
                    },
                    ClockSkew = TimeSpan.FromMinutes(2), // Reduced from 5 to 2 minutes for better security
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                    // Enhanced validation
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    RequireAudience = keycloakSettings.ValidateAudience
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
                    OnTokenValidated = async context =>
                    {
                        // Enhanced token validation
                        var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                        if (token != null)
                        {
                            // 1. Validate audience more strictly
                            if (!ValidateTokenAudience(token, keycloakSettings.ClientId))
                            {
                                Log.Warning("[JWT] Token validation failed: Invalid audience. Expected: {ExpectedAudience}, Found: {ActualAudiences}",
                                    keycloakSettings.ClientId,
                                    string.Join(", ", token.Audiences));
                                context.Fail("Invalid token audience");
                                return;
                            }

                            // 2. Check token revocation status (optional, can be expensive)
                            if (await IsTokenRevokedAsync(token.RawData, keycloakSettings))
                            {
                                Log.Warning("[JWT] Token validation failed: Token has been revoked. TokenId: {TokenId}",
                                    token.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? "unknown");
                                context.Fail("Token has been revoked");
                                return;
                            }

                            // 3. Additional security checks
                            if (!ValidateTokenSecurityClaims(token))
                            {
                                Log.Warning("[JWT] Token validation failed: Security claims validation failed");
                                context.Fail("Token security validation failed");
                                return;
                            }
                        }

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
                // Configure all policies from centralized configuration file
                AuthorizationPolicyConfiguration.ConfigurePolicies(options);
            });

            return services;
        }

        /// <summary>
        /// Add simple JWT authentication for microservices
        /// This validates JWT tokens forwarded from the Gateway
        /// Gateway handles full Keycloak flow, microservices only validate tokens
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(
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
                    ValidAudiences = new[] {
                        keycloakSettings.ClientId,
                        "account"  // Default Keycloak audience
                    },
                    ClockSkew = TimeSpan.FromMinutes(2),
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    RequireAudience = keycloakSettings.ValidateAudience
                };
            });

            return services;
        }

        /// <summary>
        /// Add basic authorization services
        /// This sets up the authorization infrastructure for use with policies
        /// </summary>
        public static IServiceCollection AddBasicAuthorization(
            this IServiceCollection services)
        {
            services.AddAuthorization();
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
        internal static void AddHybridPolicy(
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

                        // Debug: Log permission claims from resource_access roles
                        var permissionClaims = context.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role && c.Value.Contains(":"))
                            .Select(c => c.Value)
                            .ToList();
                        Log.Debug(
                            "[POLICY DEBUG] Permission Claims Found: {Count}\n" +
                            "  Permission Values: {PermissionValues}",
                            permissionClaims.Count,
                            string.Join(" | ", permissionClaims));

                        // Check permission in ClaimTypes.Role (from resource_access)
                        hasPermission = context.User.HasClaim(c =>
                            c.Type == ClaimTypes.Role &&
                            c.Value.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase));

                        Log.Information(
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
        /// Validate token audience more strictly
        /// </summary>
        private static bool ValidateTokenAudience(System.IdentityModel.Tokens.Jwt.JwtSecurityToken token, string expectedClientId)
        {
            var audiences = token.Audiences.ToList();

            // Check if expected client ID is in audiences
            if (audiences.Contains(expectedClientId))
                return true;

            // Check for account audience (default Keycloak)
            if (audiences.Contains("account"))
                return true;

            return false;
        }

        /// <summary>
        /// Check if token has been revoked (optional - can be expensive)
        /// This method can be disabled for performance by returning false
        /// </summary>
        private static async Task<bool> IsTokenRevokedAsync(string tokenString, KeycloakSettings settings)
        {
            try
            {
                // For performance, we can cache revocation status
                // In production, you might want to check against a revocation list or call Keycloak introspection endpoint
                // For now, we'll implement a simple cache-based approach

                // TODO: Implement actual revocation check with Keycloak introspection endpoint
                // This is a placeholder that always returns false (not revoked)
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[JWT] Failed to check token revocation status, assuming token is valid");
                return false; // Fail open for availability
            }
        }

        /// <summary>
        /// Validate additional security claims in token
        /// </summary>
        private static bool ValidateTokenSecurityClaims(System.IdentityModel.Tokens.Jwt.JwtSecurityToken token)
        {
            try
            {
                // 1. Check if token has required claims
                var requiredClaims = new[] { "sub", "iat", "exp", "iss" };
                foreach (var claimType in requiredClaims)
                {
                    if (!token.Claims.Any(c => c.Type == claimType))
                    {
                        Log.Warning("[JWT] Missing required claim: {ClaimType}", claimType);
                        return false;
                    }
                }

                // 2. Check token age (not too old when issued)
                var iatClaim = token.Claims.FirstOrDefault(c => c.Type == "iat");
                if (iatClaim != null && long.TryParse(iatClaim.Value, out var iat))
                {
                    var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
                    var maxAge = TimeSpan.FromHours(24); // Token shouldn't be older than 24 hours when issued

                    if (DateTime.UtcNow - issuedAt > maxAge)
                    {
                        Log.Warning("[JWT] Token is too old. IssuedAt: {IssuedAt}, MaxAge: {MaxAge}", issuedAt, maxAge);
                        return false;
                    }
                }

                // 3. Check subject is not empty
                var subClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");
                if (subClaim == null || string.IsNullOrWhiteSpace(subClaim.Value))
                {
                    Log.Warning("[JWT] Invalid or missing subject claim");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[JWT] Failed to validate token security claims");
                return false;
            }
        }

        /// <summary>
        /// Map Keycloak roles to ClaimsIdentity
        /// Simplified version - only maps essential roles to ClaimTypes.Role
        /// Additional role extraction is handled by UserContextService
        /// </summary>
        private static void MapKeycloakRoles(ClaimsIdentity identity, KeycloakSettings settings)
        {
            // Extract realm roles và add vào ClaimTypes.Role
            var realmRoles = KeycloakClaimsHelper.ExtractRealmRoles(identity.Claims);
            foreach (var role in realmRoles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Extract resource (client) roles nếu enabled
            if (settings.UseResourceRoles)
            {
                var resourceRoles = KeycloakClaimsHelper.ExtractResourceRoles(
                    identity.Claims,
                    settings.ClientId,
                    addPrefix: false);

                foreach (var role in resourceRoles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }
    }
}

