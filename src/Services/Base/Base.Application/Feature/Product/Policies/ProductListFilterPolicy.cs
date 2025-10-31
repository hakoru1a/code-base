using Infrastructure.Authorization;
using Infrastructure.Authorization.Interfaces;
using Shared.DTOs.Authorization;
using Shared.DTOs.Authorization.PolicyContexts;
using Shared.Identity;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Policy handler to determine filtering criteria for product list based on user role
    /// Returns filter metadata instead of simple Allow/Deny
    /// Uses dynamic configuration instead of hardcoded values
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class ProductListFilterPolicy : BasePolicy
    {
        private readonly IPolicyConfigurationService _policyConfigService;

        public const string POLICY_NAME = "PRODUCT:LIST_FILTER";
        public override string PolicyName => POLICY_NAME;

        public ProductListFilterPolicy(IPolicyConfigurationService policyConfigService)
        {
            _policyConfigService = policyConfigService;
        }

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            var filterCriteria = GetFilterCriteriaForUser(user);

            // Policy always allows, but returns filter metadata
            var result = PolicyEvaluationResult.Allow(
                string.Format(PolicyConstants.Messages.FilterAppliedTemplate, string.Join(", ", user.Roles)));

            // Store filter criteria in metadata
            result.Metadata = new Dictionary<string, object>
            {
                { PolicyConstants.ContextKeys.MaxPrice, filterCriteria.MaxPrice ?? decimal.MaxValue },
                { PolicyConstants.ContextKeys.AllowedCategories, filterCriteria.AllowedCategories ?? new List<string>() }
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// Get filter criteria based on user roles and configuration
        /// Priority: JWT Claims > Role Configuration > No Restrictions
        /// This encapsulates all business logic for data filtering
        /// </summary>
        public ProductListFilterContext GetFilterCriteriaForUser(UserClaimsContext user)
        {
            // Get effective configuration (combines role config + JWT claims)
            var effectiveConfig = _policyConfigService.GetEffectivePolicyConfig(user);

            // Admin and Premium users: no restrictions (unless overridden by claims)
            if (HasAnyRole(user, Roles.Admin, Roles.PremiumUser) &&
                effectiveConfig.MaxPrice == null &&
                effectiveConfig.AllowedCategories == null)
            {
                return ProductListFilterContext.NoRestrictions();
            }

            // Apply effective configuration from JWT claims or appsettings.json
            var filterContext = new ProductListFilterContext
            {
                MaxPrice = effectiveConfig.MaxPrice,
                AllowedCategories = effectiveConfig.AllowedCategories
            };

            // Special handling: Product managers inherit department from custom attributes if no explicit config
            if (HasRole(user, Roles.ProductManager) &&
                filterContext.AllowedCategories == null)
            {
                var department = user.CustomAttributes.GetValueOrDefault(PolicyConstants.UserAttributes.Department) as string;
                if (!string.IsNullOrEmpty(department))
                {
                    filterContext.AllowedCategories = new List<string> { department };
                }
            }

            return filterContext;
        }
    }
}

