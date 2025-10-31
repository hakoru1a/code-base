# Giải Thích Chi Tiết: PolicyEvaluator Biết Policy Nào Để Chạy?

## Câu Hỏi

Khi gọi:
```csharp
var policyResult = await _policyEvaluator.EvaluateAsync(
    Policies.ProductListFilterPolicy.POLICY_NAME,  // ← "PRODUCT:LIST_FILTER"
    userContext,
    new Dictionary<string, object>());
```

**PolicyEvaluator làm sao biết policy nào để chạy?**

## Câu Trả Lời Ngắn Gọn

PolicyEvaluator có một **Dictionary bên trong** map từ **policy name** (string) → **policy type** (Type):

```csharp
Dictionary<string, Type> _policyRegistry = new()
{
    { "PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy) },
    { "PRODUCT:VIEW", typeof(ProductViewPolicy) },
    { "PRODUCT:CREATE", typeof(ProductCreatePolicy) }
};
```

Khi bạn gọi `EvaluateAsync("PRODUCT:LIST_FILTER", ...)`, evaluator:
1. Tìm type trong dictionary: `"PRODUCT:LIST_FILTER"` → `ProductListFilterPolicy`
2. Lấy instance từ DI container: `_serviceProvider.GetService(typeof(ProductListFilterPolicy))`
3. Cast sang IPolicy và gọi `EvaluateAsync()`

## Giải Thích Chi Tiết: The Complete Flow

### Phase 1: APPLICATION STARTUP (Program.cs)

#### Bước 1: Developer Đăng Ký Policies

```csharp
// Program.cs
services.AddPolicyBasedAuthorization(registry =>
{
    // Developer đăng ký: "Tên policy" + Implementation class
    registry.AddPolicy<ProductListFilterPolicy>("PRODUCT:LIST_FILTER");
    registry.AddPolicy<ProductViewPolicy>("PRODUCT:VIEW");
    registry.AddPolicy<ProductCreatePolicy>("PRODUCT:CREATE");
});
```

**Điều gì xảy ra ở đây?**

```csharp
// PolicyAuthorizationExtensions.cs
public static IServiceCollection AddPolicyBasedAuthorization(...)
{
    // 1. Tạo PolicyRegistry
    var policyRegistry = new PolicyRegistry(services);
    
    // 2. Developer gọi registry.AddPolicy<T>("NAME")
    configurePolicies?.Invoke(policyRegistry);  // ← Callback được gọi ở đây
    
    // 3. Registry giờ đã chứa list: (name, type)
    // policyRegistry._policies = [
    //     ("PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy)),
    //     ("PRODUCT:VIEW", typeof(ProductViewPolicy)),
    //     ("PRODUCT:CREATE", typeof(ProductCreatePolicy))
    // ]
    
    // Tiếp theo...
}
```

#### Bước 2: PolicyRegistry Thu Thập Thông Tin

```csharp
// PolicyRegistry.AddPolicy<TPolicy>(string policyName)
public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
{
    // A. Đăng ký policy class vào DI container (Scoped lifetime)
    _services.AddScoped<TPolicy>();
    //      ↓
    //   services.AddScoped<ProductListFilterPolicy>();
    
    // B. Lưu mapping (name → type) để sau này dùng
    _policies.Add((policyName, typeof(TPolicy)));
    //      ↓
    //   _policies.Add(("PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy)));
    
    return this;
}
```

**Sau bước này:**
- ✅ DI Container biết cách tạo `ProductListFilterPolicy`
- ✅ PolicyRegistry có list mapping: `("PRODUCT:LIST_FILTER", typeof(ProductListFilterPolicy))`

#### Bước 3: PolicyEvaluator Được Tạo và Đăng Ký

```csharp
// PolicyAuthorizationExtensions.cs (tiếp)
services.AddSingleton<PolicyEvaluator>(sp =>
{
    // 1. Tạo PolicyEvaluator instance
    var evaluator = new PolicyEvaluator(sp);
    //                                  ↑
    //                    ServiceProvider được inject vào
    
    // 2. Lấy tất cả policies đã track từ registry
    foreach (var (policyName, policyType) in policyRegistry.GetRegisteredPolicies())
    {
        // 3. Đăng ký vào internal dictionary của evaluator
        evaluator.RegisterPolicy(policyType, policyName);
        //        ↓
        //   evaluator.RegisterPolicy(typeof(ProductListFilterPolicy), "PRODUCT:LIST_FILTER");
    }
    
    return evaluator;
});
```

**Điều gì xảy ra trong `evaluator.RegisterPolicy()`?**

```csharp
// PolicyEvaluator.cs
private readonly Dictionary<string, Type> _policyRegistry;

public void RegisterPolicy(Type policyType, string policyName)
{
    // Validate
    if (!typeof(IPolicy).IsAssignableFrom(policyType))
        throw new ArgumentException($"Type {policyType.Name} does not implement IPolicy");
    
    // QUAN TRỌNG: Lưu mapping vào dictionary
    _policyRegistry[policyName] = policyType;
    //      ↓
    //   _policyRegistry["PRODUCT:LIST_FILTER"] = typeof(ProductListFilterPolicy);
}
```

**Kết quả sau Application Startup:**

```
PolicyEvaluator (Singleton) instance chứa:
├─ _serviceProvider: IServiceProvider (để lấy instances từ DI)
└─ _policyRegistry: Dictionary<string, Type>
    ├─ ["PRODUCT:LIST_FILTER"] = typeof(ProductListFilterPolicy)
    ├─ ["PRODUCT:VIEW"] = typeof(ProductViewPolicy)
    └─ ["PRODUCT:CREATE"] = typeof(ProductCreatePolicy)

DI Container chứa:
├─ Scoped: ProductListFilterPolicy
├─ Scoped: ProductViewPolicy
├─ Scoped: ProductCreatePolicy
└─ Singleton: PolicyEvaluator (với _policyRegistry đã được fill)
```

---

### Phase 2: RUNTIME (HTTP Request Processing)

#### Request Flow

```
1. HTTP Request đến
   ↓
2. ProductPolicyService được inject (Scoped)
   - Constructor nhận IPolicyEvaluator (Singleton instance)
   ↓
3. ProductPolicyService.GetProductListFilterAsync() được gọi
```

#### Bước 4: Service Gọi Evaluator

```csharp
// ProductPolicyService.cs (Line 34-38)
var policyResult = await _policyEvaluator.EvaluateAsync(
    Policies.ProductListFilterPolicy.POLICY_NAME,  // = "PRODUCT:LIST_FILTER"
    userContext,
    new Dictionary<string, object>());
```

**Điều gì xảy ra tiếp theo?**

```csharp
// PolicyEvaluator.cs
public async Task<PolicyEvaluationResult> EvaluateAsync(
    string policyName,              // ← "PRODUCT:LIST_FILTER"
    UserClaimsContext user,
    Dictionary<string, object> context)
{
    // BƯỚC 1: Tìm policy type trong dictionary
    if (!_policyRegistry.TryGetValue(policyName, out var policyType))
    {
        //     _policyRegistry["PRODUCT:LIST_FILTER"]
        //               ↓
        //     policyType = typeof(ProductListFilterPolicy)
        
        return PolicyEvaluationResult.Deny($"Policy '{policyName}' not found");
    }
    
    // BƯỚC 2: Lấy instance từ DI container
    var policy = _serviceProvider.GetService(policyType) as IPolicy;
    //           _serviceProvider.GetService(typeof(ProductListFilterPolicy))
    //                      ↓
    //           DI container tạo instance mới của ProductListFilterPolicy
    //           (bởi vì nó là Scoped, tạo mới cho mỗi request)
    //                      ↓
    //           policy = new ProductListFilterPolicy(dependencies...)
    
    if (policy == null)
    {
        return PolicyEvaluationResult.Deny($"Policy '{policyName}' could not be instantiated");
    }
    
    // BƯỚC 3: Thực thi policy
    return await policy.EvaluateAsync(user, context);
    //           ↓
    //    ProductListFilterPolicy.EvaluateAsync(user, context)
}
```

#### Bước 5: Policy Execution

```csharp
// ProductListFilterPolicy.cs
public class ProductListFilterPolicy : IPolicy
{
    public string PolicyName => "PRODUCT:LIST_FILTER";
    
    public async Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Business logic here
        if (user.HasRole("Premium"))
        {
            return PolicyEvaluationResult.Allow("Premium user");
        }
        
        // ...more rules
    }
}
```

---

## Visualization: The Complete Picture

### Registration Time (Startup)

```
Developer Code                    PolicyRegistry              PolicyEvaluator
     │                                  │                          │
     │ registry.AddPolicy<              │                          │
     │   ProductListFilterPolicy>       │                          │
     │   ("PRODUCT:LIST_FILTER")        │                          │
     ├─────────────────────────────────>│                          │
     │                                  │ Store:                   │
     │                                  │ ("PRODUCT:LIST_FILTER",  │
     │                                  │  typeof(ProductList...)) │
     │                                  │                          │
     │                                  │ policyRegistry           │
     │                                  │ .GetRegisteredPolicies() │
     │                                  ├─────────────────────────>│
     │                                  │   List of (name, type)   │
     │                                  │                          │
     │                                  │                          │ foreach (name, type):
     │                                  │                          │   _policyRegistry[name] = type
     │                                  │                          │
     │                                  │                          │ Result:
     │                                  │                          │ _policyRegistry = {
     │                                  │                          │   "PRODUCT:LIST_FILTER": typeof(ProductListFilterPolicy),
     │                                  │                          │   "PRODUCT:VIEW": typeof(ProductViewPolicy),
     │                                  │                          │   ...
     │                                  │                          │ }
```

### Runtime (Request Processing)

```
ProductPolicyService              PolicyEvaluator                DI Container              Policy Instance
       │                                 │                              │                          │
       │ EvaluateAsync(                  │                              │                          │
       │   "PRODUCT:LIST_FILTER",        │                              │                          │
       │   user, context)                │                              │                          │
       ├────────────────────────────────>│                              │                          │
       │                                 │                              │                          │
       │                                 │ 1. Lookup in _policyRegistry │                          │
       │                                 │    "PRODUCT:LIST_FILTER"     │                          │
       │                                 │    ↓                          │                          │
       │                                 │    typeof(ProductListFilter  │                          │
       │                                 │           Policy)             │                          │
       │                                 │                              │                          │
       │                                 │ 2. GetService(type)          │                          │
       │                                 ├─────────────────────────────>│                          │
       │                                 │                              │                          │
       │                                 │                              │ 3. Create instance       │
       │                                 │                              │    (with dependencies)   │
       │                                 │                              ├─────────────────────────>│
       │                                 │                              │                          │
       │                                 │                              │<─────────────────────────┤
       │                                 │                              │   instance               │
       │                                 │<─────────────────────────────┤                          │
       │                                 │   policy instance            │                          │
       │                                 │                              │                          │
       │                                 │ 4. policy.EvaluateAsync()    │                          │
       │                                 ├──────────────────────────────┼─────────────────────────>│
       │                                 │                              │                          │
       │                                 │                              │                     Execute
       │                                 │                              │                  business rules
       │                                 │                              │                          │
       │                                 │<──────────────────────────────────────────────────────┤
       │                                 │   PolicyEvaluationResult     │                          │
       │<────────────────────────────────┤                              │                          │
       │   result                        │                              │                          │
```

---

## Câu Hỏi Thường Gặp

### Q1: Tại sao phải dùng string làm policy name?

**A:** Có nhiều lý do:

1. **Flexibility**: Có thể load policy names từ config file
2. **Decoupling**: Code gọi policy không phụ thuộc vào implementation class
3. **Convention**: Giống pattern của .NET Core Authorization
4. **Readability**: `"PRODUCT:VIEW"` rõ nghĩa hơn `typeof(ProductViewPolicy)`

### Q2: Điều gì xảy ra nếu policy name không tồn tại?

```csharp
// PolicyEvaluator.cs
if (!_policyRegistry.TryGetValue(policyName, out var policyType))
{
    return PolicyEvaluationResult.Deny($"Policy '{policyName}' not found");
}
```

**Result**: Return `Deny` với message rõ ràng → User nhận 403 Forbidden

### Q3: Tại sao PolicyEvaluator là Singleton mà Policies là Scoped?

**A:**

**PolicyEvaluator (Singleton)**:
- Chỉ chứa registry (dictionary mapping)
- Không có state thay đổi giữa các request
- Registry được build 1 lần lúc startup
- → An toàn để dùng chung cho tất cả requests

**Policies (Scoped)**:
- Có thể cần dependencies theo request (DbContext, HttpContext)
- Mỗi request có instance riêng → không bị state leakage
- → An toàn và flexible

### Q4: Có thể thay đổi policies lúc runtime không?

**A:** Không được khuyến khích, nhưng có thể:

```csharp
// Có thể, nhưng KHÔNG NÊN
var evaluator = serviceProvider.GetService<PolicyEvaluator>();
evaluator.RegisterPolicy<NewPolicy>("NEW:POLICY");
```

**Vấn đề**:
- PolicyEvaluator là Singleton → thread-safety issues
- Dictionary không thread-safe khi modify
- Policies mới chưa được đăng ký trong DI container

**Giải pháp đúng**: Restart application với policies mới

### Q5: PolicyName trong Policy class có được dùng không?

```csharp
public class ProductViewPolicy : IPolicy
{
    public string PolicyName => "PRODUCT:VIEW";  // ← Này có dùng không?
    
    public async Task<PolicyEvaluationResult> EvaluateAsync(...) { }
}
```

**A:** 
- **Không dùng trong flow chính** (EvaluateAsync flow)
- **Chỉ dùng nếu** gọi `registry.AddPolicy<ProductViewPolicy>()` (không truyền name)
- **Recommended**: Luôn truyền name explicitly: `registry.AddPolicy<T>("NAME")`

---

## Tóm Tắt: Key Takeaways

### 1. Policy Resolution = Dictionary Lookup

```
Policy Name (string) → Policy Type → Policy Instance
     ↓                      ↓              ↓
"PRODUCT:LIST_FILTER" → typeof(...) → new ProductListFilterPolicy()
```

### 2. Two-Phase Process

**Startup**:
- Build registry: `{"PRODUCT:LIST_FILTER": typeof(ProductListFilterPolicy)}`
- One-time operation

**Runtime**:
- Lookup type: `"PRODUCT:LIST_FILTER"` → `typeof(ProductListFilterPolicy)`
- Get instance from DI: `_serviceProvider.GetService(type)`
- Execute: `policy.EvaluateAsync()`

### 3. Separation of Concerns

```
PolicyRegistry (Startup)     PolicyEvaluator (Runtime)
├─ Collects registrations    ├─ Resolves policies
├─ Validates types           ├─ Gets instances from DI
└─ Builds initial mapping    └─ Executes evaluation
```

### 4. The Magic is Simple

Không có magic! Chỉ là:
1. Dictionary lookup
2. DI container resolution
3. Interface method call

---

## Debugging Tips

### Xem Policy Registry

```csharp
// Thêm vào PolicyEvaluator
public IReadOnlyDictionary<string, Type> GetRegisteredPolicies()
{
    return _policyRegistry;
}

// Debug trong controller hoặc service
var evaluator = serviceProvider.GetService<PolicyEvaluator>();
foreach (var (name, type) in evaluator.GetRegisteredPolicies())
{
    Console.WriteLine($"{name} → {type.Name}");
}
// Output:
// PRODUCT:LIST_FILTER → ProductListFilterPolicy
// PRODUCT:VIEW → ProductViewPolicy
```

### Log Policy Evaluation

```csharp
// PolicyEvaluator.cs
public async Task<PolicyEvaluationResult> EvaluateAsync(...)
{
    _logger.LogDebug("Resolving policy: {PolicyName}", policyName);
    
    if (!_policyRegistry.TryGetValue(policyName, out var policyType))
    {
        _logger.LogWarning("Policy not found: {PolicyName}", policyName);
        return Deny(...);
    }
    
    _logger.LogDebug("Found policy type: {PolicyType}", policyType.Name);
    
    var policy = _serviceProvider.GetService(policyType) as IPolicy;
    
    _logger.LogDebug("Policy instance created: {IsNull}", policy == null);
    
    // ...
}
```

---

## Kết Luận

**PolicyEvaluator biết policy nào để chạy vì:**

1. ✅ **Registration time**: Developer đăng ký mapping `"PRODUCT:LIST_FILTER"` → `ProductListFilterPolicy`
2. ✅ **Dictionary storage**: Mapping được lưu trong `_policyRegistry`
3. ✅ **Runtime lookup**: Tìm type theo name trong dictionary
4. ✅ **DI resolution**: Lấy instance từ DI container
5. ✅ **Polymorphism**: Cast sang `IPolicy` và gọi `EvaluateAsync()`

**Đơn giản nhưng powerful!** 🎯

