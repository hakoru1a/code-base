# PBAC Guide - Hướng dẫn sử dụng Policy-Based Access Control

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

---

## 📖 Table of Contents
1. [Cách sử dụng](#cách-sử-dụng)
2. [Workflow](#workflow)
3. [Implement Policy mới](#implement-policy-mới)
4. [Ví dụ thực tế](#ví-dụ-thực-tế)
5. [Helper Methods](#-helper-methods-trong-basepolicy)
6. [Convention đặt tên](#-convention-đặt-tên-policy)
7. [Troubleshooting](#-troubleshooting)
8. [FAQ](#-faq)

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

Không cần làm gì thêm! Policy sẽ tự động được discover và register.

### Bước 3: Sử dụng Policy

```csharp
[HttpGet("{id}")]
[RequirePolicy("INVOICE:VIEW")]
public async Task<IActionResult> GetInvoice(long id)
{
    return Ok();
}
```

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

### Ví dụ 3: Policy với permission check

```csharp
[Policy("FINANCE:APPROVE")]
public class FinanceApprovePolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (HasRole(user, Roles.Manager) && HasPermission(user, "finance:approve"))
            return Task.FromResult(PolicyEvaluationResult.Allow("OK"));
        
        return Task.FromResult(PolicyEvaluationResult.Deny("Permission denied"));
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
        if (HasRole(user, Roles.Admin))
            return Task.FromResult(PolicyEvaluationResult.Allow("Admin can cancel"));

        var orderOwnerId = GetContextValue<string>(context, "OwnerId");
        if (user.UserId == orderOwnerId)
            return Task.FromResult(PolicyEvaluationResult.Allow("User can cancel own order"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Cannot cancel other user's order"));
    }
}
```

---

## 🛠️ Helper Methods trong BasePolicy

| Method | Mô tả |
|--------|-------|
| `IsAuthenticated(user)` | Check user đã login |
| `HasRole(user, role)` | Check 1 role |
| `HasAnyRole(user, ...roles)` | Check có 1 trong các roles (OR) |
| `HasAllRoles(user, ...roles)` | Check có tất cả roles (AND) |
| `HasPermission(user, permission)` | Check permission |
| `GetContextValue<T>(context, key)` | Lấy data từ context |

---

## 📝 Convention đặt tên Policy

### Policy Name Format: `{RESOURCE}:{ACTION}`
**Ví dụ:** `PRODUCT:VIEW`, `ORDER:CANCEL`

### Policy Class Name: `{Resource}{Action}Policy`
**Ví dụ:** `ProductViewPolicy`, `OrderCancelPolicy`

### File Path: `Features/{Resource}/Policies/`
**Ví dụ:** `Features/Product/Policies/`

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

---

## ❓ FAQ

### Q: Policy không được gọi?
**A:** Check:
1. Có `[RequirePolicy("...")]` attribute chưa?
2. Policy name đúng chưa? (Case sensitive!)
3. Policy có `[Policy("...")]` attribute chưa?

### Q: Policy luôn return Deny?
**A:** Check:
1. User có đúng role/permission chưa?
2. Log user claims để debug: `Console.WriteLine($"Roles: {string.Join(", ", user.Roles)}");`

### Q: Làm sao để test policy?
**A:** Unit test:
```csharp
var policy = new InvoiceViewPolicy();
var user = new UserClaimsContext { 
    Roles = new List<string> { Roles.Admin } 
};
var result = await policy.EvaluateAsync(user, new Dictionary<string, object>());
Assert.True(result.IsAllowed);
```

---
## 🚀 Next Steps

1. ✅ Copy một template phù hợp
2. ✅ Đổi tên và logic theo yêu cầu
3. ✅ Sử dụng `[RequirePolicy]` trong controller
4. ✅ Test thử!

---

## 💡 Tips

- 💡 Bắt đầu với template đơn giản nhất
- 💡 Đặt tên policy theo convention
- 💡 Sử dụng helper methods từ BasePolicy
- 💡 Log để debug nếu cần
- 💡 Keep it simple!