# Giải thích SatisfiesSpecification trong Order Domain

## 📋 Tổng quan

`SatisfiesSpecification` là một method trong `Order` entity sử dụng **Specification Pattern** để kiểm tra các điều kiện business phức tạp một cách linh hoạt và có thể tái sử dụng.

## 🎯 Mục đích

- **Tách biệt business logic**: Tách các điều kiện kiểm tra ra khỏi entity
- **Tái sử dụng**: Có thể kết hợp nhiều specifications lại với nhau
- **Dễ test**: Mỗi specification có thể test độc lập
- **Linh hoạt**: Dễ dàng thêm/sửa/xóa các điều kiện mà không ảnh hưởng đến entity

## 📝 Cấu trúc

### 1. Method trong Order Entity

```csharp
// File: Order.cs (dòng 108-111)
public bool SatisfiesSpecification(OrderSpecifications.IOrderSpecification specification)
{
    return specification.IsSatisfiedBy(this);
}
```

### 2. Interface Specification

```csharp
// File: OrderSpecifications.cs
public interface IOrderSpecification
{
    bool IsSatisfiedBy(Order order);
}
```

## 💡 Các trường hợp sử dụng

### **Ví dụ 1: Kiểm tra Order có thể xóa không**

```csharp
// Tạo specification
var canBeDeletedSpec = new OrderSpecifications.CanBeDeletedSpecification();

// Kiểm tra order
Order order = Order.Create("John Doe");
bool canDelete = order.SatisfiesSpecification(canBeDeletedSpec);

// Kết quả: true (vì order chưa có items)
Console.WriteLine($"Can delete: {canDelete}"); // Output: Can delete: true
```

### **Ví dụ 2: Kiểm tra Order lớn (Large Order)**

```csharp
// Tạo order và thêm items
Order order = Order.Create("VIP Customer");
order.AddOrderItem(product1, 30);
order.AddOrderItem(product2, 25); // Tổng: 55 items

// Kiểm tra với threshold = 50
var largeOrderSpec = new OrderSpecifications.IsLargeOrderSpecification(threshold: 50);
bool isLarge = order.SatisfiesSpecification(largeOrderSpec);

// Kết quả: true (vì 55 >= 50)
Console.WriteLine($"Is large order: {isLarge}"); // Output: Is large order: true
```

### **Ví dụ 3: Kiểm tra Order chứa Product cụ thể**

```csharp
// Tạo order và thêm product
Product laptop = Product.Create("Laptop", "Electronics");
Order order = Order.Create("Customer A");
order.AddOrderItem(laptop, 2);

// Kiểm tra order có chứa laptop không
var containsLaptopSpec = new OrderSpecifications.ContainsProductSpecification(laptop);
bool containsLaptop = order.SatisfiesSpecification(containsLaptopSpec);

// Kết quả: true
Console.WriteLine($"Contains laptop: {containsLaptop}"); // Output: Contains laptop: true
```

### **Ví dụ 4: Kiểm tra Customer Name theo Pattern**

```csharp
// Tạo order với customer name
Order order = Order.Create("VIP Customer John");

// Kiểm tra customer name có chứa "VIP" không
var vipSpec = new OrderSpecifications.CustomerNamePatternSpecification("VIP");
bool isVip = order.SatisfiesSpecification(vipSpec);

// Kết quả: true
Console.WriteLine($"Is VIP customer: {isVip}"); // Output: Is VIP customer: true
```

### **Ví dụ 5: Kiểm tra Order Value trong khoảng**

```csharp
// Tạo order với nhiều items
Order order = Order.Create("Customer B");
order.AddOrderItem(product1, 10);
order.AddOrderItem(product2, 20); // Tổng quantity: 30

// Kiểm tra order value trong khoảng 20-50
var valueRangeSpec = new OrderSpecifications.OrderValueRangeSpecification(
    minValue: 20, 
    maxValue: 50
);
bool inRange = order.SatisfiesSpecification(valueRangeSpec);

// Kết quả: true (vì 30 nằm trong khoảng 20-50)
Console.WriteLine($"Value in range: {inRange}"); // Output: Value in range: true
```

## 🔗 Composite Specifications (Kết hợp nhiều điều kiện)

### **Ví dụ 6: Kết hợp với AND (Tất cả điều kiện phải đúng)**

```csharp
// Business rule: VIP customers với large orders được free shipping
var vipSpec = new OrderSpecifications.CustomerNamePatternSpecification("VIP");
var largeOrderSpec = new OrderSpecifications.IsLargeOrderSpecification(50);
var hasItemsSpec = new OrderSpecifications.HasItemsSpecification();

// Kết hợp các specifications với AND
var vipLargeOrderSpec = vipSpec
    .And(largeOrderSpec)
    .And(hasItemsSpec);

// Kiểm tra order
Order order = Order.Create("VIP Customer");
order.AddOrderItem(product1, 60); // Large order

bool qualifiesForFreeShipping = order.SatisfiesSpecification(vipLargeOrderSpec);

// Kết quả: true (vì thỏa tất cả điều kiện)
Console.WriteLine($"Qualifies for free shipping: {qualifiesForFreeShipping}");
```

### **Ví dụ 7: Kết hợp với OR (Chỉ cần 1 điều kiện đúng)**

```csharp
// Business rule: Order được ưu tiên nếu là VIP HOẶC large order
var vipSpec = new OrderSpecifications.CustomerNamePatternSpecification("VIP");
var largeOrderSpec = new OrderSpecifications.IsLargeOrderSpecification(100);

// Kết hợp với OR
var prioritySpec = vipSpec.Or(largeOrderSpec);

Order order1 = Order.Create("VIP Customer"); // Chỉ VIP
bool isPriority1 = order1.SatisfiesSpecification(prioritySpec); // true

Order order2 = Order.Create("Regular Customer");
order2.AddOrderItem(product1, 120); // Large order
bool isPriority2 = order2.SatisfiesSpecification(prioritySpec); // true

Order order3 = Order.Create("Regular Customer");
order3.AddOrderItem(product1, 10); // Không VIP, không large
bool isPriority3 = order3.SatisfiesSpecification(prioritySpec); // false
```

### **Ví dụ 8: Kết hợp với NOT (Phủ định điều kiện)**

```csharp
// Business rule: Order có thể xóa nếu KHÔNG có items
var hasItemsSpec = new OrderSpecifications.HasItemsSpecification();
var canBeDeletedSpec = hasItemsSpec.Not(); // Phủ định

Order emptyOrder = Order.Create("Customer");
bool canDelete = emptyOrder.SatisfiesSpecification(canBeDeletedSpec); // true

Order orderWithItems = Order.Create("Customer");
orderWithItems.AddOrderItem(product1, 5);
bool canDelete2 = orderWithItems.SatisfiesSpecification(canBeDeletedSpec); // false
```

## 🏢 Sử dụng trong Domain Service

```csharp
public class OrderDomainService
{
    public OrderStatistics CalculateOrderStatistics(Order order)
    {
        // Tạo các specifications
        var largeOrderSpec = new OrderSpecifications.IsLargeOrderSpecification();
        var hasItemsSpec = new OrderSpecifications.HasItemsSpecification();
        
        // Sử dụng SatisfiesSpecification để kiểm tra
        return new OrderStatistics
        {
            IsLargeOrder = order.SatisfiesSpecification(largeOrderSpec),
            HasItems = order.SatisfiesSpecification(hasItemsSpec),
            TotalItems = order.GetTotalItemsCount()
        };
    }
}
```

## 🎨 So sánh: Có và không có Specification Pattern

### **❌ Không dùng Specification (Cách cũ)**

```csharp
// Phải viết nhiều methods riêng lẻ trong Order entity
public bool IsVipCustomer() { /* ... */ }
public bool IsLargeOrder() { /* ... */ }
public bool CanGetFreeShipping() 
{ 
    return IsVipCustomer() && IsLargeOrder() && HasOrderItems();
}
// ❌ Khó tái sử dụng, khó kết hợp, khó test
```

### **✅ Dùng Specification Pattern (Cách mới)**

```csharp
// Tạo specifications độc lập, có thể kết hợp
var freeShippingSpec = vipSpec.And(largeOrderSpec).And(hasItemsSpec);
bool qualifies = order.SatisfiesSpecification(freeShippingSpec);
// ✅ Dễ tái sử dụng, dễ kết hợp, dễ test
```

## 📊 Lợi ích

1. **Separation of Concerns**: Business logic tách biệt khỏi entity
2. **Reusability**: Specifications có thể dùng lại ở nhiều nơi
3. **Composability**: Dễ dàng kết hợp nhiều specifications
4. **Testability**: Mỗi specification test độc lập
5. **Maintainability**: Dễ bảo trì và mở rộng

## 🔍 Các Specifications có sẵn

1. `CanBeDeletedSpecification` - Kiểm tra order có thể xóa
2. `HasItemsSpecification` - Kiểm tra order có items
3. `IsLargeOrderSpecification` - Kiểm tra order lớn (có threshold)
4. `ContainsProductSpecification` - Kiểm tra order chứa product
5. `OrderValueRangeSpecification` - Kiểm tra giá trị order trong khoảng
6. `CustomerNamePatternSpecification` - Kiểm tra customer name theo pattern

## 🎯 Kết luận

`SatisfiesSpecification` là một cách tiếp cận linh hoạt và mạnh mẽ để kiểm tra các điều kiện business phức tạp trong Order domain, giúp code dễ đọc, dễ test và dễ bảo trì hơn.

