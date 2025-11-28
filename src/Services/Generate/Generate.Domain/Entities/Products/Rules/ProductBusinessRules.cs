using Generate.Domain.Entities.Categories;
using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products.ValueObject;
using Contracts.Domain;

namespace Generate.Domain.Entities.Products.Rules;

/// <summary>
/// Chứa các business rules phức tạp cho Product entity
/// Tách riêng business logic ra khỏi entity để dễ maintain và test
/// Follows common Business Rules structure: Management, Analytics, Query, Lifecycle
/// </summary>
public static class ProductBusinessRules
{
    /// <summary>
    /// Business rules cho việc quản lý Category
    /// </summary>
    public static class CategoryManagement
    {
        /// <summary>
        /// Logic để assign product to category
        /// </summary>
        public static void AssignToCategory(Product product, Category category)
        {
            ProductValidationRules.CategoryManagement.ValidateCategory(category);

            // Business logic: Update category association
            // Sẽ được implement trong Product entity
        }

        /// <summary>
        /// Logic để remove product from category
        /// </summary>
        public static void RemoveFromCategory(Product product)
        {
            // Business logic: Category có thể null (product không bắt buộc có category)
            // Chỉ cần set Category = null
        }
    }

    /// <summary>
    /// Management operations for OrderItems in Product
    /// Handles Add, Remove, Update operations following common template pattern
    /// </summary>
    public static class OrderItemManagement
    {
        /// <summary>
        /// Adds order item to product using template method pattern
        /// </summary>
        public static void AddOrderItem(List<OrderItem> orderItems, OrderItem orderItem)
        {
            EntityManagementHelper.ExecuteAdd(orderItems, orderItem,
                validate: ProductValidationRules.OrderItemManagement.ValidateOrderItem,
                validateNotExists: ProductValidationRules.OrderItemManagement.ValidateOrderItemNotExists,
                validateConstraints: oi => ProductValidationRules.ProductConstraints.ValidateMaxOrderItemsLimit(oi));
        }

        /// <summary>
        /// Removes order item from product using template method pattern
        /// </summary>
        public static void RemoveOrderItem(List<OrderItem> orderItems, OrderItem orderItem)
        {
            EntityManagementHelper.ExecuteRemove(orderItems, orderItem,
                validate: ProductValidationRules.OrderItemManagement.ValidateOrderItem,
                validateExists: ProductValidationRules.OrderItemManagement.ValidateOrderItemExists);
        }
    }

    /// <summary>
    /// Business rules cho việc quản lý ProductDetail
    /// </summary>
    public static class ProductDetailManagement
    {
        /// <summary>
        /// Logic để update product detail
        /// </summary>
        public static void UpdateProductDetail(Product product, ProductDetail? productDetail)
        {
            ProductValidationRules.ProductDetailManagement.ValidateProductDetail(productDetail);

            // Business logic: ProductDetail có thể null hoặc update
            // Sẽ được implement trong Product entity
        }
    }

    /// <summary>
    /// Analytics operations for Product entity
    /// Handles Calculate, Count, Analyze operations following common template pattern
    /// </summary>
    public static class Analytics
    {
        /// <summary>
        /// Calculates number of order items using template method
        /// </summary>
        public static int CalculateOrderItemsCount(List<OrderItem> orderItems)
        {
            return EntityAnalyticsHelper.ExecuteCount(orderItems);
        }

        /// <summary>
        /// Calculates total ordered quantity using template method
        /// </summary>
        public static decimal CalculateTotalOrderedQuantity(List<OrderItem> orderItems)
        {
            return EntityAnalyticsHelper.ExecuteSum(orderItems, oi => (decimal)oi.Quantity);
        }

        /// <summary>
        /// Checks if product is popular using threshold analysis template
        /// </summary>
        public static bool IsPopularProduct(List<OrderItem> orderItems, int orderThreshold = 10)
        {
            return EntityAnalyticsHelper.ExecuteThresholdCheck(orderItems, oi => oi.Count(), orderThreshold);
        }

        /// <summary>
        /// Checks if product is high volume using threshold analysis template
        /// </summary>
        public static bool IsHighVolumeProduct(List<OrderItem> orderItems, decimal volumeThreshold = 100)
        {
            var totalVolume = CalculateTotalOrderedQuantity(orderItems);
            return totalVolume >= volumeThreshold;
        }

        /// <summary>
        /// Gets top orders by quantity (Analytics query)
        /// </summary>
        public static IReadOnlyList<OrderItem> GetTopOrdersByQuantity(List<OrderItem> orderItems, int topCount = 5)
        {
            return orderItems
                .OrderByDescending(oi => oi.Quantity)
                .Take(topCount)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Query operations for Product entity
    /// Handles Find, Get, Contains operations following common template pattern
    /// </summary>
    public static class Query
    {
        /// <summary>
        /// Gets order items by date range using template method
        /// </summary>
        public static IReadOnlyList<OrderItem> GetOrderItemsByDateRange(
            List<OrderItem> orderItems,
            DateTime fromDate,
            DateTime toDate)
        {
            return EntityQueryHelper.ExecuteGet(orderItems,
                oi => oi.CreatedDate >= fromDate && oi.CreatedDate <= toDate);
        }

        /// <summary>
        /// Gets unique orders using template method
        /// </summary>
        public static IReadOnlyList<OrderItem> GetUniqueOrders(List<OrderItem> orderItems)
        {
            var uniqueOrderItems = orderItems
                .GroupBy(oi => oi.Order.Id)
                .Select(g => g.First());
            return EntityQueryHelper.ExecuteGet(uniqueOrderItems);
        }

        /// <summary>
        /// Checks if product contains order item
        /// </summary>
        public static bool ContainsOrderItem(List<OrderItem> orderItems, OrderItem orderItem)
        {
            if (orderItem == null) return false;
            return EntityQueryHelper.ExecuteContains(orderItems,
                oi => oi.Order.Id == orderItem.Order.Id && oi.Product.Id == orderItem.Product.Id);
        }
    }

    /// <summary>
    /// Lifecycle operations for Product entity
    /// Handles Can*, Has*, Is* operations following common template pattern
    /// </summary>
    public static class Lifecycle
    {
        /// <summary>
        /// Checks if product can be deleted using template method
        /// </summary>
        public static bool CanBeDeleted(List<OrderItem> orderItems)
        {
            return EntityLifecycleHelper.ExecuteCanOperation(orderItems, oi => !oi.Any());
        }

        /// <summary>
        /// Checks if product is in category (simple property check)
        /// </summary>
        public static bool IsInCategory(Category? category)
        {
            return category != null;
        }

        /// <summary>
        /// Checks if product has active orders using template method
        /// </summary>
        public static bool HasActiveOrders(List<OrderItem> orderItems)
        {
            return EntityLifecycleHelper.ExecuteHasOperation(orderItems);
        }
    }
}
