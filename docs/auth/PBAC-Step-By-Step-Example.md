# PBAC Step-by-Step: Tracing Code Thực Tế

## Scenario: User Request Xem Danh Sách Product

Chúng ta sẽ trace từng bước code chạy từ lúc application start đến lúc policy được evaluate.

---

## Phase 1: Application Startup

### Step 1: Program.cs - Developer Config

```csharp
// File: Base.API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Line 45: Developer đăng ký PBAC
services.AddPolicyBasedAuthorization(registry =>
{
    // Developer nói: "Policy tên PRODUCT:LIST_FILTER được implement bởi ProductListFilterPolicy"
    registry.AddPolicy<ProductListFilterPolicy>("PRODUCT:LIST_FILTER");
    registry.AddPolicy<ProductViewPolicy>("PRODUCT:VIEW");
    registry.AddPolicy<ProductCreatePolicy>("PRODUCT:CREATE");
});
```

**💡 Hiểu như thế nào:**
- Developer đang "dạy" hệ thống: *"Khi ai đó gọi policy tên `PRODUCT:LIST_FILTER`, hãy chạy class `ProductListFilterPolicy`"*

---

### Step 2: PolicyRegistry.AddPolicy() - Thu Thập Info

```csharp
// File: Infrastructure/Extensions/PolicyAuthorizationExtensions.cs
public class PolicyRegistry
{
    private readonly IServiceCollection _services;
    private readonly List<(string policyName, Type policyType)> _policies = new();
    
    public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
    {
        // A. Đăng ký class vào DI Container
        _services.AddScoped<TPolicy>();
        // ↑ Giờ DI Container biết cách tạo ProductListFilterPolicy
        
        // B. Lưu lại mapping để dùng sau
        _policies.Add((policyName, typeof(TPolicy)));
        // ↑ _policies = [
        //     ("PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy)),
        //     ...
        // ]
        
        return this;
    }
}
```

**State sau khi chạy xong:**

```
DI Container:
├─ Scoped: ProductListFilterPolicy ✓
├─ Scoped: ProductViewPolicy ✓
└─ Scoped: ProductCreatePolicy ✓

PolicyRegistry._policies:
├─ ("PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy))
├─ ("PRODUCT:VIEW", typeof(ProductViewPolicy))
└─ ("PRODUCT:CREATE", typeof(ProductCreatePolicy))
```

---

### Step 3: PolicyEvaluator Creation - Build Registry

```csharp
// File: Infrastructure/Extensions/PolicyAuthorizationExtensions.cs
public static IServiceCollection AddPolicyBasedAuthorization(...)
{
    // ... registry.AddPolicy() đã chạy xong ...
    
    // Giờ tạo PolicyEvaluator và build internal registry
    services.AddSingleton<PolicyEvaluator>(sp =>
    {
        // 1. Tạo evaluator với ServiceProvider
        var evaluator = new PolicyEvaluator(sp);
        
        // 2. Lấy tất cả policies đã collect
        foreach (var (policyName, policyType) in policyRegistry.GetRegisteredPolicies())
        {
            // 3. Đăng ký vào internal dictionary của evaluator
            evaluator.RegisterPolicy(policyType, policyName);
        }
        
        return evaluator;
    });
    
    // Register interface
    services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());
}
```

**Trace cụ thể cho ProductListFilterPolicy:**

```csharp
// Loop iteration 1:
var policyName = "PRODUCT:LIST_FILTER";
var policyType = typeof(ProductListFilterPolicy);

evaluator.RegisterPolicy(policyType, policyName);
// ↓ Đi vào PolicyEvaluator.RegisterPolicy()
```

---

### Step 4: PolicyEvaluator.RegisterPolicy() - Store Mapping

```csharp
// File: Infrastructure/Authorization/PolicyEvaluator.cs
public class PolicyEvaluator : IPolicyEvaluator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _policyRegistry;
    
    public PolicyEvaluator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _policyRegistry = new Dictionary<string, Type>();
    }
    
    public void RegisterPolicy(Type policyType, string policyName)
    {
        // Validate
        if (!typeof(IPolicy).IsAssignableFrom(policyType))
            throw new ArgumentException(...);
        
        // QUAN TRỌNG: Lưu vào dictionary
        _policyRegistry[policyName] = policyType;
        // ↓ Cụ thể:
        // _policyRegistry["PRODUCT:LIST_FILTER"] = typeof(ProductListFilterPolicy)
    }
}
```

**State cuối cùng sau startup:**

```
PolicyEvaluator (Singleton Instance):
├─ _serviceProvider: IServiceProvider (reference đến DI container)
└─ _policyRegistry: Dictionary<string, Type>
    {
        ["PRODUCT:LIST_FILTER"] = typeof(ProductListFilterPolicy),
        ["PRODUCT:VIEW"] = typeof(ProductViewPolicy),
        ["PRODUCT:CREATE"] = typeof(ProductCreatePolicy)
    }
```

**💡 Key Point:** 
- Dictionary này giờ đã sẵn sàng!
- Mỗi khi có request, evaluator sẽ lookup trong dictionary này

---

## Phase 2: HTTP Request Processing

### Request: `GET /api/v2/product`

---

### Step 5: Controller Action Called

```csharp
// File: Base.API/Controllers/ProductControllerWithPBAC.cs
[HttpGet]
[Authorize(Policy = "BasicUser")]  // ← RBAC pass ✓
public async Task<IActionResult> GetProducts([FromQuery] PagedRequestParameter parameters)
{
    _logger.LogInformation("Getting products...");
    
    // Controller gọi policy service
    var filter = await _productPolicyService.GetProductListFilterAsync();
    //                  ↑
    //                  Đi vào ProductPolicyService
}
```

---

### Step 6: ProductPolicyService Method

```csharp
// File: Base.Application/Feature/Product/Services/ProductPolicyService.cs
public class ProductPolicyService : IProductPolicyService
{
    private readonly IPolicyEvaluator _policyEvaluator;  // ← Singleton đã được inject
    private readonly IUserContextAccessor _userContextAccessor;
    
    public async Task<ProductListFilterContext> GetProductListFilterAsync()
    {
        // 1. Lấy user context từ current request
        var userContext = _userContextAccessor.GetCurrentUserContext();
        // userContext = {
        //     UserId = "user123",
        //     Roles = ["Basic"],
        //     Claims = { ... }
        // }
        
        // 2. Gọi PolicyEvaluator với policy NAME
        var policyResult = await _policyEvaluator.EvaluateAsync(
            Policies.ProductListFilterPolicy.POLICY_NAME,  // ← "PRODUCT:LIST_FILTER"
            userContext,
            new Dictionary<string, object>());
        //  ↑
        //  Đi vào PolicyEvaluator.EvaluateAsync()
        
        // ... process result
    }
}
```

**💡 Quan trọng:**
- Service chỉ biết policy NAME (`"PRODUCT:LIST_FILTER"`)
- Service KHÔNG biết implementation class nào sẽ chạy
- PolicyEvaluator sẽ lo việc đó!

---

### Step 7: PolicyEvaluator.EvaluateAsync() - The Magic Happens Here!

```csharp
// File: Infrastructure/Authorization/PolicyEvaluator.cs
public async Task<PolicyEvaluationResult> EvaluateAsync(
    string policyName,              // = "PRODUCT:LIST_FILTER"
    UserClaimsContext user,         // = { UserId: "user123", Roles: ["Basic"] }
    Dictionary<string, object> context)
{
    // === BƯỚC 1: LOOKUP TRONG DICTIONARY ===
    Console.WriteLine($"Looking up policy: {policyName}");
    
    if (!_policyRegistry.TryGetValue(policyName, out var policyType))
    {
        // Nếu không tìm thấy
        return PolicyEvaluationResult.Deny($"Policy '{policyName}' not found");
    }
    
    // TÌM THẤY!
    Console.WriteLine($"Found policy type: {policyType.Name}");
    // Output: Found policy type: ProductListFilterPolicy
    
    // policyType = typeof(ProductListFilterPolicy)
    
    
    // === BƯỚC 2: LẤY INSTANCE TỪ DI CONTAINER ===
    Console.WriteLine("Getting policy instance from DI...");
    
    var policy = _serviceProvider.GetService(policyType) as IPolicy;
    // ↑ Tương đương với:
    // var policy = _serviceProvider.GetService(typeof(ProductListFilterPolicy)) as IPolicy;
    // ↑ DI Container tạo instance:
    // var policy = new ProductListFilterPolicy(dependencies...);
    
    if (policy == null)
    {
        return PolicyEvaluationResult.Deny($"Policy '{policyName}' could not be instantiated");
    }
    
    Console.WriteLine($"Policy instance created: {policy.GetType().Name}");
    // Output: Policy instance created: ProductListFilterPolicy
    
    
    // === BƯỚC 3: EXECUTE POLICY ===
    Console.WriteLine("Executing policy...");
    
    return await policy.EvaluateAsync(user, context);
    // ↑ Gọi method của ProductListFilterPolicy
}
```

**Visual Trace:**

```
Input: "PRODUCT:LIST_FILTER"
       ↓
_policyRegistry.TryGetValue("PRODUCT:LIST_FILTER", out policyType)
       ↓
policyType = typeof(ProductListFilterPolicy)
       ↓
_serviceProvider.GetService(typeof(ProductListFilterPolicy))
       ↓
DI Container creates instance:
   new ProductListFilterPolicy(
       policyConfigService,    // ← Injected
       logger                  // ← Injected
   )
       ↓
policy = ProductListFilterPolicy instance
       ↓
policy.EvaluateAsync(user, context)
       ↓
ProductListFilterPolicy.EvaluateAsync() executes
```

---

### Step 8: Policy Execution

```csharp
// File: Infrastructure/Authorization/Policies/ProductListFilterPolicy.cs
public class ProductListFilterPolicy : IPolicy
{
    private readonly IPolicyConfigurationService _configService;
    
    public string PolicyName => "PRODUCT:LIST_FILTER";
    
    public async Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        Console.WriteLine($"Evaluating for user: {user.UserId}, roles: {string.Join(",", user.Roles)}");
        
        // Business logic
        if (user.HasRole("Admin") || user.HasRole("Premium"))
        {
            var metadata = new Dictionary<string, object>
            {
                { "MaxPrice", decimal.MaxValue }
            };
            return PolicyEvaluationResult.Allow("Full access", metadata);
        }
        
        if (user.HasRole("Basic"))
        {
            var maxPrice = await _configService.GetUserMaxProductPriceAsync(user);
            var metadata = new Dictionary<string, object>
            {
                { "MaxPrice", maxPrice }
            };
            return PolicyEvaluationResult.Allow($"Limited to {maxPrice:N0}", metadata);
        }
        
        return PolicyEvaluationResult.Deny("No valid role");
    }
}
```

**Output for Basic user:**
```
Evaluating for user: user123, roles: Basic
→ Result: Allow (Limited to 5,000,000)
→ Metadata: { MaxPrice: 5000000 }
```

---

### Step 9: Result Flows Back

```
ProductListFilterPolicy.EvaluateAsync()
   ↓ returns PolicyEvaluationResult
PolicyEvaluator.EvaluateAsync()
   ↓ returns same result
ProductPolicyService.GetProductListFilterAsync()
   ↓ extracts metadata, returns ProductListFilterContext
Controller.GetProducts()
   ↓ uses filter to query products
Response: 200 OK with filtered products
```

---

## Complete Flow Diagram với Code Line Numbers

```
Startup Phase:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Program.cs:45
  registry.AddPolicy<ProductListFilterPolicy>("PRODUCT:LIST_FILTER")
      ↓
PolicyAuthorizationExtensions.cs:84
  _services.AddScoped<ProductListFilterPolicy>();
  _policies.Add(("PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy)));
      ↓
PolicyAuthorizationExtensions.cs:32-41
  var evaluator = new PolicyEvaluator(sp);
  evaluator.RegisterPolicy(typeof(ProductListFilterPolicy), "PRODUCT:LIST_FILTER");
      ↓
PolicyEvaluator.cs:23-30
  _policyRegistry["PRODUCT:LIST_FILTER"] = typeof(ProductListFilterPolicy);

✓ Registry built: "PRODUCT:LIST_FILTER" → typeof(ProductListFilterPolicy)


Runtime Phase:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
GET /api/v2/product
      ↓
ProductControllerWithPBAC.cs:43
  var filter = await _productPolicyService.GetProductListFilterAsync();
      ↓
ProductPolicyService.cs:34-37
  await _policyEvaluator.EvaluateAsync("PRODUCT:LIST_FILTER", userContext, context)
      ↓
PolicyEvaluator.cs:43-60
  Line 48: if (!_policyRegistry.TryGetValue("PRODUCT:LIST_FILTER", out var policyType))
           → Found! policyType = typeof(ProductListFilterPolicy)
  
  Line 53: var policy = _serviceProvider.GetService(policyType) as IPolicy;
           → DI creates: new ProductListFilterPolicy(...)
  
  Line 59: return await policy.EvaluateAsync(user, context);
           → Calls ProductListFilterPolicy.EvaluateAsync()
      ↓
ProductListFilterPolicy.cs:EvaluateAsync()
  Execute business rules
  Return Allow/Deny with metadata
      ↓
Back to Controller with result
```

---

## Tóm Tắt: Câu Trả Lời Rõ Ràng

### Câu hỏi: `_policyEvaluator.EvaluateAsync("PRODUCT:LIST_FILTER", ...)` làm sao biết chạy policy nào?

**Trả lời:**

1. **Lúc startup**, developer đăng ký:
   ```csharp
   registry.AddPolicy<ProductListFilterPolicy>("PRODUCT:LIST_FILTER");
   ```
   → PolicyEvaluator build dictionary: `{"PRODUCT:LIST_FILTER": typeof(ProductListFilterPolicy)}`

2. **Lúc runtime**, khi gọi `EvaluateAsync("PRODUCT:LIST_FILTER", ...)`:
   - Lookup trong dictionary: `"PRODUCT:LIST_FILTER"` → `typeof(ProductListFilterPolicy)`
   - Get instance từ DI: `new ProductListFilterPolicy(...)`
   - Execute: `policy.EvaluateAsync(user, context)`

3. **Kết nối** giữa policy name và implementation:
   ```
   "PRODUCT:LIST_FILTER" ──┬── (Stored in _policyRegistry dictionary)
                           │
                           └──→ typeof(ProductListFilterPolicy)
                                      │
                                      └──→ (DI Container creates instance)
                                            │
                                            └──→ ProductListFilterPolicy instance
   ```

**Đơn giản**: Giống như một phonebook!
- **Startup**: Bạn lưu số điện thoại: `{"John": "123-456"}` 
- **Runtime**: Bạn tra tên: `"John"` → Tìm thấy: `"123-456"` → Gọi số

Ở đây:
- **Startup**: Lưu mapping: `{"PRODUCT:LIST_FILTER": typeof(ProductListFilterPolicy)}`
- **Runtime**: Tra tên: `"PRODUCT:LIST_FILTER"` → Tìm thấy: `typeof(...)` → Tạo instance

---

## Debug Commands

Nếu muốn xem registry trong runtime, thêm endpoint debug:

```csharp
[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly PolicyEvaluator _evaluator;
    
    [HttpGet("policies")]
    public IActionResult GetPolicies()
    {
        var registry = _evaluator.GetType()
            .GetField("_policyRegistry", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_evaluator) as Dictionary<string, Type>;
        
        return Ok(registry.Select(kv => new 
        { 
            PolicyName = kv.Key, 
            ImplementationType = kv.Value.Name 
        }));
    }
}

// Output:
// [
//   { "policyName": "PRODUCT:LIST_FILTER", "implementationType": "ProductListFilterPolicy" },
//   { "policyName": "PRODUCT:VIEW", "implementationType": "ProductViewPolicy" },
//   ...
// ]
```

---

