using Infrastructure.Authorization;
using Shared.DTOs.Authorization;

namespace Base.Application.Feature.Product.Policies
{
    /// <summary>
    /// Context for product view policy
    /// </summary>
    public class ProductViewContext
    {
        public decimal ProductPrice { get; set; }
        public long ProductId { get; set; }
        public string? ProductCategory { get; set; }
    }

    /// <summary>
    /// Policy to restrict product viewing based on price
    /// Example: Users with "basic_user" role can only view products under 5,000,000 VND
    /// </summary>
    public class ProductViewPricePolicyHandler : BasePolicy<ProductViewContext>
    {
        private const decimal MaxPriceForBasicUser = 5_000_000m; // 5 triệu VND
        private const string BasicUserRole = "basic_user";
        private const string PremiumUserRole = "premium_user";
        private const string AdminRole = "admin";

        public const string POLICY_NAME = "PRODUCT_VIEW_PRICE";
        public override string PolicyName => POLICY_NAME;

        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            ProductViewContext context)
        {
            // Admins can view all products
            if (HasRole(user, AdminRole))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow("Admin has full access"));
            }

            // Premium users can view all products
            if (HasRole(user, PremiumUserRole))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow("Premium user has full access"));
            }

            // Basic users can only view products under the price limit
            if (HasRole(user, BasicUserRole))
            {
                if (context.ProductPrice <= MaxPriceForBasicUser)
                {
                    return Task.FromResult(PolicyEvaluationResult.Allow(
                        $"Product price {context.ProductPrice:N0} VND is within limit"));
                }

                return Task.FromResult(PolicyEvaluationResult.Deny(
                    $"Product price {context.ProductPrice:N0} VND exceeds the limit of {MaxPriceForBasicUser:N0} VND for basic users"));
            }

            // Users without proper role cannot view products
            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User does not have required role to view products"));
        }

        protected override ProductViewContext? ConvertToTypedContext(Dictionary<string, object> context)
        {
            decimal productPrice = 0m;

            // Xử lý ProductPrice có thể là decimal hoặc double
            var priceValue = GetContextValue<object>(context, "ProductPrice");
            if (priceValue is decimal dec)
                productPrice = dec;
            else if (priceValue is double dbl)
                productPrice = (decimal)dbl;

            long productId = 0L;

            // Xử lý ProductId có thể là long hoặc int
            var idValue = GetContextValue<object>(context, "ProductId");
            if (idValue is long l)
                productId = l;
            else if (idValue is int i)
                productId = i;

            return new ProductViewContext
            {
                ProductPrice = productPrice,
                ProductId = productId,
                ProductCategory = GetContextValue<string>(context, "ProductCategory")
            };
        }

    }
}

