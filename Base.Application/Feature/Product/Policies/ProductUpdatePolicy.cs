using Infrastructure.Authorization;
using Shared.DTOs.Authorization;
using Shared.DTOs.Authorization.PolicyContexts;
using Shared.Identity;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Policy for product update with ownership and category-based access control
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class ProductUpdatePolicy : BasePolicy<ProductUpdateContext>
    {
        public const string POLICY_NAME = "PRODUCT:UPDATE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            ProductUpdateContext context)
        {
            // Admins can update any product
            if (HasRole(user, Roles.Admin))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(PolicyConstants.Messages.AdminFullAccess));
            }

            // Check if user is the creator of the product
            if (!string.IsNullOrEmpty(context.CreatedBy) && context.CreatedBy == user.UserId)
            {
                if (HasPermission(user, Permissions.Product.UpdateOwn))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        PolicyConstants.Messages.UserCanUpdateOwnProducts));
                }
            }

            // Product managers can update products in their category
            if (HasRole(user, Roles.ProductManager))
            {
                var userCategory = user.CustomAttributes.GetValueOrDefault(PolicyConstants.UserAttributes.Department) as string;
                var productCategory = context.ProductCategory;

                if (!string.IsNullOrEmpty(userCategory) &&
                    !string.IsNullOrEmpty(productCategory) &&
                    userCategory.Equals(productCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        PolicyConstants.Messages.ProductManagerCanUpdateInCategory));
                }
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                PolicyConstants.Messages.UserDoesNotHavePermissionToUpdate));
        }

        protected override ProductUpdateContext? ConvertToTypedContext(Dictionary<string, object> context)
        {
            long productId = 0L;

            // Handle ProductId which can be long or int
            var idValue = GetContextValue<object>(context, PolicyConstants.ContextKeys.ProductId);
            if (idValue is long l)
                productId = l;
            else if (idValue is int i)
                productId = i;

            return new ProductUpdateContext
            {
                ProductId = productId,
                CreatedBy = GetContextValue<string>(context, PolicyConstants.ContextKeys.CreatedBy),
                ProductCategory = GetContextValue<string>(context, PolicyConstants.ContextKeys.ProductCategory)
            };
        }
    }
}

