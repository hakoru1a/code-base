using Shared.DTOs.Authorization;

namespace Shared.DTOs.Product
{
    /// <summary>
    /// Product-specific filter context returned by product policies
    /// Contains filtering criteria extracted from JWT claims and user context
    /// 
    /// NOTE: Some properties like MaxPrice, DepartmentFilter, etc. are examples for demonstration.
    /// To use these filters, you need to add corresponding properties to your Product entity.
    /// Currently implemented: AllowedCategories (filters by Category.Name)
    /// </summary>
    public class ProductFilterContext : IPolicyFilterContext
    {
        /// <summary>
        /// Maximum price user can view (from JWT claim max_product_price)
        /// Example: JWT claim "max_product_price": "20000000" (20 triệu VND)
        /// NOTE: Requires Price property in Product entity
        /// </summary>
        public decimal? MaxPrice { get; set; }
        
        /// <summary>
        /// Minimum price user can view
        /// NOTE: Requires Price property in Product entity
        /// </summary>
        public decimal? MinPrice { get; set; }
        
        /// <summary>
        /// Categories user is allowed to view
        /// Extracted from permissions like "category:view:electronics", "category:view:books"
        /// ✅ IMPLEMENTED: Filters by Category.Name (existing entity property)
        /// </summary>
        public List<string>? AllowedCategories { get; set; }
        
        /// <summary>
        /// Department-based filter (from JWT department claim)
        /// Example: JWT claim "department": "sales" => only see sales products
        /// NOTE: Requires Department property in Product entity
        /// </summary>
        public string? DepartmentFilter { get; set; }
        
        /// <summary>
        /// Region-based filter (from JWT region claim)
        /// Example: JWT claim "region": "north" => only see products for north region
        /// NOTE: Requires Region property in Product entity
        /// </summary>
        public string? RegionFilter { get; set; }
        
        /// <summary>
        /// Whether user can see products from other departments
        /// Typically true for managers and premium users
        /// </summary>
        public bool CanViewCrossDepartment { get; set; } = false;
        
        /// <summary>
        /// Whether user can see all products (admin/manager privilege)
        /// When true, all other filters are ignored
        /// </summary>
        public bool CanViewAll { get; set; } = false;
        
        /// <summary>
        /// Brand restrictions (if any)
        /// Example: Some users can only see certain brands
        /// NOTE: Requires Brand property in Product entity
        /// </summary>
        public List<string>? AllowedBrands { get; set; }
        
        /// <summary>
        /// Whether to include discontinued products
        /// Typically only admin/manager can see discontinued products
        /// NOTE: Requires IsActive or Status property in Product entity
        /// </summary>
        public bool IncludeDiscontinued { get; set; } = false;
    }
}