using Generate.Domain.Entities.Products;

namespace Generate.Domain.Entities.Orders.Rules;

/// <summary>
/// Chứa các business validation rules cho Order entity
/// Tách riêng validation logic ra khỏi entity để dễ maintain và test
/// </summary>
public static class OrderValidationRules
{
    public static class CustomerName
    {
        public static void ValidateCustomerName(string customerName)
        {
            ValidateNotEmpty(customerName);
            ValidateLength(customerName);
            ValidateFormat(customerName);
        }

        private static void ValidateNotEmpty(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw OrderError.CustomerNameCannotBeEmpty();
        }

        private static void ValidateLength(string customerName, int maxLength = 100)
        {
            if (customerName.Length > maxLength)
                throw OrderError.CustomerNameTooLong(maxLength);
        }

        private static void ValidateFormat(string customerName)
        {
            if (customerName.Trim() != customerName)
                throw OrderError.CustomerNameHasInvalidFormat();
        }
    }

    public static class OrderItem
    {
        public static void ValidateProduct(Product? product)
        {
            if (product == null)
                throw OrderError.ProductCannotBeNull();
        }

        public static void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
                throw OrderError.InvalidQuantity();
        }

        public static void ValidateProductNotExists(List<Generate.Domain.Entities.Orders.ValueObject.OrderItem> orderItems, Product product)
        {
            var exists = orderItems.Any(oi => ReferenceEquals(oi.Product, product));
            if (exists)
                throw OrderError.OrderItemAlreadyExists();
        }

        public static void ValidateProductExists(List<Generate.Domain.Entities.Orders.ValueObject.OrderItem> orderItems, Product product)
        {
            var exists = orderItems.Any(oi => ReferenceEquals(oi.Product, product));
            if (!exists)
                throw OrderError.ProductNotFoundInOrder();
        }
    }

    public static class OrderConstraints
    {
        public static void ValidateCanBeDeleted(List<Generate.Domain.Entities.Orders.ValueObject.OrderItem> orderItems)
        {
            if (orderItems.Any())
                throw OrderError.CannotDeleteOrderWithItems();
        }

        public static void ValidateNotEmpty(List<Generate.Domain.Entities.Orders.ValueObject.OrderItem> orderItems)
        {
            if (!orderItems.Any())
                throw OrderError.OrderIsEmpty();
        }

        public static void ValidateMaxItemsLimit(List<Generate.Domain.Entities.Orders.ValueObject.OrderItem> orderItems, int maxItems = 100)
        {
            if (orderItems.Count >= maxItems)
                throw OrderError.MaxItemsLimitExceeded(maxItems);
        }

        public static void ValidateThreshold(int threshold)
        {
            if (threshold <= 0)
                throw OrderError.InvalidThreshold(threshold);
        }
    }
}
