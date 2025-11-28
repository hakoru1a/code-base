using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products;

namespace Generate.Domain.Entities.Orders.Rules;

/// <summary>
/// Chứa các business rules phức tạp cho Order entity
/// Tách riêng business logic ra khỏi entity để dễ maintain và test
/// </summary>
public static class OrderBusinessRules
{
    /// <summary>
    /// Business rules cho việc quản lý OrderItems
    /// </summary>
    public static class ItemManagement
    {
        /// <summary>
        /// Logic để thêm order item - xử lý việc merge quantity nếu product đã tồn tại
        /// </summary>
        public static void AddOrderItem(List<OrderItem> orderItems, Order order, Product product, int quantity)
        {
            // Validation trước khi thực hiện business logic
            OrderValidationRules.OrderItem.ValidateProduct(product);
            OrderValidationRules.OrderItem.ValidateQuantity(quantity);

            // Business logic: Check nếu product đã tồn tại thì merge quantity
            var existingItem = orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity);
            }
            else
            {
                // Kiểm tra giới hạn số lượng items trước khi thêm
                OrderValidationRules.OrderConstraints.ValidateMaxItemsLimit(orderItems);

                var orderItem = OrderItem.Create(order, product, quantity);
                orderItems.Add(orderItem);
            }
        }

        /// <summary>
        /// Logic để xóa order item
        /// </summary>
        public static void RemoveOrderItem(List<OrderItem> orderItems, Product product)
        {
            OrderValidationRules.OrderItem.ValidateProduct(product);
            OrderValidationRules.OrderItem.ValidateProductExists(orderItems, product);

            var orderItem = orderItems.First(oi => ReferenceEquals(oi.Product, product));
            orderItems.Remove(orderItem);
        }

        /// <summary>
        /// Logic để update quantity của order item
        /// </summary>
        public static void UpdateOrderItemQuantity(List<OrderItem> orderItems, Product product, int newQuantity)
        {
            OrderValidationRules.OrderItem.ValidateProduct(product);
            OrderValidationRules.OrderItem.ValidateQuantity(newQuantity);
            OrderValidationRules.OrderItem.ValidateProductExists(orderItems, product);

            var orderItem = orderItems.First(oi => ReferenceEquals(oi.Product, product));
            orderItem.UpdateQuantity(newQuantity);
        }
    }

    /// <summary>
    /// Business rules cho việc tính toán và phân tích order
    /// </summary>
    public static class Analytics
    {
        /// <summary>
        /// Tính tổng số lượng items trong order
        /// </summary>
        public static int CalculateTotalItemsCount(List<OrderItem> orderItems)
        {
            return orderItems.Sum(oi => oi.Quantity);
        }

        /// <summary>
        /// Đếm số lượng products unique trong order
        /// </summary>
        public static int CalculateUniqueProductsCount(List<OrderItem> orderItems)
        {
            return orderItems.Count;
        }

        /// <summary>
        /// Kiểm tra order có phải là large order không
        /// </summary>
        public static bool IsLargeOrder(List<OrderItem> orderItems, int threshold = 50)
        {
            OrderValidationRules.OrderConstraints.ValidateThreshold(threshold);
            return CalculateTotalItemsCount(orderItems) >= threshold;
        }

        /// <summary>
        /// Tính tổng giá trị order (tạm thời return total quantity)
        /// TODO: Implement khi có pricing logic
        /// </summary>
        public static decimal CalculateTotalOrderValue(List<OrderItem> orderItems)
        {
            return CalculateTotalItemsCount(orderItems);
        }
    }

    /// <summary>
    /// Business rules cho việc query và tìm kiếm
    /// </summary>
    public static class Query
    {
        /// <summary>
        /// Kiểm tra product có tồn tại trong order không
        /// </summary>
        public static bool ContainsProduct(List<OrderItem> orderItems, Product? product)
        {
            if (product == null) return false;
            return orderItems.Any(oi => ReferenceEquals(oi.Product, product));
        }

        /// <summary>
        /// Lấy OrderItem theo Product
        /// </summary>
        public static OrderItem? GetOrderItem(List<OrderItem> orderItems, Product? product)
        {
            if (product == null) return null;
            return orderItems.FirstOrDefault(oi => ReferenceEquals(oi.Product, product));
        }

        /// <summary>
        /// Lấy tất cả OrderItems cho một Product (trong trường hợp có thể có nhiều entries)
        /// </summary>
        public static IReadOnlyList<OrderItem> GetOrderItemsForProduct(List<OrderItem> orderItems, Product? product)
        {
            if (product == null) return new List<OrderItem>().AsReadOnly();
            return orderItems.Where(oi => ReferenceEquals(oi.Product, product)).ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Business rules cho lifecycle của order
    /// </summary>
    public static class Lifecycle
    {
        /// <summary>
        /// Kiểm tra order có thể bị xóa không
        /// </summary>
        public static bool CanBeDeleted(List<OrderItem> orderItems)
        {
            return !orderItems.Any();
        }

        /// <summary>
        /// Kiểm tra order có items không
        /// </summary>
        public static bool HasOrderItems(List<OrderItem> orderItems)
        {
            return orderItems.Any();
        }
    }
}
