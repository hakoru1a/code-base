# Keycloak Complete Guide - Setup & Configuration

## 📋 Mục Lục

1. [Tổng quan Architecture](#tổng-quan-architecture)
2. [Keycloak Setup](#keycloak-setup)
3. [Permissions & Authorization Setup](#permissions--authorization-setup)
4. [JWT Claims Structure](#jwt-claims-structure)
5. [Testing & Verification](#testing--verification)

---

## 🏗️ Tổng quan Architecture

### BFF Pattern Overview

Trong kiến trúc BFF (Backend-for-Frontend), **API Gateway đóng vai trò trung gian** giữa browser/frontend và identity provider (Keycloak).

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Browser   │◄──── Cookie ──────►│  Gateway    │◄─── OAuth 2.0 ────►│  Keycloak   │
│  (Frontend) │   (session_id)     │    (BFF)    │      (PKCE)        │    (IdP)    │
│             │                    │             │                    │             │
│  ❌ NO      │                    │  ✅ Stores  │                    │  Issues     │
│  Tokens     │                    │  - Tokens   │                    │  Tokens     │
│             │                    │  - PKCE     │                    │             │
│             │                    │  - Sessions │                    │             │
└─────────────┘                    └─────────────┘                    └─────────────┘
                                          │
                                          │ Bearer Token
                                          ▼
                                   ┌─────────────┐
                                   │ Backend APIs│
                                   │  Services   │
                                   └─────────────┘
```

### Security Benefits

1. **Tokens không bao giờ expose ra browser**
   - Access tokens, refresh tokens lưu trong Redis (backend)
   - Browser chỉ nhận HttpOnly cookie
   - Chống XSS attacks đánh cắp tokens

2. **PKCE data được quản lý ở backend**
   - `code_verifier` lưu trong Redis
   - Chống code interception attacks

3. **Session-based authentication**
   - Browser gửi session cookie
   - Gateway tự động attach Bearer token
   - Centralized session management

### ⚠️ CRITICAL: OAuth Flow PHẢI đi qua Gateway

**✅ ĐÚNG:**
```
Browser → GET /auth/login (Gateway) → Redirect to Keycloak
Keycloak → User login → Callback to /auth/signin-oidc (Gateway)
Gateway → Exchange code + verifier → Get tokens → Create session
```

**❌ SAI:**
```
Browser → Trực tiếp Keycloak authorization endpoint
         ↓ (PKCE data không tồn tại trong Redis!)
ERROR: "Invalid or expired state parameter"
```

---

## 🚀 Keycloak Setup

### 1. Start Keycloak với Docker

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:latest \
  start-dev
```

Đợi ~30s để Keycloak start up, sau đó access:
- **Admin Console**: http://localhost:8080
- **Username**: `admin`
- **Password**: `admin`

### 2. Tạo Realm

1. Login vào Admin Console
2. Click dropdown **"master"** ở góc trên bên trái
3. Click **"Create Realm"**
4. Nhập:
   - **Realm name**: `base-realm`
   - **Enabled**: ON
5. Click **"Create"**

### 3. Tạo Client cho API Gateway

#### 3.1. General Settings

1. Vào **Clients** → Click **"Create client"**
2. Điền thông tin:
   - **Client type**: `OpenID Connect`
   - **Client ID**: `api-gateway`
   - Click **"Next"**

#### 3.2. Capability Config

3. **Capability config**:
   - **Client authentication**: ✅ ON (confidential client)
   - **Authorization**: ❌ OFF
   - **Authentication flow**:
     - ✅ Standard flow (Authorization Code Flow)
     - ✅ Direct access grants (optional, for testing)
     - ❌ Implicit flow (not secure)
     - ❌ Service accounts roles
   - Click **"Next"**

#### 3.3. Login Settings

4. **Login settings**:
   - **Root URL**: `http://localhost:5238`
   - **Home URL**: `http://localhost:5238`
   - **Valid redirect URIs**: 
     ```
     http://localhost:5238/auth/signin-oidc
     http://localhost:5238/*
     ```
   - **Valid post logout redirect URIs**: 
     ```
     http://localhost:5238/*
     http://localhost:3000/*
     ```
   - **Web origins**: 
     ```
     http://localhost:5238
     http://localhost:3000
     ```
   - Click **"Save"**

#### 3.4. Advanced Settings (PKCE)

5. Vào tab **Advanced**:
   - **Proof Key for Code Exchange Code Challenge Method**: `S256` ⚠️ **REQUIRED!**
   - **Access Token Lifespan**: 5 minutes
   - **Client Session Idle**: 30 minutes
   - **Client Session Max**: 10 hours
   - Click **"Save"**

### 4. Lấy Client Secret

1. Vào **Clients** → `api-gateway`
2. Tab **Credentials**
3. Copy **Client secret**
4. Update vào `appsettings.json`:

```json
{
  "OAuth": {
    "ClientSecret": "paste-client-secret-here"
  }
}
```

⚠️ **LƯU Ý**: Không commit client secret vào Git. Dùng environment variables hoặc User Secrets.

### 5. Tạo Test Users

1. Vào **Users** → Click **"Add user"**
2. **Create user**:
   - **Username**: `testuser`
   - **Email**: `testuser@example.com`
   - **First name**: `Test`
   - **Last name**: `User`
   - **Email verified**: ✅ ON
   - **Enabled**: ✅ ON
3. Click **"Create"**

4. **Set password**:
   - Vào tab **Credentials**
   - Click **"Set password"**
   - **Password**: `Test@123`
   - **Password confirmation**: `Test@123`
   - **Temporary**: ❌ OFF
   - Click **"Save"** → Confirm

### 6. Tạo Realm Roles

1. Vào **Realm roles** → Click **"Create role"**
2. Tạo các roles cơ bản:

| Role Name | Description |
|-----------|-------------|
| `admin` | Administrator với full access |
| `manager` | Manager role |
| `product_manager` | Product management role |
| `user` | Default user role |
| `premium_user` | Premium tier user |
| `basic_user` | Basic tier user |

3. **Assign roles cho user**:
   - Vào **Users** → Select `testuser`
   - Tab **Role mapping**
   - Click **"Assign role"**
   - Filter by realm roles
   - Select `user`, `admin`
   - Click **"Assign"**

---

## 🎯 Permissions & Authorization Setup

### Kiến thức cơ bản: Khi nào dùng gì?

#### So sánh 3 cách quản lý quyền

| Tiêu chí | Realm Roles | Client Roles | User Attributes |
|----------|-------------|--------------|-----------------|
| **Phạm vi** | Toàn realm | Riêng 1 client | Theo từng user |
| **Cấu trúc** | Hierarchical | Flat | Key-value pairs |
| **Use case** | Roles chung | Roles riêng app | Permissions chi tiết |
| **Token claim** | `realm_access.roles` | `resource_access.{client}.roles` | Custom claim |

#### 1. Realm Roles - Khi nào dùng?

✅ **Dùng cho:**
- Roles chung cho toàn bộ hệ thống: `admin`, `user`, `manager`
- Cần quản lý tập trung
- SSO (Single Sign-On) - roles được share across clients
- Roles có tính hierarchical (composite roles)

📝 **Ví dụ JWT token:**
```json
{
  "realm_access": {
    "roles": ["admin", "user", "manager"]
  }
}
```

#### 2. Client Roles - Khi nào dùng?

✅ **Dùng cho:**
- Roles riêng của từng ứng dụng: `api-gateway_admin`, `mobile-app_user`
- Cần isolation giữa các clients
- Multi-tenant applications
- Microservices architecture

📝 **Ví dụ JWT token:**
```json
{
  "resource_access": {
    "api-gateway": {
      "roles": ["api-admin", "api-user"]
    }
  }
}
```

#### 3. User Attributes - Khi nào dùng?

✅ **Dùng cho:**
- **Fine-grained permissions (PBAC)**: `product:view`, `product:create`, `order:approve`
- Metadata động: `department`, `location`, `clearance_level`
- Custom data không phải roles
- Permissions phức tạp (nhiều combinations)

📝 **Ví dụ JWT token:**
```json
{
  "permissions": "product:view product:create category:view"
}
```

### Permissions trong Codebase

Xem file `src/BuildingBlocks/Shared/Identity/Permissions.cs`:

#### Category Permissions
- `category:view`
- `category:create`
- `category:update`
- `category:delete`

#### Product Permissions
- `product:view`
- `product:create`
- `product:update`
- `product:update:own`
- `product:delete`
- `product:delete:own`
- `product:approve`

#### Order Permissions
- `order:view`
- `order:view:own`
- `order:create`
- `order:update`
- `order:cancel`
- `order:approve`

#### User Permissions
- `user:view`
- `user:create`
- `user:update`
- `user:delete`
- `user:manage_roles`

### Cách Setup Permissions

#### Option 1: User Attributes (Recommended)

**Bước 1: Tạo Client Scope**

1. Vào **Client scopes** → Click **"Create client scope"**
2. **General Settings**:
   - **Name**: `permissions`
   - **Description**: `Application permissions for fine-grained access control`
   - **Protocol**: `openid-connect`
   - Click **"Save"**

**Bước 2: Tạo Protocol Mapper**

3. Vào **Client scopes** → `permissions` → Tab **Mappers**
4. Click **"Configure a new mapper"**
5. Chọn **"User Attribute"** từ danh sách
6. Điền thông tin:

| Field | Value | Giải thích |
|-------|-------|------------|
| **Name** | `permissions-mapper` | Tên mapper để nhận diện |
| **Mapper Type** | User Attribute | Tự động set |
| **User Attribute** | `permissions` | Tên attribute trên user object |
| **Token Claim Name** | `permissions` | ⚠️ **QUAN TRỌNG** - phải trùng với code |
| **Claim JSON Type** | String | Dạng chuỗi, space-separated |
| **Add to ID token** | ❌ OFF | Không cần trong ID token |
| **Add to access token** | ✅ ON | ⚠️ **BẮT BUỘC!** |
| **Add to userinfo** | ✅ ON | Nên bật |
| **Multivalued** | ❌ OFF | Single string value |

7. Click **"Save"**

**Bước 3: Gán User Attribute**

8. Vào **Users** → Chọn user → Tab **Attributes**
9. Click **"Add attribute"**:
   - **Key**: `permissions`
   - **Value**: `product:view product:create category:view`
   - (các permissions cách nhau bởi space)
10. Click **"Save"**

**Bước 4: Assign Client Scope cho Client**

11. Vào **Clients** → `api-gateway` → Tab **Client scopes**
12. Trong **Default Client Scopes**, click **"Add client scope"**
13. Chọn `permissions` → Click **"Add"**
14. Đảm bảo `permissions` nằm trong **Default** (không phải Optional)

#### Option 2: Client Roles (Alternative)

Nếu muốn quản lý permissions như roles:

1. Vào **Clients** → `api-gateway` → Tab **Roles**
2. Click **"Create role"** cho mỗi permission:
   - **Role name**: `product:view`
   - **Description**: `Permission to view products`
3. Assign roles cho user qua **Role mapping**

⚠️ **LƯU Ý**: Cách này phức tạp hơn và cần config mapper để include roles vào `scope` claim.

---

## 📊 JWT Claims Structure

### Keycloak JWT Token Example

```json
{
  "exp": 1699095600,
  "iat": 1699095300,
  "iss": "http://localhost:8080/realms/base-realm",
  "aud": "api-gateway",
  "sub": "user-uuid-123",
  "typ": "Bearer",
  "azp": "api-gateway",
  "session_state": "...",
  "acr": "1",
  "scope": "openid profile email",
  
  "preferred_username": "testuser",
  "email": "testuser@example.com",
  "email_verified": true,
  "name": "Test User",
  "given_name": "Test",
  "family_name": "User",
  
  "realm_access": {
    "roles": ["admin", "user", "manager"]
  },
  
  "resource_access": {
    "api-gateway": {
      "roles": ["api-admin"]
    }
  },
  
  "permissions": "product:view product:create category:view"
}
```

### UserClaimsContext Mapping

Code trong `ClaimsPrincipalExtensions.cs` parse JWT thành:

```csharp
public class UserClaimsContext
{
    public string UserId { get; set; }           // từ "sub"
    public List<string> Roles { get; set; }      // từ realm_access.roles + resource_access
    public Dictionary<string, string> Claims { get; set; }  // tất cả claims
    public List<string> Permissions { get; set; }  // từ "permissions" hoặc "scope"
    public Dictionary<string, object> CustomAttributes { get; set; }  // custom attrs
}
```

### Claims Extraction Flow

1. **UserId**: Lấy từ `sub`, fallback to `preferred_username`
2. **Roles**: 
   - Extract từ `ClaimTypes.Role` (đã được map)
   - Extract từ `realm_access.roles` (parse JSON)
   - Extract từ `resource_access.{client}.roles` (parse JSON)
3. **Permissions**:
   - Extract từ claim `permissions` (User Attribute)
   - Hoặc từ claim `scope` (OAuth scopes)
   - Split by space
4. **Custom Attributes**: Extract các attrs như `department`, `region`, etc.

### Code Reference

**KeycloakAuthenticationExtensions.cs** (line 348-422):
```csharp
private static void MapKeycloakRoles(ClaimsIdentity identity, KeycloakSettings settings)
{
    // Extract realm roles
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
    
    // Extract resource (client) roles
    if (settings.UseResourceRoles)
    {
        var resourceAccessClaim = identity.FindFirst("resource_access");
        // ... parse client roles
    }
    
    // Extract permissions from scope
    var scopeClaim = identity.FindFirst("scope");
    if (scopeClaim != null)
    {
        identity.AddClaim(new Claim("permissions", scopeClaim.Value));
    }
}
```

**ClaimsPrincipalExtensions.cs** (line 17-156):
```csharp
public static UserClaimsContext ToUserClaimsContext(this ClaimsPrincipal? user)
{
    var context = new UserClaimsContext
    {
        UserId = ExtractUserId(user),
        Roles = new List<string>(),
        Claims = new Dictionary<string, string>(),
        Permissions = new List<string>(),
        CustomAttributes = new Dictionary<string, object>()
    };
    
    ExtractRoles(user, context);      // realm_access + resource_access
    ExtractClaims(user, context);     // all claims + permissions
    ExtractCustomAttributes(user, context);  // department, region, etc.
    
    return context;
}
```

---

## 🧪 Testing & Verification

### 1. Test Login Flow (qua Browser)

```bash
# Bước 1: Mở browser và truy cập
http://localhost:5238/auth/login?returnUrl=http://localhost:3000/dashboard

# Bước 2: Login tại Keycloak
# Username: testuser
# Password: Test@123

# Bước 3: Sau khi login thành công, check cookie
# DevTools → Application → Cookies → session_id
```

### 2. Verify JWT Token

```bash
# Login và lấy token (Direct Password Grant - testing only)
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=api-gateway" \
  -d "client_secret=YOUR_SECRET" \
  -d "username=testuser" \
  -d "password=Test@123" \
  -d "scope=openid profile email"

# Response:
{
  "access_token": "eyJhbGc...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGc...",
  "token_type": "Bearer"
}
```

### 3. Decode Token tại https://jwt.io

Paste `access_token` và verify:

✅ **Check claims:**
- `iss` = `http://localhost:8080/realms/base-realm`
- `aud` = `api-gateway`
- `sub` = user UUID
- `preferred_username` = `testuser`
- `realm_access.roles` = `["admin", "user"]`
- `permissions` = `"product:view product:create category:view"`

### 4. Test API với Session Cookie

```bash
# Get current user info
curl http://localhost:5238/auth/user \
  -H "Cookie: session_id=YOUR_SESSION_ID"

# Response:
{
  "userId": "uuid...",
  "username": "testuser",
  "email": "testuser@example.com",
  "roles": ["user", "admin"],
  "permissions": ["product:view", "product:create", "category:view"]
}

# Test downstream API
curl http://localhost:5238/api/products \
  -H "Cookie: session_id=YOUR_SESSION_ID"

# Gateway tự động inject Bearer token
```

### 5. Verify Redis Data

```bash
# Connect to Redis
docker exec -it redis redis-cli

# List all keys
KEYS *

# Output:
# 1) "BFF_session:abc123..."
# 2) "BFF_pkce:state_xyz..."

# Get session data
GET BFF_session:abc123...

# Check TTL
TTL BFF_session:abc123...
```

### 6. Debug Permissions

Check Gateway logs:

```
[POLICY DEBUG] Policy: CanViewProducts, RequiredPermission: product:view
  User: testuser, IsAuthenticated: True
  All Claims (15): sub=user-uuid | preferred_username=testuser | ...
  
[POLICY DEBUG] Permission Claims Found: 1
  Permission Values: product:view product:create category:view
  
[POLICY DEBUG] Permission Check Result: True
  Required: product:view
  Found in Claims: YES
  
[POLICY DEBUG] Final Result for Policy 'CanViewProducts': ALLOWED
```

---

## 🔧 Troubleshooting

### ❌ Lỗi: "Invalid or expired state parameter"

**Nguyên nhân:** 
- PKCE data không tồn tại trong Redis
- User gọi trực tiếp Keycloak (bỏ qua `/auth/login`)

**Giải pháp:**
- ✅ Luôn bắt đầu từ `/auth/login`
- ✅ Complete flow trong vòng 10 phút

### ❌ Lỗi: "PKCE validation failed"

**Nguyên nhân:**
- Keycloak PKCE setting chưa enable S256

**Giải pháp:**
- Verify: Client → Advanced → PKCE = `S256`

### ❌ Permissions không có trong token

**Nguyên nhân:**
- Mapper chưa enable "Add to access token"
- User attribute chưa set
- Client scope chưa assign

**Giải pháp:**
1. Check mapper settings: `Add to access token` = ON
2. Check user attributes có key `permissions`
3. Check client scope `permissions` trong Default Client Scopes

---

## 📚 References

- [Keycloak Documentation](https://www.keycloak.org/docs/latest/server_admin/)
- [OAuth 2.0 PKCE](https://oauth.net/2/pkce/)
- [OpenID Connect](https://openid.net/connect/)
- [BFF Pattern](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps)

---

## ✅ Setup Checklist

- [ ] Keycloak đang chạy: `http://localhost:8080`
- [ ] Realm `base-realm` đã tạo
- [ ] Client `api-gateway` đã config với PKCE = S256
- [ ] Client secret đã copy vào `appsettings.json`
- [ ] Test user `testuser` đã tạo với password `Test@123`
- [ ] Realm roles đã tạo và assign cho user
- [ ] Client scope `permissions` đã tạo với mapper
- [ ] User attributes `permissions` đã gán
- [ ] Test login: `http://localhost:5238/auth/login`
- [ ] Verify token có đầy đủ claims tại https://jwt.io

---

**💡 Tip**: Export realm configuration để backup:
- **Realm settings** → **Action** → **Partial export**
- Check tất cả options → **Export**
- Lưu file JSON để import lại sau này

