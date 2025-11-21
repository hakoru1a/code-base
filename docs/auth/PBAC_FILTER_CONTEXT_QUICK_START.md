# PBAC với Filter Context - Quick Start ⚡

> **PBAC + FilterContext** - Dynamic filtering từ JWT claims trong 60 giây

## 🎯 TL;DR - JWT Claims → Dynamic Filters

### Example: User với JWT claim `max_product_price: 20000000`

```csharp
// 1. Policy extract JWT claims → tạo FilterContext
[Policy("PRODUCT:VIEW")]
public class ProductViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, Dictionary<string, object> context)
    {
        var filterContext = new ProductFilterContext();
        
        // Extract max_product_price từ JWT claims
        if (user.Claims.TryGetValue("max_product_price", out var maxPriceStr) && 
            decimal.TryParse(maxPriceStr, out var maxPrice))
        {
            filterContext.MaxPrice = maxPrice; // 20,000,000 VND
        }
        
        return Task.FromResult(PolicyEvaluationResult.Allow("Authenticated with filters", filterContext));
    }
}

// 2. Controller sử dụng policy
[RequirePolicy("PRODUCT:VIEW")]
public async Task<IActionResult> GetProducts()
{
    return await HandleGetAllAsync<GetProductsQuery, ProductResponseDto>(new GetProductsQuery(), "Product");
}

// 3. Query Handler apply filters từ FilterContext
public async Task<List<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
{
    var query = _productRepository.FindAll();
    
    // Get filter context từ policy evaluation
    var filterContext = _httpContextAccessor.HttpContext?.GetProductFilterContext();
    if (filterContext?.MaxPrice.HasValue == true)
    {
        query = query.Where(p => p.Price <= filterContext.MaxPrice.Value); // <= 20 triệu
    }
    
    return await query.ToListAsync();
}
```

**Kết quả**: User chỉ xem được sản phẩm dưới 20 triệu VND! 🎉

---

## 📋 Workflow: JWT → Filter → Query

```
1. JWT Token: { "max_product_price": "20000000", "department": "sales" }
         ↓
2. Policy extract claims → ProductFilterContext { MaxPrice: 20M, Department: "sales" }
         ↓
3. Middleware store FilterContext vào HttpContext
         ↓
4. Query Handler apply: WHERE Price <= 20000000 AND Department = 'sales'
         ↓
5. User chỉ thấy data được filter động
```

---

## 🔧 Implementation trong 4 bước

### Step 1: Tạo Policy với FilterContext

```csharp
using Infrastructure.Authorization;
using Shared.Attributes;
using Shared.DTOs.Authorization;
using Shared.DTOs.Product;

[Policy("PRODUCT:VIEW", Description = "View products with dynamic filtering")]
public class ProductViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        if (!IsAuthenticated(user))
        {
            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated to view products"));
        }

        var filterContext = new ProductFilterContext();

        // Admin/Manager bypass all filters
        if (HasAnyRole(user, "admin", "manager"))
        {
            filterContext.CanViewAll = true;
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "Admin/Manager can view all products", filterContext));
        }

        // Extract max_product_price từ JWT claims
        if (user.Claims.TryGetValue("max_product_price", out var maxPriceStr))
        {
            if (decimal.TryParse(maxPriceStr, out var maxPrice))
            {
                filterContext.MaxPrice = maxPrice; // 20,000,000 VND
            }
        }

        // Extract department filter
        if (user.CustomAttributes.TryGetValue("department", out var department))
        {
            filterContext.DepartmentFilter = department.ToString(); // "sales"
        }

        // Extract allowed categories từ permissions
        var categoryPermissions = user.Permissions
            .Where(p => p.StartsWith("category:view:"))
            .Select(p => p.Replace("category:view:", ""))
            .ToList();
            
        if (categoryPermissions.Any())
        {
            filterContext.AllowedCategories = categoryPermissions; // ["electronics", "books"]
        }

        return Task.FromResult(PolicyEvaluationResult.Allow(
            "User authenticated with applied filters", filterContext));
    }
}
```

### Step 2: Controller với [RequirePolicy]

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [HttpGet]
    [RequirePolicy("PRODUCT:VIEW")] // ← Policy sẽ tạo FilterContext
    public async Task<IActionResult> GetProducts()
    {
        var query = new GetProductsQuery();
        var products = await _mediator.Send(query);
        
        // Optional: Log applied filters
        var filterContext = HttpContext.GetProductFilterContext();
        if (filterContext != null)
        {
            _logger.LogInformation(
                "Applied filters - MaxPrice: {MaxPrice}, Department: {Department}",
                filterContext.MaxPrice, filterContext.DepartmentFilter);
        }
        
        return Ok(products);
    }
}
```

### Step 3: Query Handler apply FilterContext

```csharp
using Infrastructure.Extensions; // For GetProductFilterContext()

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _productRepository = productRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _productRepository.FindAll().Include(p => p.Category);

        // Get filter context từ policy evaluation
        var filterContext = _httpContextAccessor.HttpContext?.GetProductFilterContext();
        if (filterContext != null)
        {
            query = ApplyPolicyFilters(query, filterContext);
        }

        var products = await query.ToListAsync(cancellationToken);
        return products.Adapt<List<ProductResponseDto>>();
    }

    private IQueryable<Product> ApplyPolicyFilters(IQueryable<Product> query, ProductFilterContext filterContext)
    {
        // Admin/Manager bypass all filters
        if (filterContext.CanViewAll)
        {
            return query;
        }

        // Apply max price filter từ JWT claims
        if (filterContext.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filterContext.MaxPrice.Value);
        }

        // Apply department filter
        if (!string.IsNullOrEmpty(filterContext.DepartmentFilter))
        {
            query = query.Where(p => p.Department == filterContext.DepartmentFilter);
        }

        // Apply category restrictions
        if (filterContext.AllowedCategories?.Any() == true)
        {
            query = query.Where(p => filterContext.AllowedCategories.Contains(p.Category.Name));
        }

        return query;
    }
}
```

### Step 4: Register dependency injection

```csharp
// Program.cs hoặc Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add HttpContextAccessor để access FilterContext
    services.AddHttpContextAccessor();
    
    // Add PBAC with auto-discovery
    services.AddPolicyBasedAuthorization(registry =>
    {
        registry.ScanAssemblies(typeof(ProductViewPolicy).Assembly);
    });
}

public void Configure(IApplicationBuilder app)
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.UsePolicyAuthorization(); // ← Middleware để store FilterContext
}
```

---

## 💡 Ví dụ JWT Claims → FilterContext

### Scenario 1: Sales Rep

**JWT Claims:**
```json
{
  "sub": "user123",
  "realm_access": { "roles": ["sales_rep"] },
  "max_product_price": "50000000",
  "department": "sales"
}
```

**FilterContext:**
```csharp
ProductFilterContext {
    MaxPrice = 50_000_000,
    DepartmentFilter = "sales",
    CanViewAll = false
}
```

**SQL Query:**
```sql
WHERE Price <= 50000000 AND Department = 'sales'
```

### Scenario 2: Premium User

**JWT Claims:**
```json
{
  "sub": "user456", 
  "realm_access": { "roles": ["premium_user"] },
  "max_product_price": "100000000",
  "permissions": "category:view:electronics category:view:books"
}
```

**FilterContext:**
```csharp
ProductFilterContext {
    MaxPrice = 100_000_000,
    AllowedCategories = ["electronics", "books"],
    CanViewCrossDepartment = true
}
```

**SQL Query:**
```sql
WHERE Price <= 100000000 AND Category IN ('electronics', 'books')
```

### Scenario 3: Manager

**JWT Claims:**
```json
{
  "sub": "admin123",
  "realm_access": { "roles": ["manager"] }
}
```

**FilterContext:**
```csharp
ProductFilterContext {
    CanViewAll = true
}
```

**SQL Query:**
```sql
-- No filters applied, see all products
SELECT * FROM Products
```

---

## 🛠️ Extension Methods

### Access FilterContext trong Controllers/Handlers:

```csharp
// Get ProductFilterContext cụ thể
var productFilter = HttpContext.GetProductFilterContext();

// Get FilterContext theo policy name
var filterContext = HttpContext.GetPolicyFilterContext<ProductFilterContext>("PRODUCT:VIEW");

// Check if có FilterContext
bool hasFilter = HttpContext.HasPolicyFilterContext("PRODUCT:VIEW");

// Get tất cả FilterContexts
var allFilters = HttpContext.GetAllPolicyFilterContexts();
```

---

## 📝 ProductFilterContext Properties

```csharp
public class ProductFilterContext : IPolicyFilterContext
{
    // Price filtering từ JWT claims
    public decimal? MaxPrice { get; set; }          // max_product_price
    public decimal? MinPrice { get; set; }          // min_product_price
    
    // Department/Region filtering
    public string? DepartmentFilter { get; set; }   // department claim
    public string? RegionFilter { get; set; }       // region claim
    
    // Category restrictions
    public List<string>? AllowedCategories { get; set; } // từ permissions
    
    // Brand restrictions  
    public List<string>? AllowedBrands { get; set; }     // brand permissions
    
    // Admin privileges
    public bool CanViewCrossDepartment { get; set; } = false;
    public bool CanViewAll { get; set; } = false;
    public bool IncludeDiscontinued { get; set; } = false;
}
```

---

## 🚀 Real-World Examples

### E-commerce với price tiers:
```json
Basic User:    { "max_product_price": "10000000" }    // 10 triệu
Premium User:  { "max_product_price": "50000000" }    // 50 triệu  
VIP User:      { "max_product_price": "200000000" }   // 200 triệu
Admin:         { "roles": ["admin"] }                 // Xem tất cả
```

### Multi-tenant với department isolation:
```json
Sales Rep:     { "department": "sales", "max_product_price": "20000000" }
Marketing Rep: { "department": "marketing", "max_product_price": "30000000" }
Manager:       { "roles": ["manager"] }  // Cross-department access
```

### Regional restrictions:
```json
North Region:  { "region": "north", "max_product_price": "15000000" }
South Region:  { "region": "south", "max_product_price": "25000000" }
National:      { "region": "national" } // All regions
```

---

## 🔍 Debugging FilterContext

### Log FilterContext trong Controller:
```csharp
[HttpGet]
[RequirePolicy("PRODUCT:VIEW")]
public async Task<IActionResult> GetProducts()
{
    var filterContext = HttpContext.GetProductFilterContext();
    
    if (filterContext != null)
    {
        _logger.LogInformation("Applied Product Filters: {@FilterContext}", filterContext);
        
        // Add debug headers
        Response.Headers.Add("X-Max-Price", filterContext.MaxPrice?.ToString() ?? "none");
        Response.Headers.Add("X-Department", filterContext.DepartmentFilter ?? "none");
        Response.Headers.Add("X-Can-View-All", filterContext.CanViewAll.ToString());
    }
    
    var result = await _mediator.Send(new GetProductsQuery());
    return Ok(result);
}
```

### Check applied SQL query:
```csharp
private IQueryable<Product> ApplyPolicyFilters(IQueryable<Product> query, ProductFilterContext filterContext)
{
    var originalQuery = query.ToString(); // Before filters
    
    // Apply filters...
    
    var finalQuery = query.ToString(); // After filters
    _logger.LogDebug("Query transformation:\nBefore: {Before}\nAfter: {After}", 
                     originalQuery, finalQuery);
    
    return query;
}
```

---

## 📚 Tạo FilterContext cho domain khác

### OrderFilterContext:
```csharp
public class OrderFilterContext : IPolicyFilterContext
{
    public decimal? MaxOrderValue { get; set; }      // từ JWT max_order_value
    public DateTime? FromDate { get; set; }          // từ JWT order_date_from  
    public DateTime? ToDate { get; set; }            // từ JWT order_date_to
    public List<string>? AllowedStatuses { get; set; } // từ permissions
    public string? CustomerRegion { get; set; }      // từ JWT region
}
```

### Usage:
```csharp
[Policy("ORDER:VIEW")]
public class OrderViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(UserClaimsContext user, Dictionary<string, object> context)
    {
        var filterContext = new OrderFilterContext();
        
        if (user.Claims.TryGetValue("max_order_value", out var maxValue))
        {
            decimal.TryParse(maxValue, out var maxOrderValue);
            filterContext.MaxOrderValue = maxOrderValue;
        }
        
        return Task.FromResult(PolicyEvaluationResult.Allow("OK", filterContext));
    }
}

// Usage in handler
var orderFilter = HttpContext.GetPolicyFilterContext<OrderFilterContext>("ORDER:VIEW");
```

---

## ✅ Best Practices

### ✅ DO:
- Extract business rules vào FilterContext thay vì hard-code
- Validate JWT claims trước khi parse (null checks, format validation)
- Log applied filters cho debugging
- Sử dụng strongly-typed FilterContext cho mỗi domain
- Handle null FilterContext gracefully

### ❌ DON'T:
- Hard-code business rules trong query handlers
- Expose internal filter logic ra client
- Ignore FilterContext (luôn check null)
- Mix authorization logic với business logic

---

## 🎯 Summary

**PBAC với FilterContext cho phép:**

✅ **Dynamic Data Filtering**: Mỗi user thấy data set khác nhau  
✅ **JWT-Driven Rules**: Business rules từ JWT claims, không hard-code  
✅ **Type Safety**: Strongly typed FilterContext  
✅ **Separation of Concerns**: Policy tạo filters, Handler apply filters  
✅ **Flexibility**: Dễ thêm filter criteria mới  

**Perfect cho:**
- E-commerce với price tiers
- Multi-tenant applications  
- Regional/departmental restrictions
- Role-based data access
- Dynamic content filtering

---

## 📚 Next Steps

1. ✅ Copy template ProductViewPolicy
2. ✅ Customize FilterContext cho domain của bạn
3. ✅ Update Query Handler để apply filters
4. ✅ Test với different JWT claims
5. ✅ Add logging để debug

---

**Tài liệu thêm:**
- [PBAC Guide](./pbac-guide.md) - Comprehensive documentation
- [JWT Claims Authorization](./jwt-claims-authorization.md) - JWT structure
- [PBAC Cheat Sheet](./pbac-cheatsheet.md) - Quick reference

**Happy filtering!** 🚀