using Generate.Domain.Entities.Categories;
using Generate.Domain.Entities.Orders.ValueObject;
using Generate.Domain.Entities.Products.ValueObject;

namespace Generate.Domain.Entities.Products.Rules;

/// <summary>
/// Chứa các business validation rules cho Product entity
/// Tách riêng validation logic ra khỏi entity để dễ maintain và test
/// </summary>
public static class ProductValidationRules
{
    public static class ProductName
    {
        public static void ValidateProductName(string productName)
        {
            ValidateNotEmpty(productName);
            ValidateLength(productName);
            ValidateFormat(productName);
        }

        private static void ValidateNotEmpty(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw ProductError.NameCannotBeEmpty();
        }

        private static void ValidateLength(string productName, int maxLength = 100)
        {
            if (productName.Length > maxLength)
                throw ProductError.NameTooLong(maxLength);
        }

        private static void ValidateFormat(string productName)
        {
            if (productName.Trim() != productName)
                throw ProductError.NameHasInvalidFormat();
        }
    }

    public static class CategoryManagement
    {
        public static void ValidateCategory(Category? category)
        {
            if (category == null)
                throw ProductError.CategoryCannotBeNull();
        }

        public static void ValidateCategoryExists(long categoryId)
        {
            if (categoryId <= 0)
                throw ProductError.CategoryNotFound(categoryId);
        }
    }

    public static class OrderItemManagement
    {
        public static void ValidateOrderItem(OrderItem? orderItem)
        {
            if (orderItem == null)
                throw ProductError.OrderItemCannotBeNull();
        }

        public static void ValidateOrderItemNotExists(List<OrderItem> orderItems, OrderItem newOrderItem)
        {
            var exists = orderItems.Any(oi => 
                ReferenceEquals(oi.Order, newOrderItem.Order) && 
                ReferenceEquals(oi.Product, newOrderItem.Product));
            
            if (exists)
                throw ProductError.OrderItemAlreadyExists();
        }

        public static void ValidateOrderItemExists(List<OrderItem> orderItems, OrderItem orderItem)
        {
            var exists = orderItems.Any(oi => 
                ReferenceEquals(oi.Order, orderItem.Order) && 
                ReferenceEquals(oi.Product, orderItem.Product));
            
            if (!exists)
                throw new Contracts.Exceptions.BusinessException("Order item not found");
        }
    }

    public static class ProductDetailManagement
    {
        public static void ValidateProductDetail(ProductDetail? productDetail)
        {
            // ProductDetail có thể null (optional)
            // Validation sẽ được handle trong ProductDetail.Create() nếu cần
        }

        public static void ValidateProductDetailNotExists(ProductDetail? existingDetail)
        {
            if (existingDetail != null)
                throw ProductError.ProductDetailAlreadyExists();
        }
    }

    public static class ProductConstraints
    {
        public static void ValidateCanBeDeleted(List<OrderItem> orderItems)
        {
            if (orderItems.Any())
                throw ProductError.CannotDeleteProductWithOrders();
        }

        public static void ValidateHasCategory(Category? category)
        {
            if (category == null)
                throw ProductError.ProductNotInCategory();
        }

        public static void ValidateMaxOrderItemsLimit(List<OrderItem> orderItems, int maxOrderItems = 10000)
        {
            if (orderItems.Count >= maxOrderItems)
                throw new Contracts.Exceptions.BusinessException($"Product cannot have more than {maxOrderItems} order items");
        }
    }
}
