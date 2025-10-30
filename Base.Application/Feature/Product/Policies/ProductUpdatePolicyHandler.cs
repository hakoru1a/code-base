using Infrastructure.Authorization;
using Shared.DTOs.Authorization;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Policy for product update with ownership check
    /// </summary>
    public class ProductUpdatePolicyHandler : BasePolicy
    {
        public const string POLICY_NAME = "PRODUCT_UPDATE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Admins can update any product
            if (HasRole(user, "admin"))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow("Admin has full access"));
            }

            // Check if user is the creator of the product
            var createdBy = GetContextValue<string>(context, "CreatedBy");
            if (!string.IsNullOrEmpty(createdBy) && createdBy == user.UserId)
            {
                if (HasPermission(user, "product:update:own"))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "User can update their own products"));
                }
            }

            // Product managers can update products in their category
            if (HasRole(user, "product_manager"))
            {
                var userCategory = user.CustomAttributes.GetValueOrDefault("department") as string;
                var productCategory = GetContextValue<string>(context, "ProductCategory");

                if (!string.IsNullOrEmpty(userCategory) &&
                    !string.IsNullOrEmpty(productCategory) &&
                    userCategory.Equals(productCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        "Product manager can update products in their category"));
                }
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have permission to update this product"));
        }
    }
}
