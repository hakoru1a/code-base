# Specification Pattern - Quick Start Guide

## 🚀 Bắt đầu nhanh

Hướng dẫn nhanh cách sử dụng Specification Pattern trong project.

## 📦 Import cần thiết

```csharp
using Contracts.Domain.Interface;              // ISpecification<T>
using Contracts.Domain.Specifications;       // Extension methods (And, Or, Not)
using Generate.Domain.Categories.Specifications;  // Category specifications
using Generate.Domain.Orders.Specifications;      // Order specifications
using Generate.Domain.Products.Specifications;   // Product specifications
```

## 🎯 3 bước sử dụng cơ bản

### Bước 1: Tạo Specification

```csharp
var canBeDeletedSpec = new CanBeDeletedSpecification();
```

### Bước 2: Kiểm tra Entity

```csharp
var category = Category.Create("Electronics");
bool result = category.SatisfiesSpecification(canBeDeletedSpec);
```

### Bước 3: Sử dụng kết quả

```csharp
if (result)
{
    // Category có thể xóa
    await _repository.DeleteAsync(category);
}
```

## 💡 Ví dụ thực tế

### Ví dụ 1: Kiểm tra Category có thể xóa

```csharp
public async Task DeleteCategoryIfPossibleAsync(long categoryId)
{
    var category = await _repository.GetByIdAsync(categoryId);
    
    var canBeDeletedSpec = new CanBeDeletedSpecification();
    
    if (category.SatisfiesSpecification(canBeDeletedSpec))
    {
        await _repository.DeleteAsync(category);
    }
    else
    {
        throw new BusinessException("Cannot delete category with products");
    }
}
```

### Ví dụ 2: Tìm các Order lớn

```csharp
public async Task<List<Order>> GetLargeOrdersAsync(int threshold = 50)
{
    var orders = await _repository.GetAllAsync();
    
    var largeOrderSpec = new IsLargeOrderSpecification(threshold);
    
    return orders
        .Where(order => order.SatisfiesSpecification(largeOrderSpec))
        .ToList();
}
```

### Ví dụ 3: Kết hợp nhiều điều kiện

```csharp
public async Task<List<Category>> GetLargeCategoriesWithProductsAsync()
{
    var categories = await _repository.GetAllAsync();
    
    var hasProductsSpec = new HasProductsSpecification();
    var isLargeSpec = new IsLargeCategorySpecification(threshold: 50);
    
    // Kết hợp: Có products VÀ là large category
    var combinedSpec = hasProductsSpec.And(isLargeSpec);
    
    return categories
        .Where(c => c.SatisfiesSpecification(combinedSpec))
        .ToList();
}
```

## 🔗 Kết hợp Specifications

### AND - Cả hai phải thỏa mãn

```csharp
var spec1 = new HasProductsSpecification();
var spec2 = new IsLargeCategorySpecification(50);

var combined = spec1.And(spec2);
// Kết quả: true khi category CÓ products VÀ là large category
```

### OR - Một trong hai thỏa mãn

```csharp
var spec1 = new HasProductsSpecification();
var spec2 = new CanBeDeletedSpecification();

var combined = spec1.Or(spec2);
// Kết quả: true khi category CÓ products HOẶC có thể xóa
```

### NOT - Phủ định

```csharp
var spec = new HasProductsSpecification();

var notSpec = spec.Not();
// Kết quả: true khi category KHÔNG có products
```

## 📋 Danh sách Specifications có sẵn

### Category Specifications
- `CanBeDeletedSpecification` - Có thể xóa không
- `HasProductsSpecification` - Có products không
- `IsLargeCategorySpecification` - Là large category không
- `ContainsProductSpecification` - Chứa product cụ thể không
- `CategoryNamePatternSpecification` - Name có chứa pattern không
- `IsPopularCategorySpecification` - Là popular category không
- `HasActiveProductsSpecification` - Có active products không

### Order Specifications
- `CanBeDeletedSpecification` - Có thể xóa không
- `HasItemsSpecification` - Có items không
- `IsLargeOrderSpecification` - Là large order không
- `ContainsProductSpecification` - Chứa product cụ thể không
- `OrderValueRangeSpecification` - Value trong khoảng không
- `CustomerNamePatternSpecification` - Customer name có chứa pattern không

### Product Specifications
- `CanBeDeletedSpecification` - Có thể xóa không
- `IsInCategorySpecification` - Có trong category không
- `BelongsToCategorySpecification` - Thuộc category cụ thể không
- `IsPopularProductSpecification` - Là popular product không
- `IsHighVolumeProductSpecification` - Là high volume product không
- `ProductNamePatternSpecification` - Name có chứa pattern không
- `HasOrderItemsSpecification` - Có order items không
- `HasProductDetailSpecification` - Có product detail không
- `HasOrdersInDateRangeSpecification` - Có orders trong date range không

## 🎓 Tips

1. **Đọc tên specification**: Tên đã mô tả rõ chức năng
   ```csharp
   new CanBeDeletedSpecification()  // Rõ ràng: kiểm tra có thể xóa
   ```

2. **Sử dụng extension methods**: Dễ đọc hơn
   ```csharp
   spec1.And(spec2)  // ✅ Dễ đọc
   new AndSpecification(spec1, spec2)  // ❌ Dài dòng
   ```

3. **Tái sử dụng**: Tạo specification một lần, dùng nhiều lần
   ```csharp
   var spec = new IsLargeCategorySpecification(50);
   // Dùng spec cho nhiều categories
   ```

## 📖 Xem thêm

- [Specification Usage Examples](./specification-usage-examples.md) - Hướng dẫn chi tiết với nhiều ví dụ

