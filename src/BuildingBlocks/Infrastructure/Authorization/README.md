# Authorization System Documentation

## Overview

This codebase implements a **layered authorization architecture** combining:

1. **RBAC (Role-Based Access Control)** at the Gateway/Controller level
2. **PBAC (Policy-Based Access Control)** at the Service level

This dual-layer approach provides both **coarse-grained** and **fine-grained** access control.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway / Controller                 │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  RBAC Layer (Coarse-grained)                        │  │
│  │  [Authorize(Policy = PolicyNames.Rbac.AdminOnly)]   │  │
│  │                                                      │  │
│  │  ✓ Quick role-based checks                          │  │
│  │  ✓ Prevents unauthorized access early               │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                    Service Layer                            │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  PBAC Layer (Fine-grained)                          │  │
│  │  IProductPolicyService.CanViewProductAsync()        │  │
│  │                                                      │  │
│  │  ✓ Complex business rules                           │  │
│  │  ✓ Context-aware authorization                      │  │
│  │  ✓ Dynamic configuration from JWT claims            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Components

### 1. RBAC (Role-Based Access Control)

**Location**: Gateway/Controller level  
**Implementation**: ASP.NET Core's built-in Authorization

#### Usage

```csharp
[Authorize(Policy = PolicyNames.Rbac.AdminOnly)]
public async Task<IActionResult> DeleteProduct(long id)
{
    // Only admins can access this endpoint
}
```

#### Available RBAC Policies

| Policy Name | Required Roles | Use Case |
|------------|---------------|----------|
| `AdminOnly` | admin | Admin-only operations |
| `ManagerOrAdmin` | admin, manager, product_manager | Management operations |
| `PremiumUser` | premium_user, admin | Premium features |
| `BasicUser` | basic_user, premium_user, admin | General access |
| `AuthenticatedUser` | Any authenticated user | Public authenticated endpoints |

**Configuration**: `Infrastructure/Extentions/KeycloakAuthenticationExtensions.cs`

---

### 2. PBAC (Policy-Based Access Control)

**Location**: Service level  
**Implementation**: Custom policy engine with evaluators

#### Usage

**Option A: Service-based (Recommended)**

```csharp
[HttpGet("{id}")]
[Authorize(Policy = PolicyNames.Rbac.BasicUser)] // RBAC first
public async Task<IActionResult> GetProduct(long id)
{
    var product = await _mediator.Send(new GetProductByIdQuery { Id = id });
    
    // PBAC check with context
    var policyCheck = await _productPolicyService.CanViewProductAsync(
        id, 
        product.Price);
    
    if (!policyCheck.IsAllowed)
    {
        return Forbid(policyCheck.Reason);
    }
    
    return Ok(product);
}
```

**Option B: Attribute-based (For middleware)**

```csharp
[HttpGet]
[RequirePolicy(PolicyNames.Pbac.Product.View)]
public async Task<IActionResult> GetProducts()
{
    // Middleware will evaluate policy before reaching here
}
```

#### Policy Naming Convention

Format: `{RESOURCE}:{ACTION}`

Examples:
- `PRODUCT:VIEW`
- `PRODUCT:CREATE`
- `ORDER:APPROVE`

**Constants**: `Shared/Identity/PolicyNames.Pbac`

---

## Creating Custom Policies

### 1. Simple Policy (Untyped Context)

```csharp
public class ProductDeletePolicy : BasePolicy
{
    public override string PolicyName => "PRODUCT:DELETE";

    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Only admins can delete
        if (HasRole(user, Roles.Admin))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow("Admin access"));
        }
        
        return Task.FromResult(PolicyEvaluationResult.Deny("Not admin"));
    }
}
```

### 2. Strongly-Typed Policy (Recommended)

```csharp
// 1. Define context
public class ProductDeleteContext
{
    public long ProductId { get; set; }
    public bool HasDependencies { get; set; }
}

// 2. Create policy
public class ProductDeletePolicy : BasePolicy<ProductDeleteContext>
{
    public override string PolicyName => "PRODUCT:DELETE";

    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        ProductDeleteContext context)
    {
        // Admins can delete anything
        if (HasRole(user, Roles.Admin))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow());
        }
        
        // Managers can delete only products without dependencies
        if (HasRole(user, Roles.Manager) && !context.HasDependencies)
        {
            return Task.FromResult(PolicyEvaluationResult.Allow());
        }
        
        return Task.FromResult(PolicyEvaluationResult.Deny(
            "Insufficient permissions to delete product"));
    }
}
```

### 3. Register Policy

```csharp
// In Program.cs or Startup.cs
builder.Services.AddPolicyBasedAuthorization(policies =>
{
    policies.AddPolicy<ProductDeletePolicy>(PolicyNames.Pbac.Product.Delete);
});
```

---

## Dynamic Configuration with JWT Claims

Policies can read configuration from JWT claims for **per-user restrictions**.

### Supported Claim Keys

| Claim Key | Type | Description | Example |
|-----------|------|-------------|---------|
| `policy:max_price` or `max_price` | decimal | Maximum price restriction | 5000000 (5M VND) |
| `policy:min_price` or `min_price` | decimal | Minimum price restriction | 100000 |
| `policy:allowed_categories` | string (comma-separated) | Allowed product categories | "electronics,books" |
| `policy:approval_limit` | decimal | Maximum approval amount | 10000000 |
| `department` | string | User's department | "electronics" |

### How It Works

```csharp
// PolicyConfigurationService reads from JWT claims
var config = _policyConfigService.GetEffectivePolicyConfig(user);

// Check against dynamic limit
if (product.Price > config.MaxPrice)
{
    return PolicyEvaluationResult.Deny($"Price exceeds limit");
}
```

### Configuring in Keycloak

1. Go to **Clients** → Your Client → **Mappers**
2. Create new mapper: **User Attribute**
   - Name: `max_price`
   - User Attribute: `max_price`
   - Token Claim Name: `policy:max_price`
   - Claim JSON Type: `String`
3. Set user attribute in **Users** → User → **Attributes**

---

## Best Practices

### ✅ DO

1. **Use RBAC for coarse-grained checks** (role-based)
   ```csharp
   [Authorize(Policy = PolicyNames.Rbac.ManagerOrAdmin)]
   ```

2. **Use PBAC for fine-grained checks** (context-aware)
   ```csharp
   var result = await _policyService.CanViewProductAsync(id, price);
   ```

3. **Use constants instead of magic strings**
   ```csharp
   // Good
   [Authorize(Policy = PolicyNames.Rbac.AdminOnly)]
   
   // Bad
   [Authorize(Policy = "AdminOnly")]
   ```

4. **Provide clear policy evaluation messages**
   ```csharp
   PolicyEvaluationResult.Deny("Product price exceeds user limit of 5M VND");
   ```

5. **Use strongly-typed contexts for complex policies**
   ```csharp
   public class ProductUpdatePolicy : BasePolicy<ProductUpdateContext>
   ```

### ❌ DON'T

1. **Don't mix RBAC and PBAC logic**
   - Keep gateway checks simple (roles only)
   - Keep service checks detailed (business rules)

2. **Don't hardcode business rules**
   - Use dynamic configuration from JWT claims
   - Make policies reusable

3. **Don't skip RBAC when using PBAC**
   - Always have gateway-level protection
   - PBAC should refine, not replace RBAC

---

## Testing

### Unit Testing Policies

```csharp
[Fact]
public async Task ProductViewPolicy_BasicUser_DeniesExpensiveProducts()
{
    // Arrange
    var policy = new ProductViewPolicy(_mockConfigService.Object);
    var user = new UserClaimsContext
    {
        UserId = "user1",
        Roles = new List<string> { Roles.BasicUser }
    };
    var context = new ProductViewContext
    {
        ProductId = 1,
        ProductPrice = 6_000_000M // Above 5M limit
    };
    
    // Act
    var result = await policy.EvaluateAsync(user, context);
    
    // Assert
    Assert.False(result.IsAllowed);
    Assert.Contains("exceeds", result.Reason);
}
```

---

## Troubleshooting

### Issue: "Policy not found"

**Cause**: Policy not registered in DI container

**Solution**: Register policy in `Program.cs`
```csharp
builder.Services.AddPolicyBasedAuthorization(policies =>
{
    policies.AddPolicy<YourPolicy>("YOUR:POLICY:NAME");
});
```

### Issue: "IHttpContextAccessor is null"

**Cause**: Missing registration

**Solution**: Already fixed! `AddPolicyBasedAuthorization()` registers it automatically.

### Issue: Middleware not evaluating policies

**Cause**: Missing `[RequirePolicy]` attribute or wrong middleware order

**Solution**: 
1. Add `[RequirePolicy("POLICY:NAME")]` to controller/action
2. Ensure middleware order:
   ```csharp
   app.UseAuthentication();
   app.UseAuthorization();
   app.UsePolicyAuthorization(); // Must be after
   ```

---

## Migration Guide

### From Old RBAC-Only to Layered Authorization

**Before:**
```csharp
[Authorize(Roles = "admin,manager")]
public async Task<IActionResult> GetProduct(long id)
{
    // Direct access
}
```

**After:**
```csharp
[Authorize(Policy = PolicyNames.Rbac.ManagerOrAdmin)] // Gateway
public async Task<IActionResult> GetProduct(long id)
{
    var product = await _productService.GetAsync(id);
    
    // Fine-grained check
    var check = await _policyService.CanViewProductAsync(id, product.Price);
    if (!check.IsAllowed)
        return Forbid(check.Reason);
        
    return Ok(product);
}
```

---

## Performance Considerations

1. **Policy Evaluator is Singleton** - Cached for performance
2. **Policies are Scoped** - New instance per request
3. **UserContextAccessor is Scoped** - Claims parsed once per request
4. **Configuration Service is Singleton** - Fast claim extraction

---

## References

- **Roles**: `Shared/Identity/Roles.cs`
- **Permissions**: `Shared/Identity/Permissions.cs`
- **Policy Names**: `Shared/Identity/PolicyNames.cs`
- **Policy Constants**: `Shared/Identity/PolicyConstants.cs`
- **Extension Methods**: `Infrastructure/Extentions/PolicyAuthorizationExtensions.cs`
- **Example Controller**: `Base.API/Controllers/ProductControllerWithPBAC.cs`

---

## Support

For questions or issues, refer to:
- Policy implementations in `Base.Application/Feature/Product/Policies/`
- Service implementations in `Base.Application/Feature/Product/Services/`

