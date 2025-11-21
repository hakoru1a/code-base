using Microsoft.AspNetCore.Http;
using Shared.DTOs.Authorization;
using Shared.DTOs.Product;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for accessing policy filter context from HttpContext
    /// </summary>
    public static class HttpContextPolicyExtensions
    {
        /// <summary>
        /// Get filter context from a specific policy evaluation
        /// </summary>
        /// <typeparam name="T">Type of filter context to retrieve</typeparam>
        /// <param name="context">HTTP context</param>
        /// <param name="policyName">Name of the policy that provided the filter context</param>
        /// <returns>Filter context or null if not found</returns>
        public static T? GetPolicyFilterContext<T>(this HttpContext context, string policyName) 
            where T : class, IPolicyFilterContext
        {
            if (context?.Items == null)
                return null;

            var key = $"PolicyFilterContext:{policyName}";
            if (context.Items.TryGetValue(key, out var filterContext) && filterContext is T typedContext)
            {
                return typedContext;
            }

            return null;
        }

        /// <summary>
        /// Get the most recent filter context (backward compatibility)
        /// </summary>
        /// <typeparam name="T">Type of filter context to retrieve</typeparam>
        /// <param name="context">HTTP context</param>
        /// <returns>Filter context or null if not found</returns>
        public static T? GetPolicyFilterContext<T>(this HttpContext context) 
            where T : class, IPolicyFilterContext
        {
            if (context?.Items == null)
                return null;

            if (context.Items.TryGetValue("PolicyFilterContext", out var filterContext) && filterContext is T typedContext)
            {
                return typedContext;
            }

            return null;
        }

        /// <summary>
        /// Get product filter context from PRODUCT:VIEW policy
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Product filter context or null if not found</returns>
        public static ProductFilterContext? GetProductFilterContext(this HttpContext context)
        {
            return context.GetPolicyFilterContext<ProductFilterContext>("PRODUCT:VIEW") ?? 
                   context.GetPolicyFilterContext<ProductFilterContext>();
        }

        /// <summary>
        /// Check if policy filter context exists
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="policyName">Policy name to check</param>
        /// <returns>True if filter context exists</returns>
        public static bool HasPolicyFilterContext(this HttpContext context, string policyName)
        {
            if (context?.Items == null)
                return false;

            var key = $"PolicyFilterContext:{policyName}";
            return context.Items.ContainsKey(key);
        }

        /// <summary>
        /// Check if any policy filter context exists
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>True if any filter context exists</returns>
        public static bool HasPolicyFilterContext(this HttpContext context)
        {
            if (context?.Items == null)
                return false;

            return context.Items.ContainsKey("PolicyFilterContext");
        }

        /// <summary>
        /// Get all policy filter contexts
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Dictionary of policy names and their filter contexts</returns>
        public static Dictionary<string, IPolicyFilterContext> GetAllPolicyFilterContexts(this HttpContext context)
        {
            var result = new Dictionary<string, IPolicyFilterContext>();

            if (context?.Items == null)
                return result;

            foreach (var item in context.Items)
            {
                if (item.Key is string key && 
                    key.StartsWith("PolicyFilterContext:") && 
                    item.Value is IPolicyFilterContext filterContext)
                {
                    var policyName = key.Replace("PolicyFilterContext:", "");
                    result[policyName] = filterContext;
                }
            }

            return result;
        }
    }
}