# PBAC Guide

## 📖 Table of Contents
1. [Cách sử dụng](#-cách-sử-dụng)
2. [Implement Policy mới](#-implement-policy-mới)
3. [Ví dụ thực tế](#-ví-dụ-thực-tế)
4. [Helper Methods trong BasePolicy](#️-helper-methods-trong-basepolicy)
5. [Convention đặt tên](#-convention-đặt-tên-policy)
6. [Troubleshooting & FAQ](#-troubleshooting--faq)

---

## 🎯 Cách sử dụng

### Sử dụng Policy trong Controller

Đơn giản chỉ cần thêm attribute `[RequirePolicy("POLICY_NAME")]` vào action hoặc controller.

```csharp
using Shared.Attributes;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [HttpGet("{id}")]
    [RequirePolicy("PRODUCT:VIEW")]  // ← Thêm attribute này
    public async Task<IActionResult> GetProduct(long id)
    {
        // Nếu policy pass → code này chạy
        // Nếu policy fail → framework tự động trả về 403 Forbidden
        return Ok(product);
    }

    [HttpPost]
    [RequirePolicy("PRODUCT:CREATE")]
    public async Task<IActionResult> CreateProduct(ProductDto dto)
    {
        // Chỉ user có quyền tạo product mới vào được
        return Ok();
    }
}
```

### Sử dụng nhiều Policies (logic AND)

Nếu bạn cần nhiều quyền để truy cập một endpoint, hãy thêm nhiều attribute. User sẽ phải thỏa mãn **tất cả** các policy.

```csharp
// Yêu cầu cả 2 policies đều pass
[RequirePolicy("PRODUCT:VIEW")]
[RequirePolicy("CATEGORY:VIEW")]
public async Task<IActionResult> GetProductWithCategory(long id)
{
    return Ok();
}
```
---

## 🚀 Implement Policy mới

### Bước 1: Tạo Policy Class

Tạo file mới trong thư mục `Features/{Resource}/Policies/`. Policy phải kế thừa từ `BasePolicy`.

```csharp
// Vị trí: src/Services/Generate/Application/Features/Invoice/Policies/InvoiceViewPolicy.cs
using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Invoice.Policies
{
    [Policy("INVOICE:VIEW", Description = "View invoices")]
    public class InvoiceViewPolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            if (IsAuthenticated(user))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "User is authenticated"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated"));
        }
    }
}
```

### Bước 2: Policy đã tự động register! ✅

Hệ thống sử dụng cơ chế auto-discovery. Miễn là policy của bạn được đánh dấu với `[Policy]` attribute và nằm trong assembly được scan, nó sẽ được tự động đăng ký.

### Bước 3: Sử dụng Policy trong Controller

Như đã hướng dẫn ở trên, chỉ cần thêm `[RequirePolicy("...")]` vào controller hoặc action.

---

## 📋 Ví dụ thực tế

### Ví dụ 1: Policy đơn giản - Chỉ cần authenticated

```csharp
[Policy("DASHBOARD:VIEW")]
public class DashboardViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (IsAuthenticated(user))
            return Task.FromResult(PolicyEvaluationResult.Allow("OK"));
        
        return Task.FromResult(PolicyEvaluationResult.Deny("Authentication required"));
    }
}
```

### Ví dụ 2: Policy với role check

```csharp
[Policy("REPORT:EXPORT")]
public class ReportExportPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (HasAnyRole(user, Roles.Admin, Roles.Manager))
            return Task.FromResult(PolicyEvaluationResult.Allow("OK"));
        
        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Only Admin or Manager can export reports"));
    }
}
```

### Ví dụ 3: Policy với context data (kiểm tra ownership)

```csharp
[Policy("ORDER:CANCEL")]
public class OrderCancelPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Admin luôn có quyền
        if (HasRole(user, Roles.Admin))
            return Task.FromResult(PolicyEvaluationResult.Allow("Admin can cancel any order"));

        // User có thể hủy order của chính mình
        var orderOwnerId = GetContextValue<string>(context, "OwnerId");
        if (user.UserId == orderOwnerId)
            return Task.FromResult(PolicyEvaluationResult.Allow("User can cancel their own order"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Cannot cancel an order owned by another user"));
    }
}
```
---

## 🛠️ Helper Methods trong BasePolicy

`BasePolicy` cung cấp các hàm tiện ích để đơn giản hóa logic trong `EvaluateAsync`.

| Method | Mô tả |
|--------|-------|
| `IsAuthenticated(user)` | Check user đã login |
| `HasRole(user, role)` | Check 1 role |
| `HasAnyRole(user, ...roles)` | Check có 1 trong các roles (OR) |
| `HasAllRoles(user, ...roles)` | Check có tất cả roles (AND) |
| `HasPermission(user, permission)` | Check permission |
| `GetContextValue<T>(context, key)` | Lấy data từ context một cách an toàn |

---

## 📝 Convention đặt tên

Việc tuân thủ convention giúp hệ thống dễ quản lý và dễ hiểu.

### Policy Name Format: `{RESOURCE}:{ACTION}`
- **Ví dụ:** `PRODUCT:VIEW`, `ORDER:CANCEL`, `REPORT:EXPORT`

### Policy Class Name: `{Resource}{Action}Policy`
- **Ví dụ:** `ProductViewPolicy`, `OrderCancelPolicy`, `ReportExportPolicy`

### File Path: `src/Services/{Service}/Application/Features/{Resource}/Policies/`
- **Ví dụ:** `.../Features/Product/Policies/ProductViewPolicy.cs`

---

## 🔍 Troubleshooting & FAQ

### Q: Policy của tôi không được gọi?
**A:** Kiểm tra lại:
1.  Attribute `[RequirePolicy("...")]` đã được thêm vào action/controller chưa?
2.  Tên policy trong attribute có **khớp chính xác** (case-sensitive) với tên trong `[Policy("...")]` attribute của class policy không?
3.  Assembly chứa policy có được scan lúc khởi động không?

### Q: Policy luôn trả về Deny?
**A:** Debug bằng cách:
1.  Đặt breakpoint trong hàm `EvaluateAsync` của policy.
2.  Kiểm tra các giá trị trong `user` object (UserId, Roles, Permissions, Claims) để chắc chắn chúng đúng như mong đợi từ JWT token.
3.  Kiểm tra logic bên trong `EvaluateAsync`.

### Q: Làm sao để test một policy?
**A:** Viết unit test cho policy để kiểm tra logic một cách độc lập.
```csharp
[Fact]
public void OrderCancelPolicy_Owner_Should_Allow()
{
    // Arrange
    var policy = new OrderCancelPolicy();
    var user = new UserClaimsContext { UserId = "user123" };
    var context = new Dictionary<string, object> { { "OwnerId", "user123" } };

    // Act
    var result = await policy.EvaluateAsync(user, context);

    // Assert
    Assert.True(result.IsAllowed);
}
```
