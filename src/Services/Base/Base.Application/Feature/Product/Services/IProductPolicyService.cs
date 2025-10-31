using Shared.DTOs.Authorization;
using Shared.DTOs.Authorization.PolicyContexts;

namespace Base.Application.Feature.Product.Services
{
    /// <summary>
    /// Service interface for product-related policy operations
    /// Encapsulates all policy evaluation logic
    /// </summary>
    public interface IProductPolicyService
    {
        /// <summary>
        /// Get filter criteria for product list based on current user's permissions
        /// Returns filter context that should be applied to product queries
        /// </summary>
        Task<ProductListFilterContext> GetProductListFilterAsync();

        /// <summary>
        /// Check if current user can view a specific product
        /// </summary>
        Task<PolicyEvaluationResult> CanViewProductAsync(long productId, decimal productPrice);

        /// <summary>
        /// Check if current user can create a product
        /// </summary>
        Task<PolicyEvaluationResult> CanCreateProductAsync();

        /// <summary>
        /// Check if current user can update a specific product
        /// </summary>
        Task<PolicyEvaluationResult> CanUpdateProductAsync(long productId, string? createdBy, string? category);

        /// <summary>
        /// Check if current user can delete a product
        /// </summary>
        Task<PolicyEvaluationResult> CanDeleteProductAsync(long productId);
    }
}
