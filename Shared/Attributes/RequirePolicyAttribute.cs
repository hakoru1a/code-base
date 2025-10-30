using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Attributes
{
    /// <summary>
    /// Attribute to require policy-based authorization (PBAC)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePolicyAttribute : Attribute, IAsyncActionFilter
    {
        public string PolicyName { get; }
        public string[]? RequiredRoles { get; set; }
        
        public RequirePolicyAttribute(string policyName)
        {
            PolicyName = policyName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // This will be intercepted by the PolicyAuthorizationFilter
            // Just mark the action with metadata
            context.HttpContext.Items["RequiredPolicy"] = PolicyName;
            context.HttpContext.Items["RequiredRoles"] = RequiredRoles;
            
            await next();
        }
    }
    
    /// <summary>
    /// Attribute to require permission-based authorization
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute
    {
        public string Permission { get; }
        
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }
    }
}

