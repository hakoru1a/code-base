# Tóm Tắt Kiến Trúc PBAC (Tiếng Việt)

## Tổng Quan

**PBAC (Policy-Based Access Control)** là hệ thống phân quyền chi tiết dựa trên các chính sách nghiệp vụ động, không chỉ kiểm tra role mà còn kiểm tra các thuộc tính của user, resource và context.

## Kiến Trúc

### Các Thành Phần Chính

```
Controller (API Layer)
    ↓
ProductPolicyService (Business Logic)
    ↓
PolicyEvaluator (Policy Engine)
    ↓
Concrete Policy (Business Rules)
```

### Giải Thích Từng Tầng

#### 1. **Controller** (`ProductControllerWithPBAC`)
- Nhận HTTP request
- Áp dụng RBAC qua `[Authorize]` (kiểm tra role ở gateway)
- Gọi `IProductPolicyService` để kiểm tra chi tiết
- Trả về 403 Forbidden nếu bị từ chối

**Tại sao đơn giản hóa?**
- ✅ Loại bỏ các comment dài dòng
- ✅ Logic rõ ràng, dễ đọc
- ✅ Không bị phân tâm bởi giải thích inline

#### 2. **ProductPolicyService**
- Đóng gói toàn bộ logic policy
- Chuẩn bị context cho việc đánh giá
- Cung cấp API domain-specific (`CanViewProductAsync`, `CanCreateProductAsync`)

**Tại sao cần layer này?**
- ✅ Controller không cần biết chi tiết về policy
- ✅ Dễ test (mock service thay vì mock evaluator)
- ✅ Tái sử dụng logic policy
- ✅ API rõ nghĩa nghiệp vụ

#### 3. **PolicyEvaluator** (Singleton)
- Quản lý registry của tất cả policies
- Resolve policy theo tên
- Thực thi policy evaluation

**Tại sao Singleton?**
- ✅ Policy registry không đổi sau khi khởi động
- ✅ Không có state giữa các request
- ✅ Performance tốt hơn (không tạo instance nhiều lần)
- ✅ Một registry duy nhất cho toàn ứng dụng

#### 4. **Policy** (Scoped)
- Chứa business rules cụ thể
- Đánh giá user + context → Allow/Deny

**Tại sao Scoped?**
- ✅ Policy có thể cần dependencies theo request (DbContext, HttpContext)
- ✅ Instance mới mỗi request → không bị leak state
- ✅ Phù hợp với pattern ASP.NET Core

## Flow Xử Lý Request

```
1. HTTP Request đến Controller
   ↓
2. [Authorize] kiểm tra RBAC (role)
   ↓ (Pass)
3. Controller gọi ProductPolicyService
   → _policyService.CanViewProductAsync(id, price)
   ↓
4. PolicyService chuẩn bị context:
   - User claims (từ JWT)
   - Resource attributes (price, category)
   ↓
5. PolicyService gọi PolicyEvaluator
   → _evaluator.EvaluateAsync("PRODUCT:VIEW", user, context)
   ↓
6. PolicyEvaluator resolve policy từ registry
   ↓
7. Policy thực thi business rules
   - Kiểm tra role
   - Kiểm tra giá trị từ JWT claims
   - Áp dụng logic nghiệp vụ
   ↓
8. Trả về kết quả:
   → Allow: Return 200 OK
   → Deny: Return 403 Forbidden + reason
```

## Flow Đăng Ký (Startup)

```
Program.cs
↓
services.AddPolicyBasedAuthorization(registry => {
    registry.AddPolicy<ProductViewPolicy>("PRODUCT:VIEW");
})
↓
1. Đăng ký infrastructure services
   - HttpContextAccessor
   - UserContextAccessor
   - PolicyConfigurationService
↓
2. Tạo PolicyRegistry
   - User register policies
   - Mỗi policy được thêm vào DI container (Scoped)
   - Track (name, type) cho sau này
↓
3. Tạo PolicyEvaluator (Singleton)
   - Lặp qua tất cả policies đã track
   - Đăng ký vào internal dictionary
   - Registry name → type
↓
4. Đăng ký IPolicyEvaluator interface
   - Point đến cùng singleton instance
```

## Tại Sao Thiết Kế Như Vậy?

### 1. Separation of Concerns (Tách Biệt Trách Nhiệm)

**Trước đây** (Logic trộn lẫn):
```csharp
public async Task<IActionResult> GetProduct(long id)
{
    var product = await GetProduct(id);
    
    // Business logic trộn trong controller
    if (User.IsInRole("Basic") && product.Price > 5_000_000)
        return Forbid();
    
    return Ok(product);
}
```

**Bây giờ** (Logic tách biệt):
```csharp
public async Task<IActionResult> GetProduct(long id)
{
    var product = await GetProduct(id);
    
    // Delegate cho policy service
    var result = await _policyService.CanViewProductAsync(id, product.Price);
    
    if (!result.IsAllowed)
        return StatusCode(403, result.Reason);
    
    return Ok(product);
}
```

**Lợi ích**:
- ✅ Controller chỉ lo HTTP concerns
- ✅ Business rules tập trung ở Policy
- ✅ Dễ test từng layer
- ✅ Dễ maintain và extend

### 2. Policy Service Layer

**Tại sao không gọi trực tiếp PolicyEvaluator từ Controller?**

**Nếu không có Policy Service**:
```csharp
// ❌ Verbose, lặp code, khó maintain
var context = new Dictionary<string, object>
{
    { "ProductPrice", price },
    { "ProductId", id }
};
var user = _userContextAccessor.GetCurrentUserContext();
var result = await _evaluator.EvaluateAsync("PRODUCT:VIEW", user, context);
```

**Với Policy Service**:
```csharp
// ✅ Clean, rõ nghĩa, dễ dùng
var result = await _policyService.CanViewProductAsync(id, price);
```

**Lợi ích**:
- ✅ API rõ ràng, domain-specific
- ✅ Encapsulate logic chuẩn bị context
- ✅ Tái sử dụng code
- ✅ Dễ mock trong unit test

### 3. Registry Pattern

**Tại sao cần PolicyRegistry?**

- ✅ Fluent API dễ sử dụng
- ✅ Validate policy types ngay khi đăng ký
- ✅ Thu thập tất cả policies trước khi khởi tạo evaluator
- ✅ Tránh circular dependency

### 4. Phân Quyền Nhiều Tầng (RBAC + PBAC)

```
Tầng 1: RBAC (Gateway/Controller)
├─→ Nhanh, kiểm tra thô (coarse-grained)
├─→ Kiểm tra role của user
└─→ [Authorize(Policy = "BasicUser")]

Tầng 2: PBAC (Service/Business Logic)  
├─→ Chi tiết, kiểm tra context (fine-grained)
├─→ Kiểm tra attributes + business rules
└─→ await _policyService.CanViewProductAsync(...)
```

**Lợi ích Defense in Depth**:
- ✅ **Performance**: RBAC chặn user không hợp lệ sớm
- ✅ **Security**: PBAC đảm bảo business rules được enforce
- ✅ **Flexibility**: Policies khác nhau cho từng operation

## Ví Dụ Cụ Thể

### Use Case: Xem Sản Phẩm

**Yêu cầu nghiệp vụ**:
- User Basic chỉ xem được sản phẩm dưới 5 triệu VND
- User Premium xem được tất cả
- Giới hạn giá có thể tùy chỉnh qua JWT claim

**Implementation**:

```csharp
// 1. Controller - Đơn giản, rõ ràng
[Authorize(Policy = "BasicUser")]
public async Task<IActionResult> GetProductById(long id)
{
    var product = await GetProduct(id);
    
    var policyCheck = await _policyService.CanViewProductAsync(id, product.Price);
    
    if (!policyCheck.IsAllowed)
        return StatusCode(403, policyCheck.Reason);
    
    return Ok(product);
}

// 2. Policy Service - Chuẩn bị context
public async Task<PolicyEvaluationResult> CanViewProductAsync(long id, decimal price)
{
    var user = _userContextAccessor.GetCurrentUserContext();
    
    var context = new Dictionary<string, object>
    {
        { "ProductPrice", price },
        { "ProductId", id }
    };
    
    return await _policyEvaluator.EvaluateAsync("PRODUCT:VIEW", user, context);
}

// 3. Policy - Business rules
public async Task<PolicyEvaluationResult> EvaluateAsync(
    UserClaimsContext user, 
    Dictionary<string, object> context)
{
    // Admin/Premium xem tất cả
    if (user.HasRole("Admin") || user.HasRole("Premium"))
        return Allow();
    
    // Kiểm tra giới hạn giá cho Basic user
    var price = (decimal)context["ProductPrice"];
    var maxPrice = user.GetClaim<decimal?>("MaxProductPrice") ?? 5_000_000m;
    
    if (price > maxPrice)
        return Deny($"Giá {price:N0} vượt quá giới hạn {maxPrice:N0}");
    
    return Allow();
}
```

## So Sánh: Trước và Sau

### Trước (Code Comment Dài Dòng)

```csharp
/// <summary>
/// Get product by ID with PBAC (Policy-Based Access Control)
/// Policy: PRODUCT:VIEW - Basic users can only view products under configured limit
/// The actual limit is determined by JWT claims or default configuration
/// RBAC: Requires authentication at Gateway level
/// PBAC: Checks fine-grained permissions at Service level
/// Both checks must pass for the request to succeed
/// </summary>
[HttpGet("{id}")]
[Authorize(Policy = "BasicUser")] // RBAC at Gateway
public async Task<IActionResult> GetProductById(long id)
{
    // First, get the product from database
    var query = new GetProductByIdQuery { Id = id };
    var product = await _mediator.Send(query);
    
    // PBAC: Check if user can view this specific product
    var policyCheck = await _productPolicyService.CanViewProductAsync(id, product.Price);
    // ... rest
}
```

**Vấn đề**:
- ❌ Comment quá dài, che khuất code
- ❌ Giải thích lặp lại những gì code đã nói
- ❌ Khó maintain khi code thay đổi
- ❌ Developer phải đọc nhiều để hiểu

### Sau (Code Đơn Giản, Document Riêng)

```csharp
/// <summary>
/// Get product by ID with PBAC validation
/// </summary>
[HttpGet("{id}")]
[Authorize(Policy = "BasicUser")]
public async Task<IActionResult> GetProductById(long id)
{
    var query = new GetProductByIdQuery { Id = id };
    var product = await _mediator.Send(query);
    
    var policyCheck = await _productPolicyService.CanViewProductAsync(id, product.Price);
    
    if (!policyCheck.IsAllowed)
        return StatusCode(403, policyCheck.Reason);
    
    return Ok(product);
}
```

**Lợi ích**:
- ✅ Code ngắn gọn, dễ đọc
- ✅ Logic rõ ràng từ code
- ✅ Chi tiết trong document riêng (PBAC-Architecture.md, PBAC-Usage-Guide.md)
- ✅ Document có thể cập nhật độc lập
- ✅ Developer đọc code nhanh, tham khảo doc khi cần

## Tóm Tắt: Injection của PolicyEvaluator

### Cách Thức Inject

```csharp
// Program.cs
services.AddPolicyBasedAuthorization(registry => {
    registry.AddPolicy<ProductViewPolicy>("PRODUCT:VIEW");
});

// Internally:
services.AddSingleton<PolicyEvaluator>(...);
services.AddSingleton<IPolicyEvaluator>(sp => sp.GetRequiredService<PolicyEvaluator>());
```

### Sử Dụng Trong Service

```csharp
public class ProductPolicyService : IProductPolicyService
{
    private readonly IPolicyEvaluator _evaluator;  // ← Inject here
    
    public ProductPolicyService(IPolicyEvaluator evaluator, ...)
    {
        _evaluator = evaluator;
    }
    
    public async Task<PolicyEvaluationResult> CanViewProductAsync(...)
    {
        return await _evaluator.EvaluateAsync("PRODUCT:VIEW", user, context);
    }
}
```

### Tại Sao Đơn Giản?

1. **Một Điểm Đăng Ký**: `AddPolicyBasedAuthorization()` handle tất cả
2. **Singleton**: Không cần inject nhiều lần, một instance duy nhất
3. **Không Cần Config Thêm**: Extension method lo hết
4. **Type-Safe**: Compile-time checking

## Kết Luận

### Những Cải Tiến

1. **Code sạch hơn**: Loại bỏ comment dài dòng
2. **Logic rõ ràng**: Mỗi layer có trách nhiệm rõ ràng
3. **Dễ maintain**: Thay đổi policy không ảnh hưởng controller
4. **Dễ test**: Mock từng layer độc lập
5. **Document đầy đủ**: Chi tiết trong file riêng

### Khi Nào Dùng

- ✅ Cần authorization phức tạp dựa trên context
- ✅ Business rules động, thay đổi theo user attributes
- ✅ Cần kiểm tra ownership, category, price limits
- ✅ Muốn tách biệt authorization logic khỏi controller

### Khi Nào Không Cần

- ❌ Authorization đơn giản chỉ cần role
- ❌ Ứng dụng nhỏ, ít business rules
- ❌ Performance critical (PBAC có overhead)

## Tài Liệu Tham Khảo

- **Architecture**: `PBAC-Architecture.md` - Kiến trúc chi tiết, flow, design decisions
- **Usage Guide**: `PBAC-Usage-Guide.md` - Hướng dẫn sử dụng, patterns, examples
- **This Document**: Tóm tắt tiếng Việt, dễ hiểu nhanh

---

*Lưu ý: Document này giải thích "tại sao" thiết kế như vậy. Để biết "cách dùng" chi tiết, xem PBAC-Usage-Guide.md*

