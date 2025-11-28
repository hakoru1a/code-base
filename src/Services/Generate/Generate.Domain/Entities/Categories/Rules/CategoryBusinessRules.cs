using Generate.Domain.Entities.Products;
using Contracts.Domain;

namespace Generate.Domain.Entities.Categories.Rules;

/// <summary>
/// Chứa các business rules phức tạp cho Category entity
/// Tách riêng business logic ra khỏi entity để dễ maintain và test
/// Follows common Business Rules structure: Management, Analytics, Query, Lifecycle
/// </summary>
public static class CategoryBusinessRules
{
    /// <summary>
    /// Management operations for Products in Category
    /// Handles Add, Remove, Update operations following common template pattern
    /// </summary>
    public static class ProductManagement
    {
        /// <summary>
        /// Adds product to category using template method pattern
        /// </summary>
        public static void AddProduct(List<Product> products, Product product)
        {
            EntityManagementHelper.ExecuteAdd(products, product,
                validate: CategoryValidationRules.ProductManagement.ValidateProduct,
                validateNotExists: CategoryValidationRules.ProductManagement.ValidateProductNotExists,
                validateConstraints: p => CategoryValidationRules.CategoryConstraints.ValidateMaxProductsLimit(p));
        }

        /// <summary>
        /// Removes product from category using template method pattern
        /// </summary>
        public static void RemoveProduct(List<Product> products, Product product)
        {
            EntityManagementHelper.ExecuteRemove(products, product,
                validate: CategoryValidationRules.ProductManagement.ValidateProduct,
                validateExists: CategoryValidationRules.ProductManagement.ValidateProductExists);
        }
    }

    /// <summary>
    /// Analytics operations for Category entity
    /// Handles Calculate, Count, Analyze operations following common template pattern
    /// </summary>
    public static class Analytics
    {
        /// <summary>
        /// Calculates number of products in category using template method
        /// </summary>
        public static int CalculateProductCount(List<Product> products)
        {
            return EntityAnalyticsHelper.ExecuteCount(products);
        }

        /// <summary>
        /// Calculates total items across all products in category
        /// </summary>
        public static int CalculateTotalItemsInCategory(List<Product> products)
        {
            return EntityAnalyticsHelper.ExecuteSum(products, p => p.OrderItems.Sum(oi => oi.Quantity));
        }

        /// <summary>
        /// Checks if category is large using threshold analysis template
        /// </summary>
        public static bool IsLargeCategory(List<Product> products, int threshold = 50)
        {
            return EntityAnalyticsHelper.ExecuteThresholdCheck(products, p => p.Count(), threshold);
        }

        /// <summary>
        /// Checks if category is popular based on order count
        /// </summary>
        public static bool IsPopularCategory(List<Product> products, int orderThreshold = 100)
        {
            return EntityAnalyticsHelper.ExecuteThresholdCheck(products,
                p => p.SelectMany(prod => prod.OrderItems).Count(),
                orderThreshold);
        }

        /// <summary>
        /// Gets top products by order count (Analytics query)
        /// </summary>
        public static IReadOnlyList<Product> GetTopProducts(List<Product> products, int topCount = 5)
        {
            return products
                .OrderByDescending(p => p.OrderItems.Count)
                .Take(topCount)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Query operations for Category entity
    /// Handles Find, Get, Contains operations following common template pattern
    /// </summary>
    public static class Query
    {
        /// <summary>
        /// Finds products by name pattern using template method
        /// </summary>
        public static IReadOnlyList<Product> FindProductsByName(List<Product> products, string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern))
                return EntityQueryHelper.ExecuteGet(products);

            var pattern = namePattern.ToLowerInvariant();
            return EntityQueryHelper.ExecuteFind(products, p => p.Name.ToLowerInvariant().Contains(pattern));
        }

        /// <summary>
        /// Gets products that have orders using template method
        /// </summary>
        public static IReadOnlyList<Product> GetProductsWithOrders(List<Product> products)
        {
            return EntityQueryHelper.ExecuteGet(products, p => p.OrderItems.Any());
        }

        /// <summary>
        /// Gets deletable products (no orders) using template method
        /// </summary>
        public static IReadOnlyList<Product> GetDeletableProducts(List<Product> products)
        {
            return EntityQueryHelper.ExecuteGet(products, p => p.CanBeDeleted());
        }

        /// <summary>
        /// Checks if category contains specific product
        /// </summary>
        public static bool ContainsProduct(List<Product> products, Product product)
        {
            if (product == null) return false;
            return EntityQueryHelper.ExecuteContains(products, p => p.Id == product.Id);
        }
    }

    /// <summary>
    /// Lifecycle operations for Category entity
    /// Handles Can*, Has*, Is* operations following common template pattern
    /// </summary>
    public static class Lifecycle
    {
        /// <summary>
        /// Checks if category can be deleted using template method
        /// </summary>
        public static bool CanBeDeleted(List<Product> products)
        {
            return EntityLifecycleHelper.ExecuteCanOperation(products, p => !p.Any());
        }

        /// <summary>
        /// Checks if category has products using template method
        /// </summary>
        public static bool HasProducts(List<Product> products)
        {
            return EntityLifecycleHelper.ExecuteHasOperation(products);
        }

        /// <summary>
        /// Checks if category has active products (with orders) using template method
        /// </summary>
        public static bool HasActiveProducts(List<Product> products)
        {
            return EntityLifecycleHelper.ExecuteHasOperation(products, p => p.OrderItems.Any());
        }

        /// <summary>
        /// Checks if category is in active state (has products and orders)
        /// </summary>
        public static bool IsActiveCategory(List<Product> products)
        {
            return EntityLifecycleHelper.ExecuteIsOperation(products, p => p.Any() && p.Any(prod => prod.OrderItems.Any()));
        }
    }
}
