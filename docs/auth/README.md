# PBAC (Policy-Based Access Control) Documentation

## 📚 Tài Liệu

### 🇻🇳 Tiếng Việt (Đọc Đầu Tiên)

1. **[PBAC-Summary-VI.md](./PBAC-Summary-VI.md)** ⭐ **BẮT ĐẦU TỪ ĐÂY**
   - Tổng quan kiến trúc PBAC
   - Giải thích tại sao thiết kế như vậy
   - So sánh trước/sau khi đơn giản hóa
   - **Đọc file này trước để hiểu big picture**

2. **[PBAC-Policy-Resolution-Explained.md](./PBAC-Policy-Resolution-Explained.md)** ⭐ **QUAN TRỌNG**
   - Giải thích chi tiết: PolicyEvaluator biết policy nào để chạy như thế nào?
   - Phân tích cơ chế resolution từ policy name → policy implementation
   - Flow đăng ký và runtime
   - **Đọc file này để hiểu cơ chế core của PBAC**

3. **[PBAC-Step-By-Step-Example.md](./PBAC-Step-By-Step-Example.md)** ⭐ **CODE TRACING**
   - Trace code thực tế từng bước
   - Từ startup đến runtime execution
   - Line-by-line explanation với console output
   - **Đọc file này để thấy code chạy như thế nào**

4. **[PBAC-Why-Two-Lists.md](./PBAC-Why-Two-Lists.md)** ⭐ **DESIGN DEEP DIVE**
   - Giải thích tại sao cần 2 data structures (List vs Dictionary)
   - Lifecycle analysis: Temporary vs Permanent
   - Performance comparison
   - **Đọc file này để hiểu design decisions**

### 🇬🇧 English (Detailed Guides)

5. **[PBAC-Architecture.md](./PBAC-Architecture.md)**
   - Complete architecture documentation
   - Component details and relationships
   - Design decisions and rationale
   - Performance considerations
   - Security best practices

6. **[PBAC-Usage-Guide.md](./PBAC-Usage-Guide.md)**
   - Practical usage examples
   - Common patterns
   - Best practices
   - Testing strategies
   - Troubleshooting guide

## 📖 Đọc Theo Thứ Tự

### Nếu bạn muốn hiểu NHANH:

```
1. PBAC-Summary-VI.md (15 phút)
   ↓ Hiểu big picture
   
2. PBAC-Policy-Resolution-Explained.md (20 phút)
   ↓ Hiểu cơ chế core
   
3. PBAC-Step-By-Step-Example.md (15 phút)
   ↓ Thấy code chạy

4. PBAC-Why-Two-Lists.md (10 phút) [OPTIONAL]
   ↓ Hiểu design decisions
```

**Tổng: ~50 phút (hoặc ~60 phút nếu đọc thêm design deep dive)**

### Nếu bạn muốn hiểu SÂU:

```
1. PBAC-Summary-VI.md
   ↓
2. PBAC-Policy-Resolution-Explained.md
   ↓
3. PBAC-Step-By-Step-Example.md
   ↓
4. PBAC-Why-Two-Lists.md (Design decisions)
   ↓
5. PBAC-Architecture.md (Architecture details)
   ↓
6. PBAC-Usage-Guide.md (Advanced patterns)
```

## 🎯 Quick Reference

### Registration (Program.cs)

```csharp
services.AddPolicyBasedAuthorization(registry =>
{
    registry.AddPolicy<ProductViewPolicy>("PRODUCT:VIEW");
    registry.AddPolicy<ProductCreatePolicy>("PRODUCT:CREATE");
});
```

### Create Policy

```csharp
public class ProductViewPolicy : IPolicy
{
    public string PolicyName => "PRODUCT:VIEW";
    
    public async Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // Business rules
        if (user.HasRole("Admin"))
            return PolicyEvaluationResult.Allow();
        
        return PolicyEvaluationResult.Deny("Access denied");
    }
}
```

### Use in Service

```csharp
public class ProductPolicyService : IProductPolicyService
{
    private readonly IPolicyEvaluator _evaluator;
    
    public async Task<PolicyEvaluationResult> CanViewProductAsync(long id, decimal price)
    {
        var user = _userContextAccessor.GetCurrentUserContext();
        var context = new Dictionary<string, object>
        {
            { "ProductId", id },
            { "ProductPrice", price }
        };
        
        return await _evaluator.EvaluateAsync("PRODUCT:VIEW", user, context);
    }
}
```

### Use in Controller

```csharp
[HttpGet("{id}")]
[Authorize(Policy = "BasicUser")]  // RBAC
public async Task<IActionResult> GetProduct(long id)
{
    var product = await GetProductFromDb(id);
    
    // PBAC
    var result = await _policyService.CanViewProductAsync(id, product.Price);
    
    if (!result.IsAllowed)
        return StatusCode(403, result.Reason);
    
    return Ok(product);
}
```

## 💡 Key Concepts

### Policy Resolution Flow

```
"PRODUCT:VIEW" (string) 
   ↓ Dictionary Lookup
typeof(ProductViewPolicy)
   ↓ DI Container
new ProductViewPolicy(dependencies)
   ↓ Execute
policy.EvaluateAsync(user, context)
```

### Layered Authorization

```
Layer 1: RBAC (Gateway)
   [Authorize(Policy = "BasicUser")]
   ↓ Pass
   
Layer 2: PBAC (Service)
   await _policyService.CanViewProductAsync(...)
   ↓ Evaluate business rules
   
Result: Allow/Deny
```

## 🔍 Câu Hỏi Thường Gặp

### Q: PolicyEvaluator biết policy nào để chạy như thế nào?

**A:** Xem [PBAC-Policy-Resolution-Explained.md](./PBAC-Policy-Resolution-Explained.md)

Tóm tắt: Dictionary lookup
```
Startup: registry["PRODUCT:VIEW"] = typeof(ProductViewPolicy)
Runtime: registry["PRODUCT:VIEW"] → typeof(ProductViewPolicy) → instance
```

### Q: Tại sao có PolicyService layer?

**A:** Xem [PBAC-Summary-VI.md](./PBAC-Summary-VI.md#2-policy-service-layer)

Tóm tắt:
- ✅ API domain-specific, dễ dùng
- ✅ Encapsulate context preparation
- ✅ Dễ test (mock service thay vì evaluator)

### Q: Tại sao PolicyEvaluator là Singleton?

**A:** Xem [PBAC-Architecture.md](./PBAC-Architecture.md#why-this-design)

Tóm tắt:
- Policy registry không đổi sau startup
- Không có state giữa requests
- Performance tốt hơn

### Q: Làm sao trace code?

**A:** Xem [PBAC-Step-By-Step-Example.md](./PBAC-Step-By-Step-Example.md)

### Q: Tại sao cần 2 lists (PolicyRegistry._policies và PolicyEvaluator._policyRegistry)?

**A:** Xem [PBAC-Why-Two-Lists.md](./PBAC-Why-Two-Lists.md)

Tóm tắt:
- List (temporary): Dùng lúc startup để collect
- Dictionary (permanent): Dùng lúc runtime cho O(1) lookup
- Không thể share vì lifecycle và purpose khác nhau

## 📊 Architecture Overview

```
┌─────────────────────────────────────────┐
│           Controller                    │
│  - HTTP Layer                           │
│  - RBAC via [Authorize]                 │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│       ProductPolicyService              │
│  - Domain-specific API                  │
│  - Context preparation                  │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         PolicyEvaluator                 │
│  - Policy resolution                    │
│  - Dictionary: name → type              │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         Concrete Policy                 │
│  - Business rules                       │
│  - Returns Allow/Deny                   │
└─────────────────────────────────────────┘
```

## 🚀 Getting Started

1. **Hiểu hệ thống** → Đọc [PBAC-Summary-VI.md](./PBAC-Summary-VI.md)

2. **Hiểu cơ chế** → Đọc [PBAC-Policy-Resolution-Explained.md](./PBAC-Policy-Resolution-Explained.md)

3. **Xem code** → Đọc [PBAC-Step-By-Step-Example.md](./PBAC-Step-By-Step-Example.md)

4. **Implement** → Follow [PBAC-Usage-Guide.md](./PBAC-Usage-Guide.md)

## 📝 Tóm Tắt: Những Điểm Chính

### 1. Code Đã Được Đơn Giản Hóa

**Trước:**
- ❌ Comments dài dòng trong code
- ❌ Giải thích lặp lại
- ❌ Khó đọc, khó maintain

**Sau:**
- ✅ Code ngắn gọn, rõ ràng
- ✅ Comments ngắn gọn
- ✅ Chi tiết trong documentation riêng

### 2. Injection Đơn Giản

```csharp
// Chỉ cần 1 dòng trong Program.cs
services.AddPolicyBasedAuthorization(registry => {
    registry.AddPolicy<YourPolicy>("POLICY:NAME");
});

// Dùng trong service
public MyService(IPolicyEvaluator evaluator) { }
```

### 3. Policy Resolution

```
Policy Name → Dictionary Lookup → Policy Type → DI Resolution → Instance → Execute
```

### 4. Layered Security

```
RBAC (Role check) + PBAC (Business rules) = Defense in Depth
```

## 🛠️ Tools & Debugging

### View Registered Policies

```csharp
var evaluator = serviceProvider.GetRequiredService<PolicyEvaluator>();
// Use reflection to view _policyRegistry
```

### Enable Logging

```csharp
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Infrastructure.Authorization": "Debug"
    }
  }
}
```

## 📚 Related Files in Codebase

### Core Implementation
- `Infrastructure/Authorization/PolicyEvaluator.cs` - Policy resolution engine
- `Infrastructure/Authorization/Interfaces/IPolicyEvaluator.cs` - Interface
- `Infrastructure/Extensions/PolicyAuthorizationExtensions.cs` - Registration API

### Example Policies
- `Infrastructure/Authorization/Policies/ProductViewPolicy.cs`
- `Infrastructure/Authorization/Policies/ProductCreatePolicy.cs`
- `Infrastructure/Authorization/Policies/ProductListFilterPolicy.cs`

### Usage Examples
- `Base.API/Controllers/ProductControllerWithPBAC.cs` - Controller usage
- `Base.Application/Feature/Product/Services/ProductPolicyService.cs` - Service layer
- `Base.API/Program.cs` - Registration

## 🎓 Learning Path

### Beginner (Người mới)
→ Đọc [PBAC-Summary-VI.md](./PBAC-Summary-VI.md)

### Intermediate (Đã hiểu basic)
→ Đọc [PBAC-Policy-Resolution-Explained.md](./PBAC-Policy-Resolution-Explained.md)
→ Đọc [PBAC-Step-By-Step-Example.md](./PBAC-Step-By-Step-Example.md)

### Advanced (Muốn hiểu sâu)
→ Đọc [PBAC-Architecture.md](./PBAC-Architecture.md)
→ Đọc [PBAC-Usage-Guide.md](./PBAC-Usage-Guide.md)

## ✅ Summary

Hệ thống PBAC đã được:
- ✅ **Đơn giản hóa**: Loại bỏ comments dài, code clean
- ✅ **Document đầy đủ**: Chi tiết trong files riêng
- ✅ **Giải thích rõ ràng**: Mechanism, flow, rationale
- ✅ **Examples thực tế**: Step-by-step tracing

**Bắt đầu từ [PBAC-Summary-VI.md](./PBAC-Summary-VI.md)! 🚀**

---

*Last updated: 2025-10-31*

