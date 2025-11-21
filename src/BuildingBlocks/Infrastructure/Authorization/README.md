# Simplified PBAC (Policy-Based Access Control)

## Overview

This is a simplified, attribute-based PBAC implementation following best practices:

1. **Simple policy attribute registration** - Use `[Policy("NAME")]` attribute
2. **Easy to extend** - Just create a new policy class with the attribute
3. **Code reuse** - Base policy with helper methods
4. **Clean architecture** - Removed unnecessary complexity

## Quick Start

### 1. Create a Policy

```csharp
using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.Identity;

[Policy("PRODUCT:VIEW", Description = "View products")]
public class ProductViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (IsAuthenticated(user))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow("User is authenticated"));
        }

        return Task.FromResult(PolicyEvaluationResult.Deny("User must be authenticated"));
    }
}
```

### 2. Register Policies (Auto-Discovery)

In your `Program.cs` or startup configuration:

```csharp
services.AddPolicyBasedAuthorization(registry =>
{
    // Scan assemblies for policies with [Policy] attribute
    registry.ScanAssemblies(typeof(ProductViewPolicy).Assembly);
});
```

### 3. Use Policies in Controllers

```csharp
[RequirePolicy("PRODUCT:VIEW")]
public async Task<IActionResult> GetProduct(long id)
{
    // Your code here
}
```

## Helper Methods in BasePolicy

- `HasRole(user, role)` - Check single role
- `HasAnyRole(user, roles...)` - Check any of the roles
- `HasAllRoles(user, roles...)` - Check all roles
- `HasPermission(user, permission)` - Check permission
- `IsAuthenticated(user)` - Check if user is authenticated
- `GetContextValue<T>(context, key)` - Get typed value from context

## Architecture

```
┌─────────────────────────────────────────┐
│  [RequirePolicy] Attribute              │
│  (Mark endpoints requiring policy)      │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  PolicyAuthorizationMiddleware          │
│  (Intercept requests, evaluate policy)  │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  PolicyEvaluator                        │
│  (Find and execute policy)              │
└────────────────┬────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────┐
│  YourPolicy : BasePolicy                │
│  (Custom business logic)                │
└─────────────────────────────────────────┘
```

## Key Files

- **IPolicy.cs** - Core policy interface
- **BasePolicy.cs** - Base implementation with helper methods
- **PolicyEvaluator.cs** - Evaluates policies by name
- **PolicyRegistry.cs** - Auto-discovers policies from assemblies
- **PolicyAttribute.cs** - Marks policies with their name
- **PolicyAuthorizationExtensions.cs** - Service registration
- **PolicyAuthorizationMiddleware.cs** - Enforces policies

## Benefits

✅ **Simple** - One attribute, one base class, no boilerplate  
✅ **Auto-discovery** - No manual registration needed  
✅ **Type-safe** - Compile-time checking of policy names  
✅ **Extensible** - Easy to add new policies  
✅ **Testable** - Simple to unit test policies  
✅ **Clean** - Removed unnecessary complexity

## Documentation

### 📚 Tài liệu chi tiết:
- **[PBAC Guide](../../../../docs/auth/pbac-guide.md)** - Hướng dẫn đầy đủ với workflow và examples
- **[PBAC Cheat Sheet](../../../../docs/auth/pbac-cheatsheet.md)** - Tài liệu tham khảo nhanh
- **[Refactor Summary](../../../../PBAC_REFACTOR_SUMMARY.md)** - Chi tiết về refactoring

### 🎯 Quick Links:
- [Cách sử dụng](../../../../docs/auth/pbac-guide.md#cách-sử-dụng) - Sử dụng policy trong controller
- [Workflow](../../../../docs/auth/pbac-guide.md#workflow) - Luồng xử lý request
- [Implement Policy mới](../../../../docs/auth/pbac-guide.md#implement-policy-mới) - 3 bước tạo policy mới
- [Templates](../../../../docs/auth/pbac-cheatsheet.md#templates) - Copy/paste templates
- [Examples](../../../../docs/auth/pbac-guide.md#ví-dụ-thực-tế) - Ví dụ thực tế

