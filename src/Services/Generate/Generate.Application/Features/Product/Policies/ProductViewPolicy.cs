using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.DTOs.Product;

namespace Generate.Application.Features.Product.Policies
{
    /// <summary>
    /// Policy for product viewing - supports dynamic filtering based on JWT claims
    /// </summary>
    [Policy("PRODUCT:VIEW", Description = "View products with dynamic filtering")]
    public class ProductViewPolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // All authenticated users can view products, but with different filters
            if (!IsAuthenticated(user))
            {
                return Task.FromResult(PolicyEvaluationResult.Deny(
                    "User must be authenticated to view products"));
            }

            var filterContext = new ProductFilterContext();

            // Admin và Manager có thể xem tất cả products
            if (HasAnyRole(user, "admin", "manager"))
            {
                filterContext.CanViewAll = true;
                filterContext.CanViewCrossDepartment = true;
                
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Admin/Manager can view all products", filterContext));
            }

            // Extract max_product_price từ JWT claims
            if (user.Claims.TryGetValue("max_product_price", out var maxPriceStr))
            {
                if (decimal.TryParse(maxPriceStr, out var maxPrice))
                {
                    filterContext.MaxPrice = maxPrice;
                }
            }

            // Extract department filter từ JWT claims
            if (user.CustomAttributes.TryGetValue("department", out var department))
            {
                filterContext.DepartmentFilter = department.ToString();
                
                // Chỉ có premium_user mới xem được cross-department
                if (HasRole(user, "premium_user"))
                {
                    filterContext.CanViewCrossDepartment = true;
                }
            }

            // Extract region filter từ JWT claims
            if (user.CustomAttributes.TryGetValue("region", out var region))
            {
                filterContext.RegionFilter = region.ToString();
            }

            // Extract allowed categories từ JWT permissions
            var categoryPermissions = user.Permissions
                .Where(p => p.StartsWith("category:view:"))
                .Select(p => p.Replace("category:view:", ""))
                .ToList();
                
            if (categoryPermissions.Any())
            {
                filterContext.AllowedCategories = categoryPermissions;
            }

            return Task.FromResult(PolicyEvaluationResult.Allow(
                "User authenticated with applied filters", filterContext));
        }
    }
}

