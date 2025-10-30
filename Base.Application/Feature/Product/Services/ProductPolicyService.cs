using Infrastructure.Authorization.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.DTOs.Authorization;
using Shared.DTOs.Authorization.PolicyContexts;
using Shared.Identity;

namespace Base.Application.Feature.Product.Services
{
    /// <summary>
    /// Service implementation for product policy operations
    /// This encapsulates all policy-related complexity from controllers
    /// </summary>
    public class ProductPolicyService : IProductPolicyService
    {
        private readonly IPolicyEvaluator _policyEvaluator;
        private readonly IUserContextAccessor _userContextAccessor;
        private readonly ILogger<ProductPolicyService> _logger;

        public ProductPolicyService(
            IPolicyEvaluator policyEvaluator,
            IUserContextAccessor userContextAccessor,
            ILogger<ProductPolicyService> logger)
        {
            _policyEvaluator = policyEvaluator;
            _userContextAccessor = userContextAccessor;
            _logger = logger;
        }

        public async Task<ProductListFilterContext> GetProductListFilterAsync()
        {
            var userContext = _userContextAccessor.GetCurrentUserContext();

            _logger.LogDebug(
                "Getting product list filter for user {UserId} with roles: {Roles}",
                userContext.UserId,
                string.Join(", ", userContext.Roles));

            var policyResult = await _policyEvaluator.EvaluateAsync(
                Policies.ProductListFilterPolicy.POLICY_NAME,
                userContext,
                new Dictionary<string, object>());

            var filter = new ProductListFilterContext();

            // Extract filter criteria from policy metadata
            if (policyResult.Metadata != null)
            {
                if (policyResult.Metadata.TryGetValue(PolicyConstants.ContextKeys.MaxPrice, out var maxPriceObj) &&
                    maxPriceObj is decimal maxPrice &&
                    maxPrice < decimal.MaxValue)
                {
                    filter.MaxPrice = maxPrice;
                }

                if (policyResult.Metadata.TryGetValue(PolicyConstants.ContextKeys.AllowedCategories, out var categoriesObj) &&
                    categoriesObj is List<string> categories &&
                    categories.Any())
                {
                    filter.AllowedCategories = categories;
                }
            }

            _logger.LogInformation(
                "Product list filter for user {UserId}: MaxPrice={MaxPrice}, Categories={Categories}",
                userContext.UserId,
                filter.MaxPrice?.ToString("N0") ?? "No limit",
                filter.AllowedCategories != null ? string.Join(", ", filter.AllowedCategories) : "All");

            return filter;
        }

        public async Task<PolicyEvaluationResult> CanViewProductAsync(long productId, decimal productPrice)
        {
            var userContext = _userContextAccessor.GetCurrentUserContext();

            _logger.LogDebug(
                "Checking if user {UserId} can view product {ProductId} with price {Price}",
                userContext.UserId,
                productId,
                productPrice);

            var policyContext = new Dictionary<string, object>
            {
                { PolicyConstants.ContextKeys.ProductPrice, productPrice },
                { PolicyConstants.ContextKeys.ProductId, productId }
            };

            var policyResult = await _policyEvaluator.EvaluateAsync(
                Policies.ProductViewPolicy.POLICY_NAME,
                userContext,
                policyContext);

            if (!policyResult.IsAllowed)
            {
                _logger.LogWarning(
                    "User {UserId} denied access to product {ProductId}. Reason: {Reason}",
                    userContext.UserId,
                    productId,
                    policyResult.Reason);
            }

            return policyResult;
        }

        public async Task<PolicyEvaluationResult> CanCreateProductAsync()
        {
            var userContext = _userContextAccessor.GetCurrentUserContext();

            _logger.LogDebug(
                "Checking if user {UserId} can create product",
                userContext.UserId);

            var policyResult = await _policyEvaluator.EvaluateAsync(
                Policies.ProductCreatePolicy.POLICY_NAME,
                userContext,
                new Dictionary<string, object>());

            if (!policyResult.IsAllowed)
            {
                _logger.LogWarning(
                    "User {UserId} denied product creation. Reason: {Reason}",
                    userContext.UserId,
                    policyResult.Reason);
            }

            return policyResult;
        }

        public async Task<PolicyEvaluationResult> CanUpdateProductAsync(
            long productId,
            string? createdBy,
            string? category)
        {
            var userContext = _userContextAccessor.GetCurrentUserContext();

            _logger.LogDebug(
                "Checking if user {UserId} can update product {ProductId}",
                userContext.UserId,
                productId);

            var policyContext = new Dictionary<string, object>
            {
                { PolicyConstants.ContextKeys.ProductId, productId },
                { PolicyConstants.ContextKeys.CreatedBy, createdBy ?? "" },
                { PolicyConstants.ContextKeys.ProductCategory, category ?? "" }
            };

            var policyResult = await _policyEvaluator.EvaluateAsync(
                Policies.ProductUpdatePolicy.POLICY_NAME,
                userContext,
                policyContext);

            if (!policyResult.IsAllowed)
            {
                _logger.LogWarning(
                    "User {UserId} denied product {ProductId} update. Reason: {Reason}",
                    userContext.UserId,
                    productId,
                    policyResult.Reason);
            }

            return policyResult;
        }

        public async Task<PolicyEvaluationResult> CanDeleteProductAsync(long productId)
        {
            var userContext = _userContextAccessor.GetCurrentUserContext();

            _logger.LogDebug(
                "Checking if user {UserId} can delete product {ProductId}",
                userContext.UserId,
                productId);

            // For now, delete is RBAC only (handled at gateway)
            // But we can add PBAC here if needed
            return await Task.FromResult(PolicyEvaluationResult.Allow("Delete is controlled by RBAC"));
        }
    }
}
