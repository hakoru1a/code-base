# PBAC Quick Start

## ⚡ Quick Start - Tạo Policy trong 30 giây

### 1. Tạo file Policy (10s)

```csharp
// File: Features/Invoice/Policies/InvoiceViewPolicy.cs
using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

[Policy("INVOICE:VIEW", Description = "View invoices")]
public class InvoiceViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (HasRole(user, Roles.Admin))
            return Task.FromResult(PolicyEvaluationResult.Allow("Admin access"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Admin only"));
    }
}
```

### 2. Policy tự động register ✅ (0s)
**Không cần làm gì!** Auto-discovery hoạt động tự động.

### 3. Sử dụng trong Controller (20s)

```csharp
[RequirePolicy("INVOICE:VIEW")]
public async Task<IActionResult> GetInvoice(long id)
{
    // Your code here
    return Ok(invoice);
}
```

**Xong!** 🎉

---

## Advanced Quick Start: Dynamic Filtering with FilterContext

Sử dụng `FilterContext` để lọc dữ liệu động dựa trên claims trong JWT.

### 1. Policy trả về `FilterContext`

```csharp
[Policy("PRODUCT:VIEW")]
public class ProductViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, Dictionary<string, object> context)
    {
        var filterContext = new ProductFilterContext();

        // Admin/Manager bypass all filters
        if (HasAnyRole(user, "admin", "manager"))
        {
            filterContext.CanViewAll = true;
            return Task.FromResult(PolicyEvaluationResult.Allow("Admin/Manager can view all products", filterContext));
        }

        // Extract max_product_price từ JWT claims
        if (user.Claims.TryGetValue("max_product_price", out var maxPriceStr) && 
            decimal.TryParse(maxPriceStr, out var maxPrice))
        {
            filterContext.MaxPrice = maxPrice;
        }

        // Extract department filter
        if (user.CustomAttributes.TryGetValue("department", out var department))
        {
            filterContext.DepartmentFilter = department.ToString();
        }
        
        return Task.FromResult(PolicyEvaluationResult.Allow("User authenticated with applied filters", filterContext));
    }
}
```

### 2. Định nghĩa các `FilterContext`

`ProductFilterContext` chứa các thuộc tính để lọc sản phẩm.

```csharp
public class ProductFilterContext : IPolicyFilterContext
{
    // Price filtering từ JWT claims
    public decimal? MaxPrice { get; set; }          // max_product_price
    public decimal? MinPrice { get; set; }          // min_product_price
    
    // Department/Region filtering
    public string? DepartmentFilter { get; set; }   // department claim
    public string? RegionFilter { get; set; }       // region claim
    
    // Category restrictions from permissions
    public List<string>? AllowedCategories { get; set; } 
    
    // Admin privileges
    public bool CanViewAll { get; set; } = false;
}
```

### 3. Query Handler áp dụng filter

```csharp
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductResponseDto>>
{
    // ... constructor ...

    public async Task<List<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _productRepository.FindAll();

        // Get filter context từ policy evaluation
        var filterContext = _httpContextAccessor.HttpContext?.GetProductFilterContext();
        if (filterContext != null && !filterContext.CanViewAll)
        {
            // Apply max price filter
            if (filterContext.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filterContext.MaxPrice.Value);
            }

            // Apply department filter
            if (!string.IsNullOrEmpty(filterContext.DepartmentFilter))
            {
                query = query.Where(p => p.Department == filterContext.DepartmentFilter);
            }
        }

        // ...
        return await query.ToListAsync(cancellationToken);
    }
}
```

---

## 📋 Copy/Paste Templates

### Template 1: Chỉ cần authenticated
```csharp
[Policy("RESOURCE:ACTION")]
public class ResourceActionPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user, Dictionary<string, object> context)
    {
        if (IsAuthenticated(user))
            return Task.FromResult(PolicyEvaluationResult.Allow("OK"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Must be authenticated"));
    }
}
```

### Template 2: Check role
```csharp
[Policy("RESOURCE:ACTION")]
public class ResourceActionPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user, Dictionary<string, object> context)
    {
        if (HasAnyRole(user, Roles.Admin, Roles.Manager))
            return Task.FromResult(PolicyEvaluationResult.Allow("Has required role"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Admin or Manager required"));
    }
}
```

### Template 3: Check permission
```csharp
[Policy("RESOURCE:ACTION")]
public class ResourceActionPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user, Dictionary<string, object> context)
    {
        if (HasPermission(user, Permissions.Resource.Action))
            return Task.FromResult(PolicyEvaluationResult.Allow("Has permission"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Permission required"));
    }
}
```
