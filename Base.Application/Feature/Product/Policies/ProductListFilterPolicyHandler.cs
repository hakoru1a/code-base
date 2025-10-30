using Infrastructure.Authorization;
using Shared.DTOs.Authorization;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Context for product list filtering
    /// </summary>
    public class ProductListFilterContext
    {
        public decimal? MaxPrice { get; set; }
        public string? Category { get; set; }
        public List<string>? AllowedCategories { get; set; }
    }

    /// <summary>
    /// Policy handler to determine filtering criteria for product list based on user role
    /// Returns filter metadata instead of Allow/Deny
    /// </summary>
    public class ProductListFilterPolicyHandler : BasePolicy
    {
        private const decimal MaxPriceForBasicUser = 5_000_000m;

        public const string POLICY_NAME = "PRODUCT_LIST_FILTER";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            var filterCriteria = GetFilterCriteriaForUser(user);

            // Policy always allows, but returns filter metadata
            var result = PolicyEvaluationResult.Allow(
                $"Filter applied for user role: {string.Join(", ", user.Roles)}");

            // Store filter criteria in metadata
            result.Metadata = new Dictionary<string, object>
            {
                { "MaxPrice", filterCriteria.MaxPrice ?? decimal.MaxValue },
                { "AllowedCategories", filterCriteria.AllowedCategories ?? new List<string>() }
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// Get filter criteria based on user roles
        /// This encapsulates all business logic for data filtering
        /// </summary>
        public ProductListFilterContext GetFilterCriteriaForUser(UserClaimsContext user)
        {
            var filterContext = new ProductListFilterContext
            {
                MaxPrice = null,
                AllowedCategories = null
            };

            // Admin and Premium users: no restrictions
            if (HasAnyRole(user, "admin", "premium_user"))
            {
                return filterContext; // No filter
            }

            // Basic users: price limit
            if (HasRole(user, "basic_user"))
            {
                filterContext.MaxPrice = MaxPriceForBasicUser;
            }

            // Product managers: filter by department/category
            if (HasRole(user, "product_manager"))
            {
                var department = user.CustomAttributes.GetValueOrDefault("department") as string;
                if (!string.IsNullOrEmpty(department))
                {
                    filterContext.AllowedCategories = new List<string> { department };
                }
            }

            return filterContext;
        }
    }
}

