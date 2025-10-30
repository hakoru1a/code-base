
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Configurations;
using System.Security.Claims;
using System.Text.Json;

namespace Infrastructure.Extentions
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
                    OnTokenValidated = context =>
                    {
                        // Extract and map roles from Keycloak token
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            MapKeycloakRoles(identity, keycloakSettings);
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Add authorization policies for RBAC
        /// </summary>
        public static IServiceCollection AddKeycloakAuthorization(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Role-based policies (RBAC at Gateway level)
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("admin"));
                
                options.AddPolicy("ManagerOrAdmin", policy => 
                    policy.RequireRole("admin", "manager", "product_manager"));
                
                options.AddPolicy("AuthenticatedUser", policy => 
                    policy.RequireAuthenticatedUser());
                
                options.AddPolicy("PremiumUser", policy => 
                    policy.RequireRole("premium_user", "admin"));
                
                options.AddPolicy("BasicUser", policy => 
                    policy.RequireRole("basic_user", "premium_user", "admin"));

                // Permission-based policies
                options.AddPolicy("CanViewProducts", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => 
                            c.Type == "permissions" && c.Value.Contains("product:view")) ||
                        context.User.IsInRole("admin") ||
                        context.User.IsInRole("manager")));

                options.AddPolicy("CanCreateProducts", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => 
                            c.Type == "permissions" && c.Value.Contains("product:create")) ||
                        context.User.IsInRole("admin") ||
                        context.User.IsInRole("product_manager")));

                options.AddPolicy("CanUpdateProducts", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => 
                            c.Type == "permissions" && c.Value.Contains("product:update")) ||
                        context.User.IsInRole("admin") ||
                        context.User.IsInRole("product_manager")));

                options.AddPolicy("CanDeleteProducts", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => 
                            c.Type == "permissions" && c.Value.Contains("product:delete")) ||
                        context.User.IsInRole("admin")));
            });

            return services;
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
                    Console.WriteLine($"Failed to parse realm_access: {ex.Message}");
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
                        Console.WriteLine($"Failed to parse resource_access: {ex.Message}");
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

