using Shared.DTOs.Authorization;

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
        /// </summary>
        Task<ProductListFilter> GetProductListFilterAsync();

        /// <summary>
        /// Check if current user can view a specific product
        /// </summary>
        Task<PolicyCheckResult> CanViewProductAsync(long productId, decimal productPrice);

        /// <summary>
        /// Check if current user can create a product
        /// </summary>
        Task<PolicyCheckResult> CanCreateProductAsync();

        /// <summary>
        /// Check if current user can update a specific product
        /// </summary>
        Task<PolicyCheckResult> CanUpdateProductAsync(long productId, string? createdBy, string? category);

        /// <summary>
        /// Check if current user can delete a product
        /// </summary>
        Task<PolicyCheckResult> CanDeleteProductAsync(long productId);
    }

    /// <summary>
    /// Filter criteria for product list
    /// </summary>
    public class ProductListFilter
    {
        public decimal? MaxPrice { get; set; }
        public List<string>? AllowedCategories { get; set; }

        public bool HasFilters => MaxPrice.HasValue || (AllowedCategories?.Any() ?? false);
    }

    /// <summary>
    /// Result of a policy check
    /// </summary>
    public class PolicyCheckResult
    {
        public bool IsAllowed { get; set; }
        public string? Reason { get; set; }

        public static PolicyCheckResult Allow(string? reason = null)
        {
            return new PolicyCheckResult { IsAllowed = true, Reason = reason };
        }

        public static PolicyCheckResult Deny(string reason)
        {
            return new PolicyCheckResult { IsAllowed = false, Reason = reason };
        }
    }
}

