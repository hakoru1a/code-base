using Infrastructure.Authorization;
using Infrastructure.Authorization.Interfaces;
using Shared.DTOs.Authorization;
using Shared.DTOs.Authorization.PolicyContexts;
using Shared.Identity;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Policy to restrict product viewing based on price and user role
    /// Uses dynamic configuration from JWT claims or appsettings.json
    /// Naming Convention: {Resource}{Action}Policy
    /// </summary>
    public class ProductViewPolicy : BasePolicy<ProductViewContext>
    {
        private readonly IPolicyConfigurationService _policyConfigService;

        public const string POLICY_NAME = "PRODUCT:VIEW";
        public override string PolicyName => POLICY_NAME;

        public ProductViewPolicy(IPolicyConfigurationService policyConfigService)
        {
            _policyConfigService = policyConfigService;
        }

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            ProductViewContext context)
        {
            // Admins can view all products (unless restricted by JWT claims)
            if (HasRole(user, Roles.Admin))
            {
                var adminConfig = _policyConfigService.GetEffectivePolicyConfig(user);
                if (adminConfig.MaxPrice.HasValue && context.ProductPrice > adminConfig.MaxPrice.Value)
                {
                    return Task.FromResult(PolicyEvaluationResult.Deny(
                        string.Format(PolicyConstants.Messages.PriceExceedsLimitTemplate,
                            context.ProductPrice, "admin", adminConfig.MaxPrice.Value)));
                }
                return Task.FromResult(PolicyEvaluationResult.Allow(PolicyConstants.Messages.AdminFullAccess));
            }

            // Premium users can view all products (unless restricted by JWT claims)
            if (HasRole(user, Roles.PremiumUser))
            {
                var premiumConfig = _policyConfigService.GetEffectivePolicyConfig(user);
                if (premiumConfig.MaxPrice.HasValue && context.ProductPrice > premiumConfig.MaxPrice.Value)
                {
                    return Task.FromResult(PolicyEvaluationResult.Deny(
                        string.Format(PolicyConstants.Messages.PriceExceedsLimitTemplate,
                            context.ProductPrice, "premium user", premiumConfig.MaxPrice.Value)));
                }
                return Task.FromResult(PolicyEvaluationResult.Allow(PolicyConstants.Messages.PremiumUserFullAccess));
            }

            // Get effective configuration (JWT claims > Role config > No restriction)
            var effectiveConfig = _policyConfigService.GetEffectivePolicyConfig(user);

            // Basic users: check against configured price limit (if configured)
            if (HasRole(user, Roles.BasicUser))
            {
                // If no max price configured, basic users can view all products
                if (!effectiveConfig.MaxPrice.HasValue)
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        string.Format(PolicyConstants.Messages.NoRestrictionTemplate, "Basic user")));
                }

                var maxPrice = effectiveConfig.MaxPrice.Value;
                if (context.ProductPrice <= maxPrice)
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        string.Format(PolicyConstants.Messages.PriceWithinLimitTemplate, context.ProductPrice, maxPrice)));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    string.Format(PolicyConstants.Messages.PriceExceedsConfiguredLimitTemplate,
                        context.ProductPrice, maxPrice, "basic users")));
            }

            // Users without proper role cannot view products
            return Task.FromResult(PolicyEvaluationResult.Deny(
                PolicyConstants.Messages.UserDoesNotHaveRoleToViewProducts));
        }

        protected override ProductViewContext? ConvertToTypedContext(Dictionary<string, object> context)
        {
            decimal productPrice = 0m;

            // Handle ProductPrice which can be decimal or double
            var priceValue = GetContextValue<object>(context, PolicyConstants.ContextKeys.ProductPrice);
            if (priceValue is decimal dec)
                productPrice = dec;
            else if (priceValue is double dbl)
                productPrice = (decimal)dbl;

            long productId = 0L;

            // Handle ProductId which can be long or int
            var idValue = GetContextValue<object>(context, PolicyConstants.ContextKeys.ProductId);
            if (idValue is long l)
                productId = l;
            else if (idValue is int i)
                productId = i;

            return new ProductViewContext
            {
                ProductPrice = productPrice,
                ProductId = productId,
                ProductCategory = GetContextValue<string>(context, PolicyConstants.ContextKeys.ProductCategory)
            };
        }
    }
}

