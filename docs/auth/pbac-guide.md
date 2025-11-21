# PBAC Guide - Hướng dẫn sử dụng Policy-Based Access Control

## 📖 Table of Contents
1. [Cách sử dụng](#cách-sử-dụng)
2. [Workflow](#workflow)
3. [Implement Policy mới](#implement-policy-mới)

---

## 🎯 Cách sử dụng

### Sử dụng Policy trong Controller

Đơn giản chỉ cần thêm attribute `[RequirePolicy("POLICY_NAME")]`:

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
        // Nếu policy fail → return 403 Forbidden
        return Ok(product);
    }

    [HttpPost]
    [RequirePolicy("PRODUCT:CREATE")]
    public async Task<IActionResult> CreateProduct(ProductDto dto)
    {
        // Chỉ user có quyền tạo product mới vào được
        return Ok();
    }

    [HttpPut("{id}")]
    [RequirePolicy("PRODUCT:UPDATE")]
    public async Task<IActionResult> UpdateProduct(long id, ProductDto dto)
    {
        return Ok();
    }

    [HttpDelete("{id}")]
    [RequirePolicy("PRODUCT:DELETE")]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        return Ok();
    }
}
```

### Sử dụng nhiều Policies

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

## 🔄 Workflow

### Luồng xử lý khi có request:

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Client gửi request + JWT Token                          │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. Authentication Middleware                                │
│    - Validate JWT token                                     │
│    - Extract claims (userId, roles, permissions)            │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Authorization Middleware (RBAC)                          │
│    - Check [Authorize] attribute                            │
│    - Verify roles if needed                                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. PolicyAuthorizationMiddleware                            │
│    - Check [RequirePolicy] attribute                        │
│    - Get policy name (e.g., "PRODUCT:VIEW")                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. PolicyEvaluator                                          │
│    - Find policy by name from registry                      │
│    - Get policy instance from DI container                  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. ProductViewPolicy.EvaluateAsync()                        │
│    - Check business logic                                   │
│    - Check roles/permissions                                │
│    - Return Allow or Deny                                   │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
         ┌─────────────┴─────────────┐
         │                           │
         ▼                           ▼
    ✅ Allow                    ❌ Deny
    Continue to                 Return 403 Forbidden
    Controller                  {
                                  "error": "Forbidden",
                                  "message": "...",
                                  "policy": "PRODUCT:VIEW"
                                }
```

### Minh họa cụ thể:

**Request:**
```http
GET /api/product/123
Authorization: Bearer eyJhbGc...
```

**Flow:**
1. ✅ JWT valid → Extract user claims
2. ✅ User authenticated → Continue
3. ✅ Check `[RequirePolicy("PRODUCT:VIEW")]` → Found
4. 🔍 Find `ProductViewPolicy` in registry
5. 🔍 Execute `ProductViewPolicy.EvaluateAsync()`
   - Check: `if (IsAuthenticated(user))` → ✅ True
6. ✅ Policy Allow → Continue to controller
7. ✅ Return product data

---

## 🚀 Implement Policy mới

### Bước 1: Tạo Policy Class

Tạo file mới trong thư mục `Features/{Resource}/Policies/`:

```csharp
using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

namespace Generate.Application.Features.Invoice.Policies
{
    /// <summary>
    /// Policy cho việc xem invoice
    /// </summary>
    [Policy("INVOICE:VIEW", Description = "View invoices")]
    public class InvoiceViewPolicy : BasePolicy
    {
        public override Task<PolicyEvaluationResult> EvaluateAsync(
            UserClaimsContext user,
            Dictionary<string, object> context)
        {
            // Business logic ở đây
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

Không cần làm gì thêm! Policy sẽ tự động được discover và register nhờ:

```csharp
// Trong Generate.API/Extensions/AuthenticationExtension.cs
services.AddPolicyBasedAuthorization(registry =>
{
    registry.ScanAssemblies(typeof(ProductViewPolicy).Assembly);
    // ↑ Tự động scan tất cả policies trong assembly này
});
```

### Bước 3: Sử dụng Policy

```csharp
[HttpGet("{id}")]
[RequirePolicy("INVOICE:VIEW")]
public async Task<IActionResult> GetInvoice(long id)
{
    return Ok();
}
```

**Xong!** 🎉

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
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "User is authenticated"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Authentication required"));
        }
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
        // Chỉ Admin và Manager mới export được
        if (HasAnyRole(user, Roles.Admin, Roles.Manager))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "User has required role"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Only Admin or Manager can export reports"));
    }
}
```

### Ví dụ 3: Policy với permission check

```csharp
[Policy("FINANCE:APPROVE")]
public class FinanceApprovePolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Cần cả role VÀ permission
        if (HasRole(user, Roles.Manager))
        {
            if (HasPermission(user, "finance:approve"))
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "User has role and permission"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                "Manager role but missing finance:approve permission"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Manager role required"));
    }
}
```

### Ví dụ 4: Policy với context data

```csharp
[Policy("ORDER:CANCEL")]
public class OrderCancelPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Admin luôn được cancel
        if (HasRole(user, Roles.Admin))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "Admin can cancel any order"));
        }

        // User thường chỉ cancel được order của mình
        var orderOwnerId = GetContextValue<string>(context, "OwnerId");
        if (user.UserId == orderOwnerId)
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "User can cancel own order"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Cannot cancel other user's order"));
    }
}
```

### Ví dụ 5: Policy phức tạp với business logic

```csharp
[Policy("DISCOUNT:APPLY")]
public class DiscountApplyPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Admin không bị giới hạn
        if (HasRole(user, Roles.Admin))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "Admin unlimited discount"));
        }

        // Manager có thể apply discount <= 20%
        if (HasRole(user, Roles.Manager))
        {
            var discountPercent = GetContextValue<decimal>(context, "DiscountPercent");
            if (discountPercent <= 20)
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    "Manager can apply discount up to 20%"));
            }

            return Task.FromResult(PolicyEvaluationResult.Deny(
                $"Manager cannot apply {discountPercent}% discount (max 20%)"));
        }

        // User thường không được apply discount
        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Only Manager or Admin can apply discounts"));
    }
}
```

---

## 🛠️ Helper Methods trong BasePolicy

| Method | Mô tả | Ví dụ |
|--------|-------|-------|
| `IsAuthenticated(user)` | Check user đã login | `if (IsAuthenticated(user))` |
| `HasRole(user, role)` | Check 1 role | `HasRole(user, Roles.Admin)` |
| `HasAnyRole(user, ...roles)` | Check có 1 trong các roles | `HasAnyRole(user, Roles.Admin, Roles.Manager)` |
| `HasAllRoles(user, ...roles)` | Check có tất cả roles | `HasAllRoles(user, Roles.Admin, Roles.Premium)` |
| `HasPermission(user, permission)` | Check permission | `HasPermission(user, "product:delete")` |
| `GetContextValue<T>(context, key)` | Lấy data từ context | `GetContextValue<string>(context, "OwnerId")` |

---

## 📝 Convention đặt tên Policy

### Policy Name Format:
```
{RESOURCE}:{ACTION}
```

**Ví dụ:**
- `PRODUCT:VIEW` - Xem product
- `PRODUCT:CREATE` - Tạo product
- `PRODUCT:UPDATE` - Cập nhật product
- `PRODUCT:DELETE` - Xóa product
- `ORDER:CANCEL` - Hủy order
- `ORDER:APPROVE` - Phê duyệt order
- `INVOICE:EXPORT` - Export invoice
- `REPORT:DOWNLOAD` - Download report

### Policy Class Name:
```
{Resource}{Action}Policy
```

**Ví dụ:**
- `ProductViewPolicy`
- `ProductCreatePolicy`
- `OrderCancelPolicy`
- `InvoiceExportPolicy`

---

## ⚡ Quick Reference

### Tạo Policy mới trong 3 bước:

```bash
# 1. Tạo file Policy
src/Services/Generate/Generate.Application/Features/{Resource}/Policies/{Resource}{Action}Policy.cs

# 2. Code Policy
[Policy("{RESOURCE}:{ACTION}")]
public class {Resource}{Action}Policy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(...)
    {
        // Business logic
    }
}

# 3. Sử dụng trong Controller
[RequirePolicy("{RESOURCE}:{ACTION}")]
public async Task<IActionResult> {Action}{Resource}() { }
```

**Xong!** Không cần register thủ công! 🎉

---

## 🔍 Troubleshooting

### Policy không được gọi?

✅ Check:
1. Có `[RequirePolicy("...")]` attribute chưa?
2. Policy name đúng chưa? (Case sensitive)
3. Policy có `[Policy("...")]` attribute chưa?
4. Assembly đã được scan chưa? (Check `AuthenticationExtension.cs`)

### Policy luôn return Deny?

✅ Check:
1. Log để xem user claims
2. Verify roles/permissions trong JWT token
3. Check business logic trong `EvaluateAsync()`

### Policy không được discover?

✅ Check:
1. Policy class có inherit `BasePolicy` chưa?
2. Policy class có `[Policy]` attribute chưa?
3. Assembly có được scan trong `AddPolicyBasedAuthorization()` chưa?

---

## 📚 Tài liệu thêm

- [Authorization README](../../src/BuildingBlocks/Infrastructure/Authorization/README.md)
- [PBAC Refactor Summary](../../PBAC_REFACTOR_SUMMARY.md)
- [JWT Claims Authorization](./jwt-claims-authorization.md)

