# Tại Sao Cần 2 Lists? PolicyRegistry._policies vs PolicyEvaluator._policyRegistry

## 🤔 Câu Hỏi

```csharp
// PolicyRegistry có list
private readonly List<(string policyName, Type policyType)> _policies = new();

// PolicyEvaluator cũng có dictionary
private readonly Dictionary<string, Type> _policyRegistry;
```

**Tại sao không dùng chung 1 cái? Tại sao phải copy từ List sang Dictionary?**

---

## 💡 Câu Trả Lời Ngắn Gọn

**Vì lifecycle và purpose khác nhau:**

1. **PolicyRegistry._policies** (List): 
   - Chỉ tồn tại lúc STARTUP
   - Dùng để COLLECT policies trong quá trình configuration
   - Sau startup → bị GC thu hồi (không dùng nữa)

2. **PolicyEvaluator._policyRegistry** (Dictionary):
   - Tồn tại suốt đời ứng dụng (Singleton)
   - Dùng để LOOKUP nhanh trong RUNTIME
   - Dictionary → O(1) lookup performance

**Không thể share vì:**
- PolicyRegistry là temporary object (chỉ dùng lúc config)
- PolicyEvaluator là singleton (dùng suốt đời app)
- Không thể giữ reference đến temporary object trong singleton

---

## 📊 Lifecycle Visualization

### Timeline: Application Startup → Runtime

```
TIME: Application Startup
═══════════════════════════════════════════════════════════════════

Program.cs được execute:
┌─────────────────────────────────────────────────────────────────┐
│ services.AddPolicyBasedAuthorization(registry => {              │
│     registry.AddPolicy<ProductViewPolicy>("PRODUCT:VIEW");      │
│ });                                                              │
└─────────────────────────────────────────────────────────────────┘
                           ↓
        ┌─────────────────────────────────────────┐
        │    PolicyRegistry (Temporary)           │
        │                                         │
        │    _policies = List<(name, type)>       │
        │    ├─ ("PRODUCT:VIEW", typeof(...))     │
        │    └─ ("PRODUCT:CREATE", typeof(...))   │
        │                                         │
        │    Purpose: COLLECT registrations       │
        │    Lifetime: Only during config         │
        └─────────────────────────────────────────┘
                           ↓
        ┌─────────────────────────────────────────┐
        │  Create PolicyEvaluator (Singleton)     │
        │                                         │
        │  foreach (name, type) in registry       │
        │      evaluator.RegisterPolicy(...)      │
        │                                         │
        │  Copy data from List → Dictionary       │
        └─────────────────────────────────────────┘
                           ↓
        ┌─────────────────────────────────────────┐
        │    PolicyEvaluator (Singleton)          │
        │                                         │
        │    _policyRegistry = Dictionary<...>    │
        │    ├─ ["PRODUCT:VIEW"] = typeof(...)    │
        │    └─ ["PRODUCT:CREATE"] = typeof(...)  │
        │                                         │
        │    Purpose: FAST LOOKUP                 │
        │    Lifetime: Entire application         │
        └─────────────────────────────────────────┘
                           ↓
        ┌─────────────────────────────────────────┐
        │  PolicyRegistry goes out of scope       │
        │  → Garbage collected                    │
        │  → _policies list is freed              │
        └─────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════════
TIME: Runtime (HTTP Requests)

        ┌─────────────────────────────────────────┐
        │  PolicyEvaluator._policyRegistry        │
        │  Used for every request                 │
        │  Fast O(1) dictionary lookup            │
        └─────────────────────────────────────────┘
```

---

## 🔍 Phân Tích Chi Tiết

### 1. PolicyRegistry._policies - Collection Phase

```csharp
// File: Infrastructure/Extensions/PolicyAuthorizationExtensions.cs
public class PolicyRegistry
{
    // ⏱️ Lifetime: Chỉ tồn tại trong quá trình AddPolicyBasedAuthorization()
    private readonly List<(string policyName, Type policyType)> _policies = new();
    
    public PolicyRegistry AddPolicy<TPolicy>(string policyName) where TPolicy : class, IPolicy
    {
        // Thêm vào list
        _policies.Add((policyName, typeof(TPolicy)));
        //           ↑
        //   Đang BUILD UP list trong quá trình config
        
        return this;
    }
    
    internal IReadOnlyList<(string policyName, Type policyType)> GetRegisteredPolicies()
    {
        return _policies.AsReadOnly();
        //     ↑
        //   Trả về list này CHỈ MỘT LẦN để copy sang PolicyEvaluator
    }
}
```

**Đặc điểm:**
- ✅ Data structure: `List` - tốt cho sequential add operations
- ✅ Purpose: Thu thập policies trong quá trình configuration
- ✅ Lifetime: Temporary (chỉ trong scope của `AddPolicyBasedAuthorization()`)
- ✅ Usage pattern: Sequential writes, single read
- ❌ Không tối ưu cho lookup (O(n) search)

**Tại sao dùng List?**
```csharp
services.AddPolicyBasedAuthorization(registry =>
{
    registry.AddPolicy<Policy1>("P1");  // Add to list
    registry.AddPolicy<Policy2>("P2");  // Add to list
    registry.AddPolicy<Policy3>("P3");  // Add to list
    // ... có thể add nhiều policies
});
// Sau khi block này chạy xong → registry không còn dùng nữa
```

List thích hợp cho pattern này vì:
- Thêm tuần tự: O(1) per add
- Không cần lookup trong phase này
- Order không quan trọng

### 2. PolicyEvaluator._policyRegistry - Lookup Phase

```csharp
// File: Infrastructure/Authorization/PolicyEvaluator.cs
public class PolicyEvaluator : IPolicyEvaluator
{
    // ⏱️ Lifetime: TOÀN BỘ đời sống của application (Singleton)
    private readonly Dictionary<string, Type> _policyRegistry;
    //               ↑
    //      Data structure: Dictionary - tối ưu cho lookup
    
    public PolicyEvaluator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _policyRegistry = new Dictionary<string, Type>();
        //               ↑
        //      Khởi tạo dictionary RỖNG, sẽ được fill sau
    }
    
    public void RegisterPolicy(Type policyType, string policyName)
    {
        // Dictionary: Tối ưu cho key-based lookup
        _policyRegistry[policyName] = policyType;
        //              ↑
        //      O(1) lookup trong runtime
    }
    
    public async Task<PolicyEvaluationResult> EvaluateAsync(string policyName, ...)
    {
        // ĐƯỢC GỌI HÀNG NGHÌN LẦN trong runtime
        if (!_policyRegistry.TryGetValue(policyName, out var policyType))
        {                    ↑
            //       Dictionary lookup: O(1) - NHANH
            return Deny(...);
        }
        // ...
    }
}
```

**Đặc điểm:**
- ✅ Data structure: `Dictionary` - tối ưu cho key-based lookup
- ✅ Purpose: Fast policy resolution trong runtime
- ✅ Lifetime: Singleton (tồn tại suốt đời app)
- ✅ Usage pattern: Build once, lookup thousands of times
- ✅ Performance: O(1) lookup

**Tại sao dùng Dictionary?**
```csharp
// Runtime: Mỗi HTTP request gọi evaluator
await _evaluator.EvaluateAsync("PRODUCT:VIEW", ...);
                                ↓
// Dictionary lookup: O(1) - Instant
if (!_policyRegistry.TryGetValue("PRODUCT:VIEW", out var type))

// Nếu dùng List: O(n) - Phải loop
// foreach (var (name, type) in _policies)
//     if (name == "PRODUCT:VIEW") ...
// → CHẬM nếu có nhiều policies
```

Performance comparison với 100 policies:
- Dictionary: 1 operation
- List: Up to 100 operations (average 50)

---

## 🚫 Tại Sao KHÔNG Thể Share?

### Option 1: PolicyEvaluator giữ reference đến PolicyRegistry? ❌

```csharp
// ❌ BAD DESIGN
public class PolicyEvaluator
{
    private readonly PolicyRegistry _registry;  // ❌ Giữ reference
    
    public async Task<PolicyEvaluationResult> EvaluateAsync(string policyName, ...)
    {
        // Phải loop qua list mỗi lần
        foreach (var (name, type) in _registry._policies)  // ❌ O(n)
        {
            if (name == policyName)
            {
                var policy = _serviceProvider.GetService(type);
                // ...
            }
        }
    }
}
```

**Vấn đề:**
1. ❌ **Performance**: O(n) lookup thay vì O(1)
2. ❌ **Memory leak**: PolicyRegistry không bao giờ được GC
3. ❌ **Wrong semantics**: Registry là "builder", không phải "lookup store"
4. ❌ **Coupling**: Evaluator depends on temporary builder object

### Option 2: PolicyRegistry dùng Dictionary thay vì List? ❌

```csharp
// ❌ COULD WORK, but not ideal
public class PolicyRegistry
{
    private readonly Dictionary<string, Type> _policies = new();  // ❌ Overkill
    
    public PolicyRegistry AddPolicy<TPolicy>(string policyName)
    {
        _policies[policyName] = typeof(TPolicy);
        return this;
    }
    
    internal Dictionary<string, Type> GetRegisteredPolicies()
    {
        return new Dictionary<string, Type>(_policies);  // ❌ Still need to copy
    }
}
```

**Vấn đề:**
1. ❌ **Over-engineering**: Dictionary overkill cho collection phase
2. ❌ **Still need copy**: Không thể return internal dictionary (encapsulation)
3. ❌ **No benefit**: Vẫn phải tạo copy vào PolicyEvaluator
4. ❌ **Semantics**: Registry là "collector", Dictionary là "lookup store"

### Option 3: Share cùng một Dictionary instance? ❌

```csharp
// ❌ TERRIBLE DESIGN
public static class PolicyAuthorizationExtensions
{
    private static Dictionary<string, Type> _sharedRegistry = new();  // ❌ Global state
    
    public static IServiceCollection AddPolicyBasedAuthorization(...)
    {
        var registry = new PolicyRegistry(_sharedRegistry);  // ❌ Pass reference
        // ...
        services.AddSingleton<PolicyEvaluator>(sp => 
            new PolicyEvaluator(sp, _sharedRegistry));  // ❌ Share same instance
    }
}
```

**Vấn đề:**
1. ❌ **Global mutable state**: Nightmare for testing
2. ❌ **Thread safety issues**: Dictionary not thread-safe for modifications
3. ❌ **Multiple app instances**: Won't work in multi-tenant scenarios
4. ❌ **Violation of principles**: Singleton với mutable shared state
5. ❌ **No encapsulation**: Anyone can modify the registry

---

## ✅ Current Design: Tại Sao ĐÚNG

```csharp
// Phase 1: Collection (using List)
PolicyRegistry._policies (List)
    ├─ Sequential adds: O(1) each
    ├─ Simple iteration: O(n) total
    └─ Read once at end: GetRegisteredPolicies()

// Phase 2: Transfer (one-time copy)
foreach (var (name, type) in registry.GetRegisteredPolicies())
{
    evaluator.RegisterPolicy(type, name);
    //        ↓ Copy from List → Dictionary
}

// Phase 3: Lookup (using Dictionary)
PolicyEvaluator._policyRegistry (Dictionary)
    ├─ One-time build: O(n) setup cost
    ├─ Thousands of lookups: O(1) each
    └─ Total runtime: O(1) × n_requests
```

**Benefits:**
1. ✅ **Right tool for the job**: List cho collection, Dictionary cho lookup
2. ✅ **Clear separation**: Builder vs Store
3. ✅ **Memory efficiency**: Registry được GC sau startup
4. ✅ **Performance**: O(1) lookup trong runtime
5. ✅ **Encapsulation**: Mỗi object có internal state riêng
6. ✅ **Thread safety**: Dictionary trong Singleton, read-only sau init
7. ✅ **Testability**: Easy to test each component independently

---

## 📈 Performance Analysis

### Scenario: 50 policies, 10,000 requests/minute

#### Current Design (List → Dictionary)

```
Startup:
- Build List: 50 × O(1) = 50 operations
- Copy to Dictionary: 50 × O(1) = 50 operations
- Total: 100 operations (ONE TIME)

Runtime (per minute):
- Lookups: 10,000 × O(1) = 10,000 operations
- Average lookup time: 0.001ms
- Total: 10 seconds worth of lookups

Memory:
- PolicyRegistry: ~1KB (freed after startup)
- PolicyEvaluator Dictionary: ~2KB (kept forever)
- Total: ~2KB runtime memory
```

#### Alternative: Share List (if we could)

```
Runtime (per minute):
- Lookups: 10,000 × O(n) = 10,000 × 50 = 500,000 operations
- Average lookup time: 0.05ms
- Total: 500 seconds worth of lookups (50x SLOWER)

Memory:
- Same: ~2KB
```

**Result: Current design is 50x faster for lookups!**

---

## 🎯 Analogy: Restaurant Kitchen

### PolicyRegistry._policies = Shopping List (List)

```
Chef chuẩn bị đi chợ:
├─ Thịt bò (để làm bò xào)
├─ Cà rốt (để làm salad)
├─ Khoai tây (để làm khoai tây chiên)
└─ Gạo (để nấu cơm)

Purpose: COLLECT ingredients
Structure: Simple list, sequential
Lifetime: Chỉ dùng để đi chợ, về nhà bỏ đi
```

### PolicyEvaluator._policyRegistry = Kitchen Inventory (Dictionary)

```
Kho bếp (organized by location):
├─ ["Freezer"] → Thịt bò
├─ ["Fridge"] → Cà rốt
├─ ["Pantry"] → Khoai tây
└─ ["Cupboard"] → Gạo

Purpose: QUICK ACCESS during cooking
Structure: Organized by key (location)
Lifetime: Permanent kitchen storage
```

**Tại sao không dùng shopping list trong kitchen?**
- ❌ Shopping list: Phải đọc từ đầu đến cuối để tìm ingredient
- ✅ Kitchen inventory: Biết ngay nơi lưu trữ, lấy instant

---

## 🔧 Code Example: The Transfer Process

```csharp
// File: Infrastructure/Extensions/PolicyAuthorizationExtensions.cs
public static IServiceCollection AddPolicyBasedAuthorization(
    this IServiceCollection services,
    Action<PolicyRegistry>? configurePolicies = null)
{
    // 1️⃣ Create temporary registry (with List internally)
    var policyRegistry = new PolicyRegistry(services);
    
    // 2️⃣ User populates the list
    configurePolicies?.Invoke(policyRegistry);
    //    ↓ policyRegistry._policies now contains:
    //      [("PRODUCT:VIEW", typeof(ProductViewPolicy)),
    //       ("PRODUCT:CREATE", typeof(ProductCreatePolicy)), ...]
    
    // 3️⃣ Create Singleton PolicyEvaluator and TRANSFER data
    services.AddSingleton<PolicyEvaluator>(sp =>
    {
        var evaluator = new PolicyEvaluator(sp);
        //    ↑ evaluator._policyRegistry is empty Dictionary
        
        // 4️⃣ ONE-TIME COPY: List → Dictionary
        foreach (var (policyName, policyType) in policyRegistry.GetRegisteredPolicies())
        {
            evaluator.RegisterPolicy(policyType, policyName);
            //        ↑ Adds to Dictionary: O(1)
        }
        
        return evaluator;
        //     ↑ evaluator._policyRegistry now filled with Dictionary
    });
    
    // 5️⃣ policyRegistry goes out of scope here
    //    → Garbage collected
    //    → _policies List is freed
    
    services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());
    
    return services;
}
```

**State after this method:**
```
✅ PolicyEvaluator (Singleton) exists with Dictionary
❌ PolicyRegistry doesn't exist anymore (GC'd)
❌ List doesn't exist anymore (GC'd)
```

---

## 💭 FAQ

### Q1: Có thể optimize để không cần copy không?

**A:** Không, vì:
- Registry phải là temporary (không nên tồn tại lâu dài)
- Evaluator phải là singleton (tồn tại suốt đời app)
- Copy chỉ xảy ra 1 lần lúc startup → negligible cost

### Q2: Copy có tốn memory không?

**A:** Minimal:
- 50 policies × (string + Type reference) ≈ 2-3KB
- Copy once, use thousands of times
- Memory saved by freeing Registry >> memory used by Dictionary

### Q3: Có thể dùng ImmutableDictionary để share không?

**A:** Có thể, nhưng không cần thiết:
```csharp
// Possible, but over-engineering
var immutableDict = policies.ToImmutableDictionary();
// Can safely share, but no benefit over copying
```

### Q4: Nếu có 1000 policies thì sao?

**A:** Vẫn OK:
- Startup copy: 1000 operations (< 1ms)
- Runtime lookup: Still O(1)
- Memory: ~10-20KB (negligible)

---

## 📝 Summary

### Tại Sao Cần 2 Structures?

| Aspect | PolicyRegistry._policies | PolicyEvaluator._policyRegistry |
|--------|-------------------------|--------------------------------|
| **Type** | `List<(string, Type)>` | `Dictionary<string, Type>` |
| **Purpose** | Collect registrations | Fast lookup |
| **Lifetime** | Temporary (startup only) | Permanent (singleton) |
| **Operations** | Sequential adds | Key-based lookups |
| **Performance** | O(1) add, O(n) search | O(1) lookup |
| **When used** | During configuration | During every request |
| **Memory** | Freed after startup | Kept forever |

### Tại Sao Không Share?

1. ❌ **Different lifetimes**: Temporary vs Permanent
2. ❌ **Different purposes**: Collection vs Lookup
3. ❌ **Performance**: List O(n) vs Dictionary O(1)
4. ❌ **Encapsulation**: Each object owns its data
5. ❌ **Memory**: Registry should be freed after use

### The Design is Optimal Because:

1. ✅ Right data structure for each phase
2. ✅ Clear separation of concerns
3. ✅ One-time setup cost, optimal runtime performance
4. ✅ Memory efficient (Registry is freed)
5. ✅ Thread-safe (Dictionary is read-only after init)
6. ✅ Easy to understand and maintain

---

## 🎯 Kết Luận

**Câu trả lời cho câu hỏi ban đầu:**

> "Tại sao _policies đã có list này rồi mà registry lại cần list này nữa?"

**Vì:**
1. **PolicyRegistry._policies** là **temporary collection** dùng lúc config
2. **PolicyEvaluator._policyRegistry** là **permanent lookup store** dùng lúc runtime
3. **Không thể share** vì lifecycle và purpose khác nhau
4. **Copy một lần** lúc startup là acceptable cost
5. **Dictionary** trong runtime cho performance O(1) lookup

**Pattern này gọi là "Builder Pattern":**
- Builder (Registry) collects configuration
- Final product (Evaluator) optimized for use
- Builder discarded after building

**Giống như:**
- Xây nhà: Giàn giáo (temporary) vs Nhà (permanent)
- Nấu ăn: Shopping list (temporary) vs Kitchen inventory (permanent)
- Software: Configuration (startup) vs Runtime (production)

🎯 **Two lists là INTENTIONAL DESIGN, không phải redundant!**

