using Infrastructure.Authorization.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Middlewares
{
    /// <summary>
    /// Middleware to handle policy-based authorization at service level
    /// Evaluates policies marked with [RequirePolicy] attribute
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
            // This is set by RequirePolicyAttribute or can be set manually
            if (context.Items.TryGetValue("RequiredPolicy", out var policyNameObj) && 
                policyNameObj is string policyName)
            {
                _logger.LogInformation(
                    "Evaluating policy {PolicyName} for user {UserId} on endpoint {Path}",
                    policyName,
                    context.User?.Identity?.Name ?? "anonymous",
                    context.Request.Path);

                var userContext = context.User.ToUserClaimsContext();
                var evaluationContext = ExtractEvaluationContext(context);
                
                var result = await policyEvaluator.EvaluateAsync(policyName, userContext, evaluationContext);

                if (!result.IsAllowed)
                {
                    await HandlePolicyDenied(context, policyName, userContext.UserId, result.Reason);
                    return;
                }

                _logger.LogDebug(
                    "Policy {PolicyName} granted access for user {UserId}. Reason: {Reason}",
                    policyName, userContext.UserId, result.Reason);
            }

            await _next(context);
        }

        /// <summary>
        /// Extract evaluation context from HTTP request
        /// Can be extended to include route values, query parameters, etc.
        /// </summary>
        private Dictionary<string, object> ExtractEvaluationContext(HttpContext context)
        {
            var evaluationContext = new Dictionary<string, object>();

            // Add route values if available
            if (context.Request.RouteValues.Any())
            {
                foreach (var routeValue in context.Request.RouteValues)
                {
                    if (routeValue.Value != null)
                    {
                        evaluationContext[$"route:{routeValue.Key}"] = routeValue.Value;
                    }
                }
            }

            // Add request method
            evaluationContext["http_method"] = context.Request.Method;

            // Add request path
            evaluationContext["request_path"] = context.Request.Path.Value ?? "";

            return evaluationContext;
        }

        /// <summary>
        /// Handle policy evaluation failure
        /// Returns consistent error response
        /// </summary>
        private async Task HandlePolicyDenied(
            HttpContext context, 
            string policyName, 
            string userId, 
            string? reason)
        {
            _logger.LogWarning(
                "Policy {PolicyName} denied access for user {UserId} on {Path}. Reason: {Reason}",
                policyName, userId, context.Request.Path, reason);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Forbidden",
                message = reason ?? "Access denied by policy",
                policy = policyName,
                timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(errorResponse, options));
        }
    }
}

