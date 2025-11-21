# PBAC Quick Start - Bắt đầu nhanh

> **Policy-Based Access Control** - Hệ thống phân quyền dựa trên policies

## ⚡ Tạo Policy mới trong 30 giây

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

## 🛠️ Helper Methods

```csharp
IsAuthenticated(user)                          // Check đã login
HasRole(user, Roles.Admin)                     // Check 1 role
HasAnyRole(user, Roles.Admin, Roles.Manager)   // Check nhiều roles (OR)
HasAllRoles(user, Roles.Admin, Roles.Premium)  // Check tất cả (AND)
HasPermission(user, "product:delete")          // Check permission
GetContextValue<T>(context, "key")             // Lấy data từ context
```

---

## 📝 Naming Convention

| Element | Format | Example |
|---------|--------|---------|
| Policy Name | `{RESOURCE}:{ACTION}` | `PRODUCT:VIEW` |
| Class Name | `{Resource}{Action}Policy` | `ProductViewPolicy` |
| File Path | `Features/{Resource}/Policies/` | `Features/Product/Policies/` |

---

## 🎯 Common Use Cases

### CRUD Operations
```csharp
[Policy("PRODUCT:VIEW")]    // All authenticated
[Policy("PRODUCT:CREATE")]  // Admin + Manager
[Policy("PRODUCT:UPDATE")]  // Admin + Manager  
[Policy("PRODUCT:DELETE")]  // Admin only
```

### Approval Flow
```csharp
[Policy("ORDER:SUBMIT")]    // User can submit
[Policy("ORDER:APPROVE")]   // Manager can approve
[Policy("ORDER:REJECT")]    // Manager can reject
```

---

## 🔄 Request Flow

```
Request → JWT Valid? → [RequirePolicy] → PolicyEvaluator → YourPolicy
            ↓              ↓                  ↓                ↓
          ❌ 401        ✅ Yes             Found           Allow?
                                            ↓                ↓
                                        Execute         ✅ 200 OK
                                                       or
                                                       ❌ 403 Forbidden
```

---

## 📚 Full Documentation

| Tài liệu | Nội dung | Link |
|----------|----------|------|
| **PBAC Guide** | Hướng dẫn đầy đủ + Workflow + Examples | [📖 Đọc](./docs/auth/pbac-guide.md) |
| **PBAC Cheat Sheet** | Templates + Quick Reference | [⚡ Đọc](./docs/auth/pbac-cheatsheet.md) |
| **Documentation Index** | Tổng hợp tất cả tài liệu | [📚 Đọc](./docs/auth/INDEX.md) |
| **Refactor Summary** | Chi tiết refactoring | [📋 Đọc](./PBAC_REFACTOR_SUMMARY.md) |

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

---

**Cần help?** → Đọc [Full Guide](./docs/auth/pbac-guide.md) hoặc [Cheat Sheet](./docs/auth/pbac-cheatsheet.md)

**Happy coding!** 🎉

