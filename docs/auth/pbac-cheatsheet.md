# PBAC Cheat Sheet - Tài liệu tham khảo nhanh

## 🚀 Quick Start (3 bước)

### 1️⃣ Tạo Policy (1 file)

```csharp
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
            return Task.FromResult(PolicyEvaluationResult.Allow("OK"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Denied"));
    }
}
```

### 2️⃣ Policy tự động register ✅
Không cần làm gì! Auto-discovery đã hoạt động.

### 3️⃣ Sử dụng trong Controller

```csharp
[RequirePolicy("INVOICE:VIEW")]
public async Task<IActionResult> GetInvoice(long id) { }
```

---

## 📖 Templates

### Template 1: Authentication Only

```csharp
[Policy("RESOURCE:ACTION")]
public class ResourceActionPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user, Dictionary<string, object> context)
    {
        if (IsAuthenticated(user))
            return Task.FromResult(PolicyEvaluationResult.Allow("Authenticated"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Must be authenticated"));
    }
}
```

### Template 2: Role-Based

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

### Template 3: Permission-Based

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

### Template 4: Role + Permission

```csharp
[Policy("RESOURCE:ACTION")]
public class ResourceActionPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user, Dictionary<string, object> context)
    {
        if (HasRole(user, Roles.Admin))
        {
            if (HasPermission(user, Permissions.Resource.Action))
                return Task.FromResult(PolicyEvaluationResult.Allow("Has role and permission"));

            return Task.FromResult(PolicyEvaluationResult.Deny("Has role but missing permission"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny("Admin role required"));
    }
}
```

### Template 5: With Context Data

```csharp
[Policy("RESOURCE:ACTION")]
public class ResourceActionPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user, Dictionary<string, object> context)
    {
        // Admin bypass
        if (HasRole(user, Roles.Admin))
            return Task.FromResult(PolicyEvaluationResult.Allow("Admin"));

        // Check ownership
        var ownerId = GetContextValue<string>(context, "OwnerId");
        if (user.UserId == ownerId)
            return Task.FromResult(PolicyEvaluationResult.Allow("Owner"));

        return Task.FromResult(PolicyEvaluationResult.Deny("Not owner"));
    }
}
```

---

## 🛠️ Helper Methods

```csharp
// Authentication
IsAuthenticated(user)                          // Check logged in

// Roles
HasRole(user, Roles.Admin)                     // Check single role
HasAnyRole(user, Roles.Admin, Roles.Manager)   // Check any role
HasAllRoles(user, Roles.Admin, Roles.Premium)  // Check all roles

// Permissions
HasPermission(user, "product:delete")          // Check permission

// Context
GetContextValue<string>(context, "OwnerId")    // Get context data
GetContextValue<decimal>(context, "Amount")    // Get typed value
```

---

## 📝 Naming Convention

### Policy Name: `{RESOURCE}:{ACTION}`
```
PRODUCT:VIEW      PRODUCT:CREATE    PRODUCT:UPDATE    PRODUCT:DELETE
ORDER:VIEW        ORDER:CREATE      ORDER:UPDATE      ORDER:DELETE
INVOICE:VIEW      INVOICE:EXPORT    INVOICE:APPROVE   INVOICE:REJECT
```

### Class Name: `{Resource}{Action}Policy`
```
ProductViewPolicy      ProductCreatePolicy
OrderViewPolicy        OrderCancelPolicy
InvoiceExportPolicy    InvoiceApprovePolicy
```

### File Location:
```
src/Services/{Service}/{Service}.Application/
  Features/
    {Resource}/
      Policies/
        {Resource}{Action}Policy.cs
```

---

## 🎯 Common Patterns

### Pattern 1: CRUD Operations

```csharp
[Policy("PRODUCT:VIEW")]    // All authenticated
[Policy("PRODUCT:CREATE")]  // Admin + Manager
[Policy("PRODUCT:UPDATE")]  // Admin + Manager
[Policy("PRODUCT:DELETE")]  // Admin only
```

### Pattern 2: Approval Flow

```csharp
[Policy("ORDER:CREATE")]    // All authenticated
[Policy("ORDER:SUBMIT")]    // All authenticated
[Policy("ORDER:APPROVE")]   // Manager + Admin
[Policy("ORDER:REJECT")]    // Manager + Admin
```

### Pattern 3: Export/Report

```csharp
[Policy("REPORT:VIEW")]     // All authenticated
[Policy("REPORT:EXPORT")]   // Manager + Admin
[Policy("REPORT:DELETE")]   // Admin only
```

---

## 💡 Best Practices

### ✅ DO

```csharp
// Clear, descriptive names
[Policy("PRODUCT:VIEW")]

// Simple, focused logic
if (IsAuthenticated(user))
    return Allow();

// Descriptive messages
return Deny("Only administrators can delete products");

// Use helper methods
if (HasAnyRole(user, Roles.Admin, Roles.Manager))
```

### ❌ DON'T

```csharp
// Vague names
[Policy("CHECK_STUFF")]

// Complex, nested logic
if (a && b || (c && !d) || (e && f && g))

// Generic messages
return Deny("No");

// Duplicate code
if (user.Roles.Contains("admin") || user.Roles.Contains("manager"))
```

---

## 🔄 Request Flow (Simple)

```
Request → JWT Valid? → [RequirePolicy] → Find Policy → Evaluate
                ↓           ↓               ↓             ↓
              ❌ 401      ✅ Yes          Found        Allow/Deny
                                          ↓             ↓
                                      Execute         ✅ Controller
                                                     or
                                                     ❌ 403
```

---

## 📱 Usage in Controllers

### Single Policy
```csharp
[RequirePolicy("PRODUCT:VIEW")]
public async Task<IActionResult> Get(long id) { }
```

### Multiple Policies (AND logic)
```csharp
[RequirePolicy("PRODUCT:VIEW")]
[RequirePolicy("CATEGORY:VIEW")]
public async Task<IActionResult> GetWithCategory(long id) { }
```

### Apply to entire controller
```csharp
[RequirePolicy("ADMIN:ACCESS")]
public class AdminController : ControllerBase
{
    // All actions require ADMIN:ACCESS
}
```

---

## 🐛 Debugging

### Check Policy được gọi chưa?
```csharp
public override Task<PolicyEvaluationResult> EvaluateAsync(...)
{
    Console.WriteLine($"Policy called for user: {user.UserId}");
    // Your logic
}
```

### Check User Claims
```csharp
Console.WriteLine($"UserId: {user.UserId}");
Console.WriteLine($"Roles: {string.Join(", ", user.Roles)}");
Console.WriteLine($"Permissions: {string.Join(", ", user.Permissions)}");
```

### Check Context Data
```csharp
foreach (var item in context)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
```

---

## 📊 Response Examples

### Success (200 OK)
```json
{
  "id": 123,
  "name": "Product Name"
}
```

### Policy Denied (403 Forbidden)
```json
{
  "error": "Forbidden",
  "message": "Only administrators can delete products",
  "policy": "PRODUCT:DELETE",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

### Not Authenticated (401 Unauthorized)
```json
{
  "error": "Unauthorized",
  "message": "Authentication required"
}
```

---

## 🎓 Examples by Use Case

### E-commerce
```csharp
[Policy("PRODUCT:VIEW")]      // Browse products
[Policy("CART:ADD")]          // Add to cart
[Policy("ORDER:CREATE")]      // Place order
[Policy("ORDER:CANCEL")]      // Cancel own order
[Policy("REFUND:REQUEST")]    // Request refund
```

### CMS
```csharp
[Policy("POST:VIEW")]         // View posts
[Policy("POST:CREATE")]       // Create post
[Policy("POST:PUBLISH")]      // Publish post
[Policy("POST:DELETE")]       // Delete post
[Policy("COMMENT:MODERATE")]  // Moderate comments
```

### Financial
```csharp
[Policy("TRANSACTION:VIEW")]     // View transactions
[Policy("TRANSACTION:CREATE")]   // Create transaction
[Policy("TRANSACTION:APPROVE")]  // Approve (Manager)
[Policy("TRANSACTION:AUDIT")]    // Audit (Admin)
```

---

## 🔗 Links

- 📖 [Full Guide](./pbac-guide.md) - Chi tiết đầy đủ
- 📝 [Authorization README](../../src/BuildingBlocks/Infrastructure/Authorization/README.md)
- 📋 [Refactor Summary](../../PBAC_REFACTOR_SUMMARY.md)

---

**Tip:** Copy một template phù hợp, đổi tên, và bắt đầu code! 🚀

