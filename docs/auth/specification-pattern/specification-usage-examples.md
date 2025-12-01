# Specification Pattern - Hướng dẫn sử dụng

## 📋 Tổng quan

Specification Pattern là một design pattern giúp tách biệt business logic queries và conditions ra khỏi domain entities. Trong project này, tất cả specifications đều implement `ISpecification<T>` từ Contracts layer và được tách thành các file riêng để dễ quản lý.

## 🎯 Cấu trúc

### Interface chung từ Contracts

```csharp
// Contracts.Domain.Interface.ISpecification<T>
public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
}
```

### Composite Specifications

```csharp
// Contracts.Domain.Specifications.CompositeSpecification
- AndSpecification<T>    // Cả hai specifications phải thỏa mãn
- OrSpecification<T>    // Một trong hai specifications phải thỏa mãn
- NotSpecification<T>   // Phủ định của specification
```

### Extension Methods

```csharp
// Contracts.Domain.Specifications.SpecificationExtensions
- And<T>(ISpecification<T> right)  // Kết hợp AND
- Or<T>(ISpecification<T> right)   // Kết hợp OR
- Not<T>()                         // Phủ định
```

---

## 📦 Category Specifications

### 1. CanBeDeletedSpecification

Kiểm tra category có thể bị xóa không (không có products).

```csharp
using Generate.Domain.Categories;
using Generate.Domain.Categories.Specifications;
using Contracts.Domain.Interface;

// Tạo category
var category = Category.Create("Electronics");

// Kiểm tra có thể xóa không
var canBeDeletedSpec = new CanBeDeletedSpecification();
bool canDelete = category.SatisfiesSpecification(canBeDeletedSpec);

Console.WriteLine($"Can delete: {canDelete}"); // Output: true (chưa có products)

// Thêm product
var product = Product.Create("Laptop");
category.AddProduct(product);

// Kiểm tra lại
canDelete = category.SatisfiesSpecification(canBeDeletedSpec);
Console.WriteLine($"Can delete: {canDelete}"); // Output: false (đã có products)
```

### 2. HasProductsSpecification

Kiểm tra category có products không.

```csharp
var category = Category.Create("Electronics");
var hasProductsSpec = new HasProductsSpecification();

bool hasProducts = category.SatisfiesSpecification(hasProductsSpec);
Console.WriteLine($"Has products: {hasProducts}"); // Output: false

category.AddProduct(Product.Create("Laptop"));
hasProducts = category.SatisfiesSpecification(hasProductsSpec);
Console.WriteLine($"Has products: {hasProducts}"); // Output: true
```

### 3. IsLargeCategorySpecification

Kiểm tra category có phải là large category không (số lượng products >= threshold).

```csharp
var category = Category.Create("Electronics");

// Thêm nhiều products
for (int i = 0; i < 60; i++)
{
    category.AddProduct(Product.Create($"Product {i}"));
}

// Kiểm tra với threshold mặc định (50)
var largeCategorySpec = new IsLargeCategorySpecification();
bool isLarge = category.SatisfiesSpecification(largeCategorySpec);
Console.WriteLine($"Is large category: {isLarge}"); // Output: true

// Kiểm tra với threshold tùy chỉnh (100)
var largeCategorySpec100 = new IsLargeCategorySpecification(threshold: 100);
isLarge = category.SatisfiesSpecification(largeCategorySpec100);
Console.WriteLine($"Is large category (100+): {isLarge}"); // Output: false
```

### 4. ContainsProductSpecification

Kiểm tra category có chứa product cụ thể không.

```csharp
var category = Category.Create("Electronics");
var product1 = Product.Create("Laptop");
var product2 = Product.Create("Mouse");

category.AddProduct(product1);

var containsSpec = new ContainsProductSpecification(product1);
bool contains = category.SatisfiesSpecification(containsSpec);
Console.WriteLine($"Contains product1: {contains}"); // Output: true

var containsSpec2 = new ContainsProductSpecification(product2);
contains = category.SatisfiesSpecification(containsSpec2);
Console.WriteLine($"Contains product2: {contains}"); // Output: false
```

### 5. CategoryNamePatternSpecification

Kiểm tra category name có chứa pattern không.

```csharp
var category = Category.Create("Electronics & Gadgets");

var patternSpec = new CategoryNamePatternSpecification("electronics");
bool matches = category.SatisfiesSpecification(patternSpec);
Console.WriteLine($"Name contains 'electronics': {matches}"); // Output: true (case-insensitive)

var patternSpec2 = new CategoryNamePatternSpecification("food");
matches = category.SatisfiesSpecification(patternSpec2);
Console.WriteLine($"Name contains 'food': {matches}"); // Output: false
```

### 6. IsPopularCategorySpecification

Kiểm tra category có popular không (nhiều orders).

```csharp
var category = Category.Create("Electronics");
var product = Product.Create("Laptop");
category.AddProduct(product);

// Giả sử product có nhiều order items
// (Trong thực tế, order items sẽ được thêm qua Order entity)

var popularSpec = new IsPopularCategorySpecification(orderThreshold: 100);
bool isPopular = category.SatisfiesSpecification(popularSpec);
```

### 7. HasActiveProductsSpecification

Kiểm tra category có active products không (products có order items).

```csharp
var category = Category.Create("Electronics");
var product = Product.Create("Laptop");
category.AddProduct(product);

var activeProductsSpec = new HasActiveProductsSpecification();
bool hasActive = category.SatisfiesSpecification(activeProductsSpec);
```

---

## 🛒 Order Specifications

### 1. CanBeDeletedSpecification

Kiểm tra order có thể bị xóa không (không có items).

```csharp
using Generate.Domain.Orders;
using Generate.Domain.Orders.Specifications;

var order = Order.Create("John Doe");

var canBeDeletedSpec = new CanBeDeletedSpecification();
bool canDelete = order.SatisfiesSpecification(canBeDeletedSpec);
Console.WriteLine($"Can delete: {canDelete}"); // Output: true

// Thêm order item
var product = Product.Create("Laptop");
order.AddOrderItem(product, 2);

canDelete = order.SatisfiesSpecification(canBeDeletedSpec);
Console.WriteLine($"Can delete: {canDelete}"); // Output: false
```

### 2. HasItemsSpecification

Kiểm tra order có items không.

```csharp
var order = Order.Create("John Doe");
var hasItemsSpec = new HasItemsSpecification();

bool hasItems = order.SatisfiesSpecification(hasItemsSpec);
Console.WriteLine($"Has items: {hasItems}"); // Output: false

order.AddOrderItem(Product.Create("Laptop"), 1);
hasItems = order.SatisfiesSpecification(hasItemsSpec);
Console.WriteLine($"Has items: {hasItems}"); // Output: true
```

### 3. IsLargeOrderSpecification

Kiểm tra order có phải là large order không (tổng quantity >= threshold).

```csharp
var order = Order.Create("John Doe");
order.AddOrderItem(Product.Create("Laptop"), 30);
order.AddOrderItem(Product.Create("Mouse"), 25); // Tổng: 55 items

// Kiểm tra với threshold mặc định (50)
var largeOrderSpec = new IsLargeOrderSpecification();
bool isLarge = order.SatisfiesSpecification(largeOrderSpec);
Console.WriteLine($"Is large order: {isLarge}"); // Output: true

// Kiểm tra với threshold tùy chỉnh (100)
var largeOrderSpec100 = new IsLargeOrderSpecification(threshold: 100);
isLarge = order.SatisfiesSpecification(largeOrderSpec100);
Console.WriteLine($"Is large order (100+): {isLarge}"); // Output: false
```

### 4. ContainsProductSpecification

Kiểm tra order có chứa product cụ thể không.

```csharp
var order = Order.Create("John Doe");
var product1 = Product.Create("Laptop");
var product2 = Product.Create("Mouse");

order.AddOrderItem(product1, 2);

var containsSpec = new ContainsProductSpecification(product1);
bool contains = order.SatisfiesSpecification(containsSpec);
Console.WriteLine($"Contains product1: {contains}"); // Output: true

var containsSpec2 = new ContainsProductSpecification(product2);
contains = order.SatisfiesSpecification(containsSpec2);
Console.WriteLine($"Contains product2: {contains}"); // Output: false
```

### 5. OrderValueRangeSpecification

Kiểm tra order value có trong khoảng không.

```csharp
var order = Order.Create("John Doe");
order.AddOrderItem(Product.Create("Laptop"), 10);
order.AddOrderItem(Product.Create("Mouse"), 5); // Tổng: 15 items

var valueRangeSpec = new OrderValueRangeSpecification(minValue: 10, maxValue: 20);
bool inRange = order.SatisfiesSpecification(valueRangeSpec);
Console.WriteLine($"Value in range [10-20]: {inRange}"); // Output: true

var valueRangeSpec2 = new OrderValueRangeSpecification(minValue: 20, maxValue: 30);
inRange = order.SatisfiesSpecification(valueRangeSpec2);
Console.WriteLine($"Value in range [20-30]: {inRange}"); // Output: false
```

### 6. CustomerNamePatternSpecification

Kiểm tra customer name có chứa pattern không.

```csharp
var order = Order.Create("VIP Customer John");

var patternSpec = new CustomerNamePatternSpecification("vip");
bool matches = order.SatisfiesSpecification(patternSpec);
Console.WriteLine($"Customer name contains 'vip': {matches}"); // Output: true

var patternSpec2 = new CustomerNamePatternSpecification("premium");
matches = order.SatisfiesSpecification(patternSpec2);
Console.WriteLine($"Customer name contains 'premium': {matches}"); // Output: false
```

---

## 📱 Product Specifications

### 1. CanBeDeletedSpecification

Kiểm tra product có thể bị xóa không (không có order items).

```csharp
using Generate.Domain.Products;
using Generate.Domain.Products.Specifications;

var product = Product.Create("Laptop");

var canBeDeletedSpec = new CanBeDeletedSpecification();
bool canDelete = product.SatisfiesSpecification(canBeDeletedSpec);
Console.WriteLine($"Can delete: {canDelete}"); // Output: true
```

### 2. IsInCategorySpecification

Kiểm tra product có trong category không.

```csharp
var category = Category.Create("Electronics");
var product = Product.Create("Laptop");

var isInCategorySpec = new IsInCategorySpecification();
bool inCategory = product.SatisfiesSpecification(isInCategorySpec);
Console.WriteLine($"Is in category: {inCategory}"); // Output: false

product.AssignToCategory(category);
inCategory = product.SatisfiesSpecification(isInCategorySpec);
Console.WriteLine($"Is in category: {inCategory}"); // Output: true
```

### 3. BelongsToCategorySpecification

Kiểm tra product thuộc về category cụ thể không.

```csharp
var category1 = Category.Create("Electronics");
var category2 = Category.Create("Food");
var product = Product.Create("Laptop");

product.AssignToCategory(category1);

var belongsToSpec = new BelongsToCategorySpecification(category1);
bool belongs = product.SatisfiesSpecification(belongsToSpec);
Console.WriteLine($"Belongs to category1: {belongs}"); // Output: true

var belongsToSpec2 = new BelongsToCategorySpecification(category2);
belongs = product.SatisfiesSpecification(belongsToSpec2);
Console.WriteLine($"Belongs to category2: {belongs}"); // Output: false
```

### 4. IsPopularProductSpecification

Kiểm tra product có phải là popular product không (số lượng order items >= threshold).

```csharp
var product = Product.Create("Laptop");

// Giả sử product có nhiều order items
// (Trong thực tế, order items sẽ được thêm qua Order entity)

var popularSpec = new IsPopularProductSpecification(orderThreshold: 10);
bool isPopular = product.SatisfiesSpecification(popularSpec);
```

### 5. IsHighVolumeProductSpecification

Kiểm tra product có high volume không (tổng quantity >= threshold).

```csharp
var product = Product.Create("Laptop");

// Giả sử product có tổng quantity lớn
var highVolumeSpec = new IsHighVolumeProductSpecification(volumeThreshold: 100);
bool isHighVolume = product.SatisfiesSpecification(highVolumeSpec);
```

### 6. ProductNamePatternSpecification

Kiểm tra product name có chứa pattern không.

```csharp
var product = Product.Create("Gaming Laptop Pro");

var patternSpec = new ProductNamePatternSpecification("gaming");
bool matches = product.SatisfiesSpecification(patternSpec);
Console.WriteLine($"Name contains 'gaming': {matches}"); // Output: true
```

### 7. HasOrderItemsSpecification

Kiểm tra product có order items không.

```csharp
var product = Product.Create("Laptop");
var hasOrderItemsSpec = new HasOrderItemsSpecification();

bool hasOrderItems = product.SatisfiesSpecification(hasOrderItemsSpec);
Console.WriteLine($"Has order items: {hasOrderItems}"); // Output: false
```

### 8. HasProductDetailSpecification

Kiểm tra product có product detail không.

```csharp
var product = Product.Create("Laptop");
var hasDetailSpec = new HasProductDetailSpecification();

bool hasDetail = product.SatisfiesSpecification(hasDetailSpec);
Console.WriteLine($"Has product detail: {hasDetail}"); // Output: false

// Giả sử thêm product detail
// product.UpdateProductDetail(new ProductDetail(...));
// hasDetail = product.SatisfiesSpecification(hasDetailSpec);
// Console.WriteLine($"Has product detail: {hasDetail}"); // Output: true
```

### 9. HasOrdersInDateRangeSpecification

Kiểm tra product có orders trong date range không.

```csharp
var product = Product.Create("Laptop");
var fromDate = DateTime.Now.AddDays(-30);
var toDate = DateTime.Now;

var dateRangeSpec = new HasOrdersInDateRangeSpecification(fromDate, toDate);
bool hasOrdersInRange = product.SatisfiesSpecification(dateRangeSpec);
```

---

## 🔗 Composite Specifications

### Sử dụng Extension Methods

```csharp
using Contracts.Domain.Specifications; // Import extension methods

// AND - Cả hai specifications phải thỏa mãn
var category = Category.Create("Electronics");
var product = Product.Create("Laptop");
category.AddProduct(product);

var hasProductsSpec = new HasProductsSpecification();
var canBeDeletedSpec = new CanBeDeletedSpecification();

// Category có products VÀ có thể xóa (mâu thuẫn, sẽ false)
var andSpec = hasProductsSpec.And(canBeDeletedSpec);
bool result = category.SatisfiesSpecification(andSpec);
Console.WriteLine($"Has products AND can delete: {result}"); // Output: false

// OR - Một trong hai specifications phải thỏa mãn
var orSpec = hasProductsSpec.Or(canBeDeletedSpec);
result = category.SatisfiesSpecification(orSpec);
Console.WriteLine($"Has products OR can delete: {result}"); // Output: true

// NOT - Phủ định
var notSpec = hasProductsSpec.Not();
result = category.SatisfiesSpecification(notSpec);
Console.WriteLine($"NOT has products: {result}"); // Output: false (vì category có products)
```

### Kết hợp nhiều Specifications

```csharp
// Complex condition: Category có products VÀ là large category VÀ không thể xóa
var category = Category.Create("Electronics");

// Thêm nhiều products để đạt threshold
for (int i = 0; i < 60; i++)
{
    category.AddProduct(Product.Create($"Product {i}"));
}

var hasProductsSpec = new HasProductsSpecification();
var isLargeSpec = new IsLargeCategorySpecification(threshold: 50);
var canBeDeletedSpec = new CanBeDeletedSpecification();

// Kết hợp: HasProducts AND IsLarge AND NOT CanBeDeleted
var complexSpec = hasProductsSpec
    .And(isLargeSpec)
    .And(canBeDeletedSpec.Not());

bool satisfies = category.SatisfiesSpecification(complexSpec);
Console.WriteLine($"Complex condition satisfied: {satisfies}"); // Output: true
```

### Order Example - Discount Logic

```csharp
var order = Order.Create("VIP Customer");
order.AddOrderItem(Product.Create("Laptop"), 30);
order.AddOrderItem(Product.Create("Mouse"), 25); // Tổng: 55 items

var largeOrderSpec = new IsLargeOrderSpecification(threshold: 50);
var vipCustomerSpec = new CustomerNamePatternSpecification("vip");
var hasItemsSpec = new HasItemsSpecification();

// VIP + Large Order = 25% discount
var vipLargeSpec = vipCustomerSpec.And(largeOrderSpec);
if (order.SatisfiesSpecification(vipLargeSpec))
{
    Console.WriteLine("Apply 25% discount");
}

// Large Order = 15% discount
if (order.SatisfiesSpecification(largeOrderSpec))
{
    Console.WriteLine("Apply 15% discount");
}

// VIP Customer = 10% discount
if (order.SatisfiesSpecification(vipCustomerSpec))
{
    Console.WriteLine("Apply 10% discount");
}

// Has Items = 5% discount
if (order.SatisfiesSpecification(hasItemsSpec))
{
    Console.WriteLine("Apply 5% discount");
}
```

---

## 🏭 Application Layer Usage

### Repository Pattern với Specifications

```csharp
public class CategoryRepository : ICategoryRepository
{
    private readonly DbContext _context;

    public async Task<List<Category>> FindBySpecificationAsync(
        ISpecification<Category> specification)
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .ToListAsync();

        return categories
            .Where(c => specification.IsSatisfiedBy(c))
            .ToList();
    }

    // Specific business queries
    public async Task<List<Category>> FindLargeCategoriesAsync(int threshold = 50)
    {
        var spec = new IsLargeCategorySpecification(threshold);
        return await FindBySpecificationAsync(spec);
    }

    public async Task<List<Category>> FindPopularCategoriesAsync()
    {
        var spec = new IsPopularCategorySpecification(orderThreshold: 100);
        return await FindBySpecificationAsync(spec);
    }
}
```

### Service Layer với Specifications

```csharp
public class CategoryService
{
    private readonly ICategoryRepository _repository;

    public async Task<List<Category>> GetDeletableCategoriesAsync()
    {
        var spec = new CanBeDeletedSpecification();
        return await _repository.FindBySpecificationAsync(spec);
    }

    public async Task<List<Category>> GetLargeCategoriesWithProductsAsync(int threshold = 50)
    {
        var hasProductsSpec = new HasProductsSpecification();
        var isLargeSpec = new IsLargeCategorySpecification(threshold);
        var combinedSpec = hasProductsSpec.And(isLargeSpec);

        return await _repository.FindBySpecificationAsync(combinedSpec);
    }
}
```

### Filter Service với Dynamic Specifications

```csharp
public class OrderFilterService
{
    public List<Order> FilterOrders(List<Order> orders, OrderFilterCriteria criteria)
    {
        ISpecification<Order> specification = new AlwaysTrueSpecification();

        // Dynamic specification building
        if (criteria.MinItems.HasValue)
        {
            specification = specification.And(
                new IsLargeOrderSpecification(criteria.MinItems.Value));
        }

        if (!string.IsNullOrEmpty(criteria.CustomerPattern))
        {
            specification = specification.And(
                new CustomerNamePatternSpecification(criteria.CustomerPattern));
        }

        if (criteria.HasItems)
        {
            specification = specification.And(new HasItemsSpecification());
        }

        return orders
            .Where(order => specification.IsSatisfiedBy(order))
            .ToList();
    }
}

// Usage
var criteria = new OrderFilterCriteria 
{ 
    MinItems = 50, 
    CustomerPattern = "VIP",
    HasItems = true 
};
var filteredOrders = filterService.FilterOrders(allOrders, criteria);
```

---

## 🎯 Best Practices

### ✅ Nên làm

1. **Sử dụng Specifications cho business queries phức tạp**
   ```csharp
   // ✅ Good
   var spec = new IsLargeCategorySpecification(50);
   bool isLarge = category.SatisfiesSpecification(spec);
   ```

2. **Kết hợp specifications với extension methods**
   ```csharp
   // ✅ Good
   var spec = hasProductsSpec.And(isLargeSpec).And(canBeDeletedSpec.Not());
   ```

3. **Tái sử dụng specifications trong Repository/Service**
   ```csharp
   // ✅ Good
   public async Task<List<Category>> GetLargeCategories() 
   {
       var spec = new IsLargeCategorySpecification(50);
       return await _repository.FindBySpecificationAsync(spec);
   }
   ```

### ❌ Không nên làm

1. **Không dùng specifications cho simple property checks**
   ```csharp
   // ❌ Bad - Quá đơn giản
   var spec = new HasProductsSpecification();
   bool hasProducts = category.SatisfiesSpecification(spec);
   
   // ✅ Good - Dùng trực tiếp
   bool hasProducts = category.Products.Any();
   ```

2. **Không tạo specifications cho validation kỹ thuật**
   ```csharp
   // ❌ Bad - Validation kỹ thuật nên ở FluentValidation
   var spec = new NameRequiredSpecification();
   
   // ✅ Good - Validation kỹ thuật ở Application layer (FluentValidation)
   ```

3. **Không tạo specifications cho one-time business rules**
   ```csharp
   // ❌ Bad - Chỉ dùng một lần
   var spec = new VerySpecificOneTimeRuleSpecification();
   
   // ✅ Good - Dùng trực tiếp trong method
   ```

---

## 📚 Tóm tắt

- **ISpecification<T>**: Interface chung từ Contracts layer
- **Tách file**: Mỗi specification trong file riêng để dễ quản lý
- **Composite**: Sử dụng And, Or, Not để kết hợp specifications
- **Extension Methods**: Dễ dàng kết hợp specifications với `.And()`, `.Or()`, `.Not()`
- **Application Layer**: Sử dụng specifications trong Repository và Service
- **Best Practices**: Chỉ dùng cho business queries phức tạp, không dùng cho validation kỹ thuật

