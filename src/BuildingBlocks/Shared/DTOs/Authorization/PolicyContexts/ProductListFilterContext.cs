namespace Shared.DTOs.Authorization.PolicyContexts
{
    /// <summary>
    /// Context and result for product list filtering policy
    /// Represents filter criteria that should be applied to product queries based on user permissions
    /// This serves as both the policy evaluation result and the filter to be applied
    /// </summary>
    public class ProductListFilterContext
    {
        /// <summary>
        /// Maximum price the user is allowed to see (null = no limit)
        /// Example: Basic users might only see products under 5,000,000 VND
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Categories the user is allowed to see (null/empty = all categories)
        /// Example: Product managers might only see products in their department
        /// </summary>
        public List<string>? AllowedCategories { get; set; }

        /// <summary>
        /// Indicates if any filters are active
        /// </summary>
        public bool HasFilters => MaxPrice.HasValue || (AllowedCategories?.Any() ?? false);

        /// <summary>
        /// Creates a context with no restrictions (for admin/premium users)
        /// </summary>
        public static ProductListFilterContext NoRestrictions() => new();

        /// <summary>
        /// Creates a context with price restriction
        /// </summary>
        public static ProductListFilterContext WithMaxPrice(decimal maxPrice) =>
            new() { MaxPrice = maxPrice };

        /// <summary>
        /// Creates a context with category restriction
        /// </summary>
        public static ProductListFilterContext WithCategories(params string[] categories) =>
            new() { AllowedCategories = categories?.ToList() };

        /// <summary>
        /// Creates a context with both price and category restrictions
        /// </summary>
        public static ProductListFilterContext WithRestrictions(decimal? maxPrice, List<string>? categories) =>
            new() { MaxPrice = maxPrice, AllowedCategories = categories };
    }
}

