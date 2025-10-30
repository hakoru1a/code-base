# Keycloak Integration - Quick Start Guide

## 📋 Tổng Quan

Hệ thống này implement authentication và authorization với Keycloak theo mô hình 2 tầng:

1. **RBAC (Role-Based Access Control)** ở API Gateway
2. **PBAC (Policy-Based Access Control)** ở Service Layer

### 🎯 Ví dụ Use Case

**Scenario:** User với role `basic_user` chỉ được xem sản phẩm có giá dưới 5 triệu VND

- **Tầng 1 (Gateway - RBAC):** Kiểm tra user có role `basic_user` hay không
- **Tầng 2 (Service - PBAC):** Kiểm tra giá sản phẩm có <= 5,000,000 VND hay không

---

## 🚀 Quick Start

### Bước 1: Chạy Keycloak

```bash
# Sử dụng Docker
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:23.0 \
  start-dev

# Hoặc sử dụng Docker Compose
docker-compose -f docker-compose-keycloak.yml up -d
```

Truy cập: http://localhost:8080
- Username: `admin`
- Password: `admin`

### Bước 2: Cấu hình Keycloak

1. **Tạo Realm:**
   - Realm name: `base-realm`

2. **Tạo Clients:**
   - `api-gateway` (confidential client)
   - `base-api` (confidential client)
   - `generate-api` (confidential client)

3. **Tạo Roles:**
   - `admin` - Full access
   - `premium_user` - Xem tất cả sản phẩm
   - `basic_user` - Xem sản phẩm dưới 5M VND
   - `product_manager` - Quản lý sản phẩm

4. **Tạo Test Users:**
   - `admin.user` / `admin123` → roles: `admin`
   - `premium.user` / `premium123` → roles: `premium_user`
   - `basic.user` / `basic123` → roles: `basic_user`

> **Chi tiết setup:** Xem [02-KEYCLOAK-REALM-SETUP.md](./02-KEYCLOAK-REALM-SETUP.md)

### Bước 3: Cấu hình Services

**API Gateway (`appsettings.json`):**

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "base-realm",
    "ClientId": "api-gateway",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "RequireHttpsMetadata": false
  }
}
```

**Base.API (`appsettings.json`):**

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "base-realm",
    "ClientId": "base-api",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "RequireHttpsMetadata": false
  }
}
```

### Bước 4: Chạy Services

```bash
# API Gateway
cd ApiGateway
dotnet run

# Base API
cd Base.API
dotnet run

# Generate API
cd Generate.API
dotnet run
```

---

## 🧪 Testing

### 1. Lấy Access Token

**Sử dụng Postman hoặc cURL:**

```bash
# Basic User
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=base-api" \
  -d "client_secret=YOUR_CLIENT_SECRET" \
  -d "username=basic.user" \
  -d "password=basic123"

# Response:
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 900,
  "refresh_token": "...",
  "token_type": "Bearer"
}
```

### 2. Test RBAC (Gateway Level)

```bash
TOKEN="your_access_token_here"

# Test với basic_user - Should PASS (role check)
curl -X GET http://localhost:5000/api/v2/product \
  -H "Authorization: Bearer $TOKEN"

# Test với no token - Should FAIL 401
curl -X GET http://localhost:5000/api/v2/product
```

### 3. Test PBAC (Service Level)

```bash
# Test xem sản phẩm giá rẻ (< 5M) với basic_user
# Should PASS (role check + policy check)
curl -X GET http://localhost:5000/api/v2/product/1 \
  -H "Authorization: Bearer $TOKEN"

# Test xem sản phẩm giá cao (> 5M) với basic_user
# Should FAIL 403 (role check pass, policy check fail)
curl -X GET http://localhost:5000/api/v2/product/999 \
  -H "Authorization: Bearer $TOKEN"

# Response when denied:
{
  "error": "Forbidden",
  "message": "Product price 30,000,000 VND exceeds the limit of 5,000,000 VND for basic users",
  "policy": "PRODUCT_VIEW_PRICE"
}
```

### 4. Test với Premium User

```bash
# Get token for premium user
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -d "grant_type=password" \
  -d "client_id=base-api" \
  -d "client_secret=YOUR_CLIENT_SECRET" \
  -d "username=premium.user" \
  -d "password=premium123"

PREMIUM_TOKEN="premium_user_token_here"

# Test xem sản phẩm giá cao với premium_user
# Should PASS (premium user không có giới hạn giá)
curl -X GET http://localhost:5000/api/v2/product/999 \
  -H "Authorization: Bearer $PREMIUM_TOKEN"
```

---

## 📁 Cấu Trúc Code

```
CodeBase/
├── ApiGateway/                    # API Gateway với RBAC
│   ├── Program.cs                 # Keycloak authentication setup
│   ├── ocelot.json                # Routing configuration
│   └── appsettings.json           # Keycloak settings
│
├── Base.API/                      # Service với PBAC
│   ├── Program.cs                 # PBAC setup
│   ├── Controllers/
│   │   ├── ProductController.cs           # Original controller
│   │   └── ProductControllerWithPBAC.cs   # Enhanced with PBAC
│   └── appsettings.json
│
├── Base.Application/
│   └── Feature/Product/Policies/
│       └── ProductViewPricePolicy.cs      # PBAC policies
│
├── Infrastructure/
│   ├── Authorization/
│   │   ├── Interfaces/
│   │   │   ├── IPolicy.cs                 # Policy interface
│   │   │   └── IPolicyEvaluator.cs        # Evaluator interface
│   │   ├── BasePolicy.cs                  # Base policy class
│   │   └── PolicyEvaluator.cs             # Policy evaluator implementation
│   ├── Extentions/
│   │   ├── KeycloakAuthenticationExtensions.cs   # Keycloak JWT setup
│   │   └── PolicyAuthorizationExtensions.cs      # PBAC setup
│   └── Middlewares/
│       └── PolicyAuthorizationMiddleware.cs      # PBAC middleware
│
├── Shared/
│   ├── Configurations/
│   │   └── KeycloakSettings.cs            # Keycloak config model
│   ├── DTOs/Authorization/
│   │   └── PolicyRequirement.cs           # Policy DTOs
│   └── Attributes/
│       └── RequirePolicyAttribute.cs      # Policy attribute
│
└── docs/
    ├── README-KEYCLOAK.md                 # This file
    ├── 01-KEYCLOAK-PROCESSING-FLOW.md     # Flow documentation
    ├── 02-KEYCLOAK-REALM-SETUP.md         # Setup guide
    └── 03-SCALABILITY-AND-EXTENSIBILITY.md # Advanced guide
```

---

## 🔐 Security Layers

### Layer 1: API Gateway (RBAC)

**Mục đích:** Kiểm tra nhanh quyền truy cập dựa trên role

**Policies:**
- `AdminOnly` - Chỉ admin
- `ManagerOrAdmin` - Admin hoặc Manager
- `BasicUser` - Basic user trở lên
- `PremiumUser` - Premium user trở lên

**Example:**
```csharp
[Authorize(Policy = "BasicUser")]
public async Task<IActionResult> GetProducts()
{
    // User phải có role: basic_user, premium_user, hoặc admin
}
```

### Layer 2: Service (PBAC)

**Mục đích:** Kiểm tra chi tiết dựa trên business rules

**Policies:**
- `PRODUCT_VIEW_PRICE` - Kiểm tra giá sản phẩm
- `PRODUCT_CREATE` - Kiểm tra quyền tạo
- `PRODUCT_UPDATE` - Kiểm tra quyền sửa (ownership, department)

**Example:**
```csharp
[Authorize(Policy = "BasicUser")]  // Gateway RBAC
public async Task<IActionResult> GetProductById(long id)
{
    var product = await GetProduct(id);
    
    // Service PBAC
    var result = await _policyEvaluator.EvaluateAsync(
        "PRODUCT_VIEW_PRICE",
        userContext,
        new { ProductPrice = product.Price });
    
    if (!result.IsAllowed)
        return Forbid(result.Reason);
    
    return Ok(product);
}
```

---

## 📊 Flow Diagram

```
Client Request
      │
      ▼
┌──────────────────┐
│   API Gateway    │
│   (RBAC Check)   │  ◄─── Check: Does user have "basic_user" role?
└────────┬─────────┘
         │ ✅ PASS
         ▼
┌──────────────────┐
│   Base.API       │
│   (PBAC Check)   │  ◄─── Check: Is product price <= 5M VND?
└────────┬─────────┘
         │
    ┌────┴─────┐
    │          │
   ✅ PASS   ❌ FAIL
    │          │
    ▼          ▼
  200 OK    403 Forbidden
```

---

## 🎓 Example Scenarios

### Scenario 1: Basic User xem sản phẩm rẻ

**Setup:**
- User: `basic.user`
- Role: `basic_user`
- Product: Áo thun - 200,000 VND

**Result:** ✅ **SUCCESS**

**Flow:**
1. Gateway: ✅ Role check passed (basic_user có trong policy "BasicUser")
2. Service: ✅ Policy check passed (200,000 <= 5,000,000)

### Scenario 2: Basic User xem sản phẩm đắt

**Setup:**
- User: `basic.user`
- Role: `basic_user`
- Product: iPhone 15 Pro - 30,000,000 VND

**Result:** ❌ **FORBIDDEN (403)**

**Flow:**
1. Gateway: ✅ Role check passed
2. Service: ❌ Policy check failed (30,000,000 > 5,000,000)

**Response:**
```json
{
  "error": "Forbidden",
  "message": "Product price 30,000,000 VND exceeds the limit of 5,000,000 VND for basic users",
  "policy": "PRODUCT_VIEW_PRICE"
}
```

### Scenario 3: Premium User xem sản phẩm đắt

**Setup:**
- User: `premium.user`
- Role: `premium_user`
- Product: iPhone 15 Pro - 30,000,000 VND

**Result:** ✅ **SUCCESS**

**Flow:**
1. Gateway: ✅ Role check passed
2. Service: ✅ Policy check passed (premium user không có giới hạn)

---

## 🔧 Troubleshooting

### Lỗi: "401 Unauthorized"

**Nguyên nhân:** Token không hợp lệ hoặc hết hạn

**Giải pháp:**
1. Kiểm tra token có tồn tại trong header `Authorization: Bearer <token>`
2. Decode token tại https://jwt.io để kiểm tra expiration
3. Lấy token mới từ Keycloak

### Lỗi: "403 Forbidden" tại Gateway

**Nguyên nhân:** User không có role phù hợp

**Giải pháp:**
1. Decode token và kiểm tra `realm_access.roles`
2. Verify user có role cần thiết trong Keycloak
3. Kiểm tra policy mapping trong code

### Lỗi: "403 Forbidden" tại Service

**Nguyên nhân:** PBAC policy denied

**Giải pháp:**
1. Kiểm tra response message để xem lý do
2. Review policy logic trong code
3. Kiểm tra context data (e.g., product price)

### Lỗi: "CORS Error"

**Nguyên nhân:** Keycloak client không có web origins

**Giải pháp:**
1. Vào Keycloak → Clients → Chọn client
2. Thêm vào "Web origins": `+`

---

## 📚 Documentation

1. **[Processing Flow](./01-KEYCLOAK-PROCESSING-FLOW.md)**
   - Detailed authentication & authorization flow
   - Sequence diagrams
   - Request/response examples

2. **[Keycloak Setup](./02-KEYCLOAK-REALM-SETUP.md)**
   - Step-by-step realm configuration
   - Client setup
   - User and role management
   - Testing guide

3. **[Scalability & Extensibility](./03-SCALABILITY-AND-EXTENSIBILITY.md)**
   - Horizontal scaling strategies
   - Adding new policies
   - Performance optimization
   - Multi-tenancy support

---

## 🚀 Khả Năng Mở Rộng

### Thêm Policy Mới

**Ví dụ: Time-based access policy**

```csharp
// 1. Create policy
public class TimeBasedAccessPolicy : BasePolicy
{
    public override string PolicyName => "TIME_BASED_ACCESS";
    
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        var hour = DateTime.Now.Hour;
        if (hour >= 9 && hour < 18)
            return Allow("Business hours");
        
        return Deny("Outside business hours");
    }
}

// 2. Register in Program.cs
builder.Services.AddPolicyBasedAuthorization(policies =>
{
    policies.AddPolicy<TimeBasedAccessPolicy>();
});

// 3. Use in controller
var result = await _policyEvaluator.EvaluateAsync(
    "TIME_BASED_ACCESS", userContext, context);
```

### Thêm Custom Claims

1. Thêm attribute trong Keycloak user
2. Tạo mapper để include vào token
3. Extract trong `UserClaimsContext`

```csharp
var department = user.CustomAttributes.GetValueOrDefault("department");
```

### Scale Horizontally

```bash
# Docker Compose
docker-compose up -d --scale base-api=5

# Kubernetes
kubectl scale deployment base-api --replicas=5
```

---

## 📞 Support

Nếu có vấn đề:

1. Check logs trong `Base.API/logs/`
2. Review Keycloak admin console
3. Xem [Troubleshooting](#-troubleshooting) section
4. Đọc chi tiết trong docs folder

---

## ✅ Checklist

- [ ] Keycloak đã chạy trên port 8080
- [ ] Realm "base-realm" đã được tạo
- [ ] Clients đã được cấu hình với client secrets
- [ ] Test users đã được tạo với đúng roles
- [ ] Services có Keycloak settings trong appsettings.json
- [ ] Có thể lấy access token thành công
- [ ] RBAC hoạt động ở Gateway level
- [ ] PBAC hoạt động ở Service level
- [ ] Test scenarios pass

---

## 🎉 Kết Luận

Bạn đã có một hệ thống authentication & authorization hoàn chỉnh với:

✅ **RBAC ở Gateway** - Fast, role-based access control
✅ **PBAC ở Service** - Fine-grained, business-rule-based authorization
✅ **Flexible & Extensible** - Dễ dàng thêm policies mới
✅ **Production-ready** - Scalable, cacheable, monitorable

**Next Steps:**
1. Customize policies theo business requirements
2. Add more test users và scenarios
3. Implement caching cho performance
4. Setup monitoring và logging
5. Deploy to production

