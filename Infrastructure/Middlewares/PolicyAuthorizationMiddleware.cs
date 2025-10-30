using Infrastructure.Authorization.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.DTOs.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Infrastructure.Middlewares
{
    /// <summary>
    /// Middleware to handle policy-based authorization at service level
    /// </summary>
    public class PolicyAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PolicyAuthorizationMiddleware> _logger;

        public PolicyAuthorizationMiddleware(
            RequestDelegate next,
            ILogger<PolicyAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IPolicyEvaluator policyEvaluator)
        {
            // Check if policy is required for this endpoint
            if (context.Items.TryGetValue("RequiredPolicy", out var policyNameObj) && 
                policyNameObj is string policyName)
            {
                _logger.LogInformation("Evaluating policy: {PolicyName} for user", policyName);

                var userContext = ExtractUserContext(context.User);
                var evaluationContext = new Dictionary<string, object>();

                // You can add more context extraction logic here based on route, query, etc.
                
                var result = await policyEvaluator.EvaluateAsync(policyName, userContext, evaluationContext);

                if (!result.IsAllowed)
                {
                    _logger.LogWarning(
                        "Policy {PolicyName} denied access for user {UserId}. Reason: {Reason}",
                        policyName, userContext.UserId, result.Reason);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        error = "Forbidden",
                        message = result.Reason,
                        policy = policyName
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                    return;
                }

                _logger.LogInformation(
                    "Policy {PolicyName} granted access for user {UserId}",
                    policyName, userContext.UserId);
            }

            await _next(context);
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
                catch (Exception ex)
                {
                    // Log but don't fail
                    Console.WriteLine($"Failed to parse realm_access: {ex.Message}");
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to parse resource_access: {ex.Message}");
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

            return context;
        }
    }
}

