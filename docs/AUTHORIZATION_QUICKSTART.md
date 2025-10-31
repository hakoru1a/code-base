# Authorization Quick Start Guide

## 🚀 Tổng quan

Hệ thống authorization của chúng ta sử dụng **2 lớp bảo mật**:

1. **RBAC** (ở Gateway) - Kiểm tra nhanh dựa trên role
2. **PBAC** (ở Service) - Kiểm tra chi tiết dựa trên business rules

## 📋 Setup

### 1. Cấu hình trong `Program.cs`

```csharp
// 1. Thêm Authentication với Keycloak
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddKeycloakAuthorization();

// 2. Thêm Policy-Based Authorization
builder.Services.AddPolicyBasedAuthorization(policies =>
{
    policies.AddPolicy<ProductViewPolicy>(PolicyNames.Pbac.Product.View);
    policies.AddPolicy<ProductCreatePolicy>(PolicyNames.Pbac.Product.Create);
    policies.AddPolicy<ProductUpdatePolicy>(PolicyNames.Pbac.Product.Update);
    policies.AddPolicy<ProductListFilterPolicy>(PolicyNames.Pbac.Product.ListFilter);
});

// 3. Thêm vào middleware pipeline (SAU UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();
app.UsePolicyAuthorization(); // ← Phải sau UseAuthorization
```

### 2. Inject Service vào Controller

```csharp
public class ProductController : ControllerBase
{
    private readonly IProductPolicyService _policyService;
    
    public ProductController(IProductPolicyService policyService)
    {
        _policyService = policyService;
    }
}
```

---

## 💡 Các Patterns Sử Dụng

### Pattern 1: RBAC Only (Đơn giản)

**Khi nào dùng**: Endpoint chỉ cần kiểm tra role, không có business rules phức tạp

```csharp
[HttpDelete("{id}")]
[Authorize(Policy = PolicyNames.Rbac.AdminOnly)]
public async Task<IActionResult> DeleteProduct(long id)
{
    // Chỉ admin mới vào được đây
    await _productService.DeleteAsync(id);
    return Ok();
}
```

### Pattern 2: RBAC + PBAC (Phổ biến nhất)

**Khi nào dùng**: Cần kiểm tra role + business rules (giá, category, ownership, etc.)

```csharp
[HttpGet("{id}")]
[Authorize(Policy = PolicyNames.Rbac.BasicUser)] // Bước 1: RBAC
public async Task<IActionResult> GetProduct(long id)
{
    var product = await _productService.GetAsync(id);
    
    // Bước 2: PBAC - kiểm tra chi tiết
    var check = await _policyService.CanViewProductAsync(id, product.Price);
    
    if (!check.IsAllowed)
    {
        return StatusCode(403, new { error = check.Reason });
    }
    
    return Ok(product);
}
```

### Pattern 3: PBAC with Filter (Cho List)

**Khi nào dùng**: List endpoint cần filter data dựa trên user permissions

```csharp
[HttpGet]
[Authorize(Policy = PolicyNames.Rbac.BasicUser)]
public async Task<IActionResult> GetProducts([FromQuery] PagedRequestParameter parameters)
{
    // Lấy filter criteria dựa trên user
    var filter = await _policyService.GetProductListFilterAsync();
    
    // Apply filter vào query
    var query = new GetProductsQuery
    {
        Parameters = parameters,
        MaxPrice = filter.MaxPrice, // Basic user có limit giá
        AllowedCategories = filter.AllowedCategories // Manager chỉ thấy category của mình
    };
    
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

### Pattern 4: Attribute-based PBAC (Với Middleware)

**Khi nào dùng**: Muốn middleware tự động kiểm tra policy trước khi vào action

```csharp
[HttpGet]
[RequirePolicy(PolicyNames.Pbac.Product.View)]
public async Task<IActionResult> GetProducts()
{
    // Middleware đã kiểm tra policy rồi
    // Nếu vào được đây nghĩa là đã pass
    var products = await _productService.GetAllAsync();
    return Ok(products);
}
```

---

## 🔐 Roles và Permissions

### Roles (RBAC)

Sử dụng constants từ `Shared.Identity.Roles`:

```csharp
Roles.Admin              // "admin"
Roles.Manager            // "manager"
Roles.ProductManager     // "product_manager"
Roles.PremiumUser        // "premium_user"
Roles.BasicUser          // "basic_user"
```

### RBAC Policies

Sử dụng từ `Shared.Identity.PolicyNames.Rbac`:

```csharp
PolicyNames.Rbac.AdminOnly        // Chỉ admin
PolicyNames.Rbac.ManagerOrAdmin   // Admin hoặc manager
PolicyNames.Rbac.PremiumUser      // Premium user hoặc admin
PolicyNames.Rbac.BasicUser        // Bất kỳ user nào có account
PolicyNames.Rbac.AuthenticatedUser // Bất kỳ user đã login
```

### PBAC Policies

Sử dụng từ `Shared.Identity.PolicyNames.Pbac`:

```csharp
PolicyNames.Pbac.Product.View         // "PRODUCT:VIEW"
PolicyNames.Pbac.Product.Create       // "PRODUCT:CREATE"
PolicyNames.Pbac.Product.Update       // "PRODUCT:UPDATE"
PolicyNames.Pbac.Product.Delete       // "PRODUCT:DELETE"
PolicyNames.Pbac.Product.ListFilter   // "PRODUCT:LIST_FILTER"
```

### Permissions (Fine-grained)

Sử dụng từ `Shared.Identity.Permissions`:

```csharp
Permissions.Product.Create      // "product:create"
Permissions.Product.View        // "product:view"
Permissions.Product.Update      // "product:update"
Permissions.Product.UpdateOwn   // "product:update:own"
Permissions.Product.Delete      // "product:delete"
```

---

## 🎯 Tạo Policy Mới

### Bước 1: Tạo Context (nếu cần)

```csharp
// Shared/DTOs/Authorization/PolicyContexts/OrderApproveContext.cs
public class OrderApproveContext
{
    public long OrderId { get; set; }
    public decimal OrderTotal { get; set; }
    public string OrderStatus { get; set; }
}
```

### Bước 2: Tạo Policy

```csharp
// Application/Feature/Order/Policies/OrderApprovePolicy.cs
public class OrderApprovePolicy : BasePolicy<OrderApproveContext>
{
    private readonly IPolicyConfigurationService _configService;
    
    public const string POLICY_NAME = "ORDER:APPROVE";
    public override string PolicyName => POLICY_NAME;
    
    public OrderApprovePolicy(IPolicyConfigurationService configService)
    {
        _configService = configService;
    }
    
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        OrderApproveContext context)
    {
        // Admin approve mọi order
        if (HasRole(user, Roles.Admin))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                PolicyConstants.Messages.AdminFullAccess));
        }
        
        // Manager có approval limit
        if (HasRole(user, Roles.Manager))
        {
            var config = _configService.GetEffectivePolicyConfig(user);
            
            if (config.ApprovalLimit.HasValue && 
                context.OrderTotal <= config.ApprovalLimit.Value)
            {
                return Task.FromResult(PolicyEvaluationResult.Allow(
                    $"Order within approval limit"));
            }
            
            return Task.FromResult(PolicyEvaluationResult.Deny(
                $"Order total {context.OrderTotal:N0} exceeds approval limit"));
        }
        
        return Task.FromResult(PolicyEvaluationResult.Deny(
            "User does not have permission to approve orders"));
    }
}
```

### Bước 3: Register Policy

```csharp
// Program.cs
builder.Services.AddPolicyBasedAuthorization(policies =>
{
    policies.AddPolicy<OrderApprovePolicy>(PolicyNames.Pbac.Order.Approve);
});
```

### Bước 4: Tạo Service Method (Optional)

```csharp
// Application/Feature/Order/Services/IOrderPolicyService.cs
public interface IOrderPolicyService
{
    Task<PolicyEvaluationResult> CanApproveOrderAsync(long orderId, decimal total);
}

// Implementation
public class OrderPolicyService : IOrderPolicyService
{
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IUserContextAccessor _userContextAccessor;
    
    public async Task<PolicyEvaluationResult> CanApproveOrderAsync(
        long orderId, 
        decimal total)
    {
        var userContext = _userContextAccessor.GetCurrentUserContext();
        var policyContext = new Dictionary<string, object>
        {
            { PolicyConstants.ContextKeys.OrderId, orderId },
            { PolicyConstants.ContextKeys.OrderTotal, total }
        };
        
        return await _policyEvaluator.EvaluateAsync(
            PolicyNames.Pbac.Order.Approve,
            userContext,
            policyContext);
    }
}
```

### Bước 5: Sử dụng trong Controller

```csharp
[HttpPost("{id}/approve")]
[Authorize(Policy = PolicyNames.Rbac.ManagerOrAdmin)]
public async Task<IActionResult> ApproveOrder(long id)
{
    var order = await _orderService.GetAsync(id);
    
    var check = await _orderPolicyService.CanApproveOrderAsync(id, order.Total);
    
    if (!check.IsAllowed)
    {
        return StatusCode(403, new { error = check.Reason });
    }
    
    await _orderService.ApproveAsync(id);
    return Ok();
}
```

---

## 🔧 Dynamic Configuration với JWT Claims

### Cấu hình trong Keycloak

1. **Client Mappers**:
   - Token Claim Name: `policy:max_price`
   - User Attribute: `max_price`
   - Claim JSON Type: `String`

2. **User Attributes**:
   ```
   max_price = 5000000
   allowed_categories = electronics,books
   approval_limit = 10000000
   department = electronics
   ```

### Supported Claims

| Claim | Type | Description |
|-------|------|-------------|
| `policy:max_price` | decimal | Giới hạn giá tối đa |
| `policy:min_price` | decimal | Giới hạn giá tối thiểu |
| `policy:allowed_categories` | string (comma-separated) | Categories được phép |
| `policy:approval_limit` | decimal | Hạn mức duyệt |
| `department` | string | Phòng ban |

### Sử dụng trong Policy

```csharp
var config = _configService.GetEffectivePolicyConfig(user);

// Tự động đọc từ JWT claims
if (config.MaxPrice.HasValue && product.Price > config.MaxPrice.Value)
{
    return PolicyEvaluationResult.Deny($"Price exceeds limit");
}
```

---

## 📊 So sánh RBAC vs PBAC

| Aspect | RBAC | PBAC |
|--------|------|------|
| **Level** | Gateway/Controller | Service |
| **Granularity** | Coarse (role-based) | Fine (context-aware) |
| **Performance** | Fast | Slightly slower |
| **Flexibility** | Low | High |
| **Dynamic** | Static roles | Dynamic with JWT claims |
| **Use Case** | "Who can access?" | "Can this user do this specific action?" |
| **Example** | Admin can access endpoint | User can view product under $1000 |

---

## ✅ Best Practices

### 1. Luôn dùng RBAC trước
```csharp
// ✅ Good
[Authorize(Policy = PolicyNames.Rbac.BasicUser)] // RBAC first
public async Task<IActionResult> GetProduct(long id)
{
    // PBAC check here
}

// ❌ Bad - Không có RBAC gateway protection
public async Task<IActionResult> GetProduct(long id)
{
    // Only PBAC - anyone can hit this endpoint
}
```

### 2. Sử dụng constants
```csharp
// ✅ Good
[Authorize(Policy = PolicyNames.Rbac.AdminOnly)]

// ❌ Bad
[Authorize(Policy = "AdminOnly")]
```

### 3. Provide clear error messages
```csharp
// ✅ Good
PolicyEvaluationResult.Deny("Product price 6,000,000 VND exceeds user limit of 5,000,000 VND");

// ❌ Bad
PolicyEvaluationResult.Deny("Access denied");
```

### 4. Tách policy logic ra khỏi controller
```csharp
// ✅ Good - Use service
var check = await _policyService.CanViewProductAsync(id, price);

// ❌ Bad - Policy logic in controller
if (User.IsInRole("basic_user") && product.Price > 5000000) { ... }
```

---

## 🐛 Troubleshooting

### Issue: "Policy not found"
```
Fix: Register policy trong Program.cs
policies.AddPolicy<YourPolicy>(PolicyNames.Pbac.Your.Policy);
```

### Issue: Middleware không chạy
```
Fix: Kiểm tra thứ tự middleware
app.UseAuthentication();
app.UseAuthorization();
app.UsePolicyAuthorization(); // Must be AFTER UseAuthorization
```

### Issue: JWT claims không đọc được
```
Fix: Kiểm tra Keycloak mapper configuration
- Token Claim Name: "policy:max_price"
- User Attribute: "max_price"
```

---

## 📚 Examples

Xem thêm examples tại:
- `Base.API/Controllers/ProductControllerWithPBAC.cs`
- `Base.Application/Feature/Product/Policies/`
- `Base.Application/Feature/Product/Services/ProductPolicyService.cs`

## 📖 Full Documentation

Xem documentation đầy đủ tại: `Infrastructure/Authorization/README.md`

