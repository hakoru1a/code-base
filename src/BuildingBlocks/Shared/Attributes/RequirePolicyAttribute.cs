using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared.Attributes
{
    /// <summary>
    /// Attribute to specify that an endpoint requires policy evaluation (PBAC)
    /// Used with PolicyAuthorizationMiddleware to enforce policy-based access control
    /// Can be applied to controllers or individual actions
    /// </summary>
    /// <example>
    /// [RequirePolicy("PRODUCT:VIEW")]
    /// public async Task<IActionResult> GetProduct(long id) { }
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequirePolicyAttribute : Attribute, IAsyncActionFilter
    {
        /// <summary>
        /// The name of the policy to evaluate
        /// Should match a registered policy name
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// Creates a new RequirePolicy attribute
        /// </summary>
        /// <param name="policyName">The policy name to evaluate (e.g., "PRODUCT:VIEW")</param>
        /// <exception cref="ArgumentException">Thrown when policyName is null or empty</exception>
        public RequirePolicyAttribute(string policyName)
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new ArgumentException("Policy name cannot be empty", nameof(policyName));
            }

            PolicyName = policyName;
        }

        /// <summary>
        /// Called before the action executes
        /// Sets the policy name in HttpContext.Items for middleware to evaluate
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Set policy name in context items for PolicyAuthorizationMiddleware
            context.HttpContext.Items["RequiredPolicy"] = PolicyName;
            
            // Continue to next filter/middleware
            await next();
        }
    }
    
    /// <summary>
    /// Attribute to require permission-based authorization
    /// This is a marker attribute for future permission-based authorization
    /// </summary>
    /// <example>
    /// [RequirePermission(Permissions.Product.Create)]
    /// public async Task<IActionResult> CreateProduct() { }
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequirePermissionAttribute : Attribute
    {
        /// <summary>
        /// The required permission
        /// </summary>
        public string Permission { get; }
        
        /// <summary>
        /// Creates a new RequirePermission attribute
        /// </summary>
        /// <param name="permission">The permission required (e.g., "product:create")</param>
        /// <exception cref="ArgumentException">Thrown when permission is null or empty</exception>
        public RequirePermissionAttribute(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
            {
                throw new ArgumentException("Permission cannot be empty", nameof(permission));
            }

            Permission = permission;
        }
    }
}

