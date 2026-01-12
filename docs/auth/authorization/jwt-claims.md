# JWT Claims & Authorization Flow

---

## 🎯 JWT Token Structure

### Keycloak Access Token (Decoded)

```json
{
  "exp": 1699095600,
  "iat": 1699095300,
  "iss": "http://localhost:8080/realms/base-realm",
  "aud": "api-gateway",
  "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "typ": "Bearer",
  "azp": "api-gateway",
  "session_state": "xyz-session-state-123",
  "acr": "1",
  "scope": "openid profile email",
  
  "preferred_username": "testuser",
  "email": "testuser@example.com",
  "email_verified": true,
  "name": "Test User",
  "given_name": "Test",
  "family_name": "User",
  
  "realm_access": {
    "roles": ["admin", "user", "manager", "offline_access", "uma_authorization", "default-roles-base-realm"]
  },
  
  "resource_access": {
    "api-gateway": {
      "roles": ["product:view", "product:create", "product:update", "category:view", "category:create", "order:view"]
    },
    "account": {
      "roles": ["manage-account", "view-profile"]
    }
  },
  
  "department": "Sales",
  "region": "Hanoi",
  "clearance_level": "5"
}
```

### Standard JWT Claims

| Claim | Type | Description | Example |
|-------|------|-------------|---------|
| `iss` | String | Issuer - Keycloak realm URL | `http://localhost:8080/realms/base-realm` |
| `sub` | String | Subject - User unique ID | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| `aud` | String/Array | Audience - Target client(s) | `api-gateway`, `account` |
| `exp` | Number | Expiration time (Unix timestamp) | `1699095600` (5 minutes) |
| `iat` | Number | Issued at time | `1699095300` |
| `azp` | String | Authorized party - Client ID | `api-gateway` |
| `typ` | String | Token type | `Bearer` |
| `scope` | String | OAuth scopes | `openid profile email` |

### Keycloak Custom Claims

| Claim | Type | Description | Dùng cho |
|-------|------|-------------|----------|
| `preferred_username` | String | Username | User identification |
| `email` | String | Email address | Contact info |
| `name` | String | Full name | Display name |
| `realm_access` | Object | Realm-level roles | RBAC |
| `resource_access` | Object | Client-specific roles & permissions | RBAC + PBAC |
| `department`, `region`, etc. | String | Custom attributes | ABAC (Attribute-Based AC) |

**📝 Lưu ý:** Để cấu hình custom attributes (department, region, clearance_level, permissions) xuất hiện trong JWT token, xem hướng dẫn chi tiết tại [Keycloak Guide - Bước 9: Cấu hình Attribute Permissions](../authentication/keycloak-guide.md#bước-9-cấu-hình-attribute-permissions-mappers).

---

## 🔄 Claims Parsing Flow

### Flow Overview

```
1. Browser sends request with session cookie
         ↓
2. Gateway → SessionValidationMiddleware
         ↓
3. Gateway calls Auth Service to validate session
         ↓
4. Auth Service returns access_token from session
         ↓
5. Gateway parses JWT and validates signature
         ↓
6. KeycloakAuthenticationExtensions.MapKeycloakRoles()
   - Extract realm_access.roles → Add to ClaimTypes.Role
   - Extract resource_access.{client}.roles → Add to ClaimTypes.Role (includes permissions)
   - Extract scope → Add to "scope" claim
         ↓
7. ClaimsPrincipalExtensions.ToUserClaimsContext()
   - Extract UserId from "sub"
   - Collect all Roles (from ClaimTypes.Role) 
   - Parse Permissions (from resource_access roles with ":" format)
   - Extract CustomAttributes (department, region, etc.)
         ↓
8. UserClaimsContext Object
   {
     UserId: "...",
     Roles: ["admin", "user", ...],
     Permissions: ["product:view", "product:create", ...],
     Claims: { ... },
     CustomAttributes: { ... }
   }
         ↓
9. Authorization Check at Gateway (RBAC)
         ↓
10. Gateway injects Bearer token and forwards to Backend Services
         ↓
11. Backend Services perform additional PBAC checks
```

### Step-by-Step Code Flow

#### Step 1: JWT Bearer Authentication

**File**: `KeycloakAuthenticationExtensions.cs` (line 43-136)

```csharp
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = $"{keycloakSettings.Authority}/realms/{keycloakSettings.Realm}";
    options.Audience = keycloakSettings.ClientId;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "preferred_username",
        RoleClaimType = ClaimTypes.Role  // Map roles to standard claim type
    };
    
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                MapKeycloakRoles(identity, keycloakSettings);
            }
            return Task.CompletedTask;
        }
    };
});
```

#### Step 2: Map Keycloak Roles to Claims

**File**: `KeycloakAuthenticationExtensions.cs` (line 348-422)

```csharp
private static void MapKeycloakRoles(ClaimsIdentity identity, KeycloakSettings settings)
{
    // 1. Extract realm roles
    var realmAccessClaim = identity.FindFirst("realm_access");
    if (realmAccessClaim != null)
    {
        var realmAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            realmAccessClaim.Value);
        
        if (realmAccess != null && realmAccess.TryGetValue("roles", out var rolesElement))
        {
            var roles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }
    
    // 2. Extract resource (client) roles
    if (settings.UseResourceRoles)
    {
        var resourceAccessClaim = identity.FindFirst("resource_access");
        if (resourceAccessClaim != null)
        {
            var resourceAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                resourceAccessClaim.Value);
            
            if (resourceAccess != null && 
                resourceAccess.TryGetValue(settings.ClientId, out var clientRoles))
            {
                if (clientRoles.TryGetProperty("roles", out var rolesElement))
                {
                    var roles = JsonSerializer.Deserialize<List<string>>(
                        rolesElement.GetRawText());
                    
                    foreach (var role in roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
        }
    }
    
    // 3. Extract scope (OAuth scopes, not permissions)
    var scopeClaim = identity.FindFirst("scope");
    if (scopeClaim != null)
    {
        identity.AddClaim(new Claim("scope", scopeClaim.Value));
    }
}
```

**Input (JWT Claims):**
```json
{
  "realm_access": { "roles": ["admin", "user"] },
  "resource_access": { "api-gateway": { "roles": ["product:view", "product:create"] } },
  "scope": "openid profile email"
}
```

**Output (ClaimsIdentity):**
```csharp
Claims:
  - ClaimTypes.Role = "admin"
  - ClaimTypes.Role = "user"  
  - ClaimTypes.Role = "product:view"
  - ClaimTypes.Role = "product:create"
  - "scope" = "openid profile email"
```

#### Step 3: Extract to UserClaimsContext

**File**: `ClaimsPrincipalExtensions.cs` (line 17-156)

```csharp
public static UserClaimsContext ToUserClaimsContext(this ClaimsPrincipal? user)
{
    if (user == null || user.Identity?.IsAuthenticated != true)
    {
        return CreateAnonymousContext();
    }
    
    var context = new UserClaimsContext
    {
        UserId = ExtractUserId(user),
        Roles = new List<string>(),
        Claims = new Dictionary<string, string>(),
        Permissions = new List<string>(),
        CustomAttributes = new Dictionary<string, object>()
    };
    
    // Extract roles from multiple sources
    ExtractRoles(user, context);
    
    // Extract all claims
    ExtractClaims(user, context);
    
    // Extract custom attributes
    ExtractCustomAttributes(user, context);
    
    return context;
}

private static string ExtractUserId(ClaimsPrincipal user)
{
    return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? user.FindFirst("sub")?.Value
        ?? user.FindFirst("preferred_username")?.Value
        ?? "anonymous";
}

private static void ExtractRoles(ClaimsPrincipal user, UserClaimsContext context)
{
    // 1. Extract roles from standard claims (đã được map ở Step 2)
    var standardRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
    context.Roles.AddRange(standardRoles);
    
    // 2. Extract roles from Keycloak realm_access (backup nếu chưa map)
    ExtractRealmRoles(user, context);
    
    // 3. Extract resource_access roles (backup nếu chưa map)
    ExtractResourceRoles(user, context);
    
    // Remove duplicates
    context.Roles = context.Roles.Distinct().ToList();
}

private static void ExtractClaims(ClaimsPrincipal user, UserClaimsContext context)
{
    foreach (var claim in user.Claims)
    {
        if (!context.Claims.ContainsKey(claim.Type))
        {
            context.Claims[claim.Type] = claim.Value;
        }
        
        // Extract permissions from roles (resource_access format: "permission:action")
        if (claim.Type == ClaimTypes.Role && claim.Value.Contains(':'))
        {
            context.Permissions.Add(claim.Value);
        }
    }
    
    // Remove duplicate permissions
    context.Permissions = context.Permissions.Distinct().ToList();
}

private static void ExtractCustomAttributes(ClaimsPrincipal user, UserClaimsContext context)
{
    // Extract common custom attributes
    ExtractStringAttribute(user, context, "department");
    ExtractStringAttribute(user, context, "location");
    ExtractStringAttribute(user, context, "region");
    ExtractStringAttribute(user, context, "team");
    
    // Extract integer attributes
    ExtractIntAttribute(user, context, "clearance_level");
}
```

**Result (UserClaimsContext):**
```csharp
{
    UserId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    Roles = ["admin", "user"],  // Only realm/traditional roles
    Permissions = ["product:view", "product:create", "category:view"],  // From resource_access roles
    Claims = {
        "sub" = "a1b2c3d4...",
        "preferred_username" = "testuser", 
        "email" = "testuser@example.com",
        ...
    },
    CustomAttributes = {
        "department" = "Sales",
        "region" = "Hanoi",
        "clearance_level" = 5
    }
}
```

---

## 🔐 RBAC (Role-Based Access Control)

### Gateway Level - Coarse-Grained Authorization

**Use Case:** Kiểm soát truy cập vào controllers/endpoints dựa trên roles.

### Role Definitions

**File**: `Shared/Identity/Roles.cs`

```csharp
public static class Roles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string ProductManager = "product_manager";
    public const string User = "user";
    public const string PremiumUser = "premium_user";
    public const string BasicUser = "basic_user";
}
```

### RBAC Policies

**File**: `KeycloakAuthenticationExtensions.cs` (line 149-175)

```csharp
services.AddAuthorization(options =>
{
    // Admin only
    options.AddPolicy(PolicyNames.Rbac.AdminOnly, policy =>
        policy.RequireRole(Roles.Admin));
    
    // Manager or Admin
    options.AddPolicy(PolicyNames.Rbac.ManagerOrAdmin, policy =>
        policy.RequireRole(
            Roles.Admin,
            Roles.Manager,
            Roles.ProductManager));
    
    // Any authenticated user
    options.AddPolicy(PolicyNames.Rbac.AuthenticatedUser, policy =>
        policy.RequireAuthenticatedUser());
    
    // Premium users
    options.AddPolicy(PolicyNames.Rbac.PremiumUser, policy =>
        policy.RequireRole(
            Roles.PremiumUser,
            Roles.Admin));
});
```

### Usage Example

```csharp
// Gateway Controller
[Authorize(Policy = PolicyNames.Rbac.AdminOnly)]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        // Chỉ admin có thể access
        return Ok(users);
    }
}

[Authorize(Policy = PolicyNames.Rbac.ManagerOrAdmin)]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // Admin, Manager, ProductManager có thể access
        return Ok(products);
    }
}
```

### How RBAC Works

1. **Request arrives** with JWT token
2. **Middleware validates** JWT signature
3. **MapKeycloakRoles()** extracts roles → adds to `ClaimTypes.Role`
4. **[Authorize] attribute** checks policy
5. **Policy requirement**: `RequireRole("admin")`
6. **Authorization handler** checks: `user.IsInRole("admin")`
7. **Result**: `200 OK` or `403 Forbidden`

---

## 🎯 PBAC (Permission-Based/Policy-Based Access Control)

### Service Level - Fine-Grained Authorization

**Use Case:** Kiểm soát truy cập chi tiết dựa trên permissions và business logic.

### Permission Definitions

**File**: `Shared/Identity/Permissions.cs`

```csharp
public static class Permissions
{
    public static class Product
    {
        public const string View = "product:view";
        public const string Create = "product:create";
        public const string Update = "product:update";
        public const string UpdateOwn = "product:update:own";
        public const string Delete = "product:delete";
        public const string DeleteOwn = "product:delete:own";
        public const string Approve = "product:approve";
    }
    
    public static class Category
    {
        public const string View = "category:view";
        public const string Create = "category:create";
        public const string Update = "category:update";
        public const string Delete = "category:delete";
    }
}
```

### PBAC Policy Example

**File**: `Base.Application/Feature/Product/Policies/ProductListFilterPolicy.cs`

```csharp
public class ProductListFilterPolicy : IPolicyHandler<ProductListFilterContext>
{
    public Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext userContext,
        ProductListFilterContext policyContext)
    {
        // Admin và PremiumUser: không giới hạn
        if (userContext.Roles.Contains(Roles.Admin) ||
            userContext.Roles.Contains(Roles.PremiumUser))
        {
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "Admin/Premium users have unlimited access"));
        }
        
        // BasicUser: giới hạn products < 5,000,000 VND
        if (userContext.Roles.Contains(Roles.BasicUser))
        {
            policyContext.MaxPrice = 5_000_000;
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "Basic users can view products under 5M VND"));
        }
        
        // Authenticated users: chỉ xem products trong department của họ
        if (userContext.IsAuthenticated &&
            userContext.CustomAttributes.TryGetValue("department", out var dept))
        {
            policyContext.AllowedCategories = new List<string> { dept.ToString() };
            return Task.FromResult(PolicyEvaluationResult.Allow(
                $"User can view products in {dept} department"));
        }
        
        return Task.FromResult(PolicyEvaluationResult.Deny(
            "User does not meet any filter criteria"));
    }
}
```

### PBAC Usage Example

```csharp
public class ProductService : IProductService
{
    private readonly IProductPolicyService _policyService;
    
    public async Task<List<ProductDto>> GetAllAsync()
    {
        var userContext = _httpContextAccessor.HttpContext?.User
            .ToUserClaimsContext();
        
        // Evaluate policy
        var filterContext = await _policyService
            .EvaluateProductListFilterAsync(userContext);
        
        // Apply filter to query
        var query = _dbContext.Products.AsQueryable();
        
        if (filterContext.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filterContext.MaxPrice.Value);
        }
        
        if (filterContext.AllowedCategories?.Any() == true)
        {
            query = query.Where(p => filterContext.AllowedCategories
                .Contains(p.Category));
        }
        
        return await query.ToListAsync();
    }
}
```

### How PBAC Works

1. **Request arrives** at service endpoint
2. **Extract UserClaimsContext** from `ClaimsPrincipal`
3. **Build PolicyContext** with request data (ProductId, Price, etc.)
4. **Call Policy Handler**: `EvaluateAsync(userContext, policyContext)`
5. **Policy logic** checks:
   - Roles (`userContext.Roles`)
   - Permissions (`userContext.Permissions`)
   - Custom attributes (`userContext.CustomAttributes`)
   - Context data (`policyContext`)
6. **Return result**: `PolicyEvaluationResult.Allow()` or `Deny()`
7. **Service applies filter** or returns `403 Forbidden`

---

## 🔀 Hybrid Authorization

### Combining RBAC + PBAC

**File**: `KeycloakAuthenticationExtensions.cs` (line 259-343)

```csharp
private static void AddHybridPolicy(
    AuthorizationOptions options,
    string policyName,
    string? requiredPermission = null,
    params string[] allowedRoles)
{
    options.AddPolicy(policyName, policy =>
        policy.RequireAssertion(context =>
        {
            // Check permission (PBAC) - from resource_access roles
            bool hasPermission = false;
            if (!string.IsNullOrEmpty(requiredPermission))
            {
                hasPermission = context.User.HasClaim(c =>
                    c.Type == ClaimTypes.Role &&
                    c.Value.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase));
            }
            
            // Check role (RBAC)
            bool hasRole = allowedRoles.Length > 0 &&
                allowedRoles.Any(role => context.User.IsInRole(role));
            
            // Grant access if has permission OR has role
            return hasPermission || hasRole;
        }));
}

// Usage:
AddHybridPolicy(
    options,
    PolicyNames.Hybrid.Product.CanView,
    Permissions.Product.View,  // permission
    Roles.Admin,               // OR roles
    Roles.Manager);
```

### Hybrid Policy Definitions

**File**: `Shared/Identity/PolicyNames.cs`

```csharp
public static class PolicyNames
{
    public static class Hybrid
    {
        public static class Product
        {
            public const string CanView = "Hybrid.Product.CanView";
            public const string CanCreate = "Hybrid.Product.CanCreate";
            public const string CanUpdate = "Hybrid.Product.CanUpdate";
            public const string CanDelete = "Hybrid.Product.CanDelete";
        }
        
        public static class Category
        {
            public const string CanView = "Hybrid.Category.CanView";
            public const string CanCreate = "Hybrid.Category.CanCreate";
        }
    }
}
```

### Hybrid Authorization Logic

```
User can access if:
    (has permission "product:view")  
    OR  
    (has role "admin" OR "manager")

Example 1: User with permission "product:view" but no roles → ALLOWED
Example 2: User with role "admin" but no permissions → ALLOWED
Example 3: User with permission "product:create" (not view) and no roles → DENIED
```

### Usage Example

```csharp
[Authorize(Policy = PolicyNames.Hybrid.Product.CanView)]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // User can access if:
        // - Has permission "product:view" OR
        // - Has role "admin" OR "manager"
        return Ok(products);
    }
}
```

---

## 💻 Code Examples

### Example 1: Simple RBAC Check

```csharp
[Authorize(Policy = PolicyNames.Rbac.AdminOnly)]
[HttpDelete("products/{id}")]
public async Task<IActionResult> DeleteProduct(long id)
{
    // Only admin can delete
    await _productService.DeleteAsync(id);
    return NoContent();
}
```

### Example 2: Hybrid Authorization

```csharp
[Authorize(Policy = PolicyNames.Hybrid.Product.CanUpdate)]
[HttpPut("products/{id}")]
public async Task<IActionResult> UpdateProduct(long id, ProductUpdateDto dto)
{
    // Can update if:
    // - Has permission "product:update" OR
    // - Has role "admin" or "product_manager"
    await _productService.UpdateAsync(id, dto);
    return Ok();
}
```

### Example 3: PBAC with Custom Logic

```csharp
[Authorize]
[HttpGet("products")]
public async Task<IActionResult> GetProducts()
{
    var userContext = User.ToUserClaimsContext();
    
    // Evaluate policy
    var filterContext = await _policyService
        .EvaluateProductListFilterAsync(userContext);
    
    // Apply filter
    var products = await _productService
        .GetFilteredProductsAsync(filterContext);
    
    return Ok(products);
}
```

### Example 4: Check Permission in Code

```csharp
[Authorize]
[HttpPost("products")]
public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
{
    var userContext = User.ToUserClaimsContext();
    
    // Check permission explicitly
    if (!userContext.Permissions.Contains(Permissions.Product.Create) &&
        !userContext.Roles.Contains(Roles.Admin))
    {
        return Forbid("You don't have permission to create products");
    }
    
    await _productService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
}
```

### Example 5: Attribute-Based Access Control (ABAC)

```csharp
[Authorize]
[HttpGet("products/{id}")]
public async Task<IActionResult> GetProduct(long id)
{
    var userContext = User.ToUserClaimsContext();
    var product = await _productService.GetByIdAsync(id);
    
    // Check if user's department matches product category
    if (userContext.CustomAttributes.TryGetValue("department", out var dept))
    {
        if (product.Category != dept.ToString() && 
            !userContext.Roles.Contains(Roles.Admin))
        {
            return Forbid("You can only view products in your department");
        }
    }
    
    return Ok(product);
}
```

---

## 🔍 Debugging Authorization

### Log Claims

```csharp
var claims = User.Claims.Select(c => $"{c.Type} = {c.Value}");
_logger.LogDebug("User Claims: {Claims}", string.Join(", ", claims));

// Output:
// sub = a1b2c3d4-..., 
// preferred_username = testuser,
// http://schemas.microsoft.com/ws/2008/06/identity/claims/role = admin,
// http://schemas.microsoft.com/ws/2008/06/identity/claims/role = user,
// http://schemas.microsoft.com/ws/2008/06/identity/claims/role = product:view,
// http://schemas.microsoft.com/ws/2008/06/identity/claims/role = product:create
```

### Log UserClaimsContext

```csharp
var userContext = User.ToUserClaimsContext();
_logger.LogDebug("UserContext: UserId={UserId}, Roles={Roles}, Permissions={Permissions}",
    userContext.UserId,
    string.Join(",", userContext.Roles),
    string.Join(",", userContext.Permissions));

// Output:
// UserContext: UserId=a1b2c3d4-..., 
// Roles=admin,user, 
// Permissions=product:view,product:create,category:view
```

### Log Policy Evaluation

**Automatic logs từ `AddHybridPolicy()`:**

```
[POLICY DEBUG] Policy: CanViewProducts, RequiredPermission: product:view
  User: testuser, IsAuthenticated: True
  All Claims (15): sub=a1b2c3d4 | preferred_username=testuser | ...

[POLICY DEBUG] Permission Claims Found: 3
  Permission Values: product:view, product:create, category:view

[POLICY DEBUG] Permission Check Result: True
  Required: product:view
  Found in Role Claims: YES

[POLICY DEBUG] Final Result for Policy 'CanViewProducts': ALLOWED
  HasPermission: True, HasRole: False
```

---

## 📊 Summary

### Claims Flow

```
Browser Request (session cookie)
    ↓
Gateway SessionValidationMiddleware
    ↓
Auth Service validates session → returns access_token
    ↓
Gateway parses JWT Token
    ↓
JwtBearerAuthentication validates signature
    ↓
MapKeycloakRoles() extracts:
    - realm_access.roles → ClaimTypes.Role (traditional roles)
    - resource_access.{client}.roles → ClaimTypes.Role (permissions as roles)
    - scope → "scope" claim (OAuth scopes)
    ↓
ClaimsPrincipal with all claims
    ↓
ToUserClaimsContext() creates:
    - UserClaimsContext { UserId, Roles, Permissions, ... }
    ↓
Authorization Check at Gateway (RBAC):
    - Check Roles with [Authorize] policies
    ↓
Gateway injects Bearer token → forwards to Backend Service
    ↓
Backend Service Authorization (PBAC):
    - Check Permissions + Business Logic
    - Apply fine-grained access control
    ↓
Result: Allow (200 OK) or Deny (403 Forbidden)
```

### When to Use What?

| Scenario | Use | Example |
|----------|-----|---------|
| Kiểm soát truy cập endpoint | **RBAC** | `[Authorize(Policy = "AdminOnly")]` |
| Kiểm soát chi tiết dựa trên business logic | **PBAC** | Filter products by price/category |
| Kết hợp flexibility | **Hybrid** | Permission OR Role |
| Dựa trên user attributes | **ABAC** | Check department, region, etc. |

---

**Tip**: Luôn log claims khi debug authorization issues. Sử dụng `User.ToUserClaimsContext()` để dễ dàng access roles và permissions.

