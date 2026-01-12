# Keycloak Gateway Setup Guide - Direct JWT Authentication

## 🏗️ Tổng quan Architecture

### Gateway-Direct Pattern

Trong kiến trúc này, **API Gateway trực tiếp xử lý JWT authentication** từ Keycloak. Frontend sẽ authenticate trực tiếp với Keycloak và gửi Bearer token qua Gateway.

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Browser   │◄──── JWT Auth ────►│   Gateway   │◄───── JWT Val ────►│  Keycloak   │
│  (Frontend) │   Bearer Token     │(Auth + Route)│                    │    (IdP)    │
│             │                    │             │                    │             │
│  ✅ Handles │                    │  ✅ Handles │                    │  ✅ Issues  │
│  - Login UI │                    │  - JWT Val  │                    │  - JWT      │
│  - Tokens   │                    │  - RBAC     │                    │  - Refresh  │
│  - Refresh  │                    │  - Routing  │                    │  - UserInfo │
│             │                    │  - Proxy    │                    │             │
└─────────────┘                    └─────────────┘                    └─────────────┘
        │                                  │                                   │
        │ Direct OAuth 2.0 Flow           │ Bearer Token Forward                │
        └──────────────────────────────────┼──────────────────────────────────┘
                                          ▼
                                   ┌─────────────┐
                                   │ Backend APIs│
                                   │  Services   │
                                   │   (RBAC)    │
                                   └─────────────┘
```

## 🚀 Hướng dẫn Setup nhanh

### Bước 1: Tạo file .env

Tạo file `.env` trong thư mục `infra/` với nội dung sau:

```bash
# Keycloak Configuration
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin123
KEYCLOAK_DB=keycloak
KEYCLOAK_DB_USER=keycloak_user
KEYCLOAK_DB_PASSWORD=keycloak123
KEYCLOAK_PORT=8080
KEYCLOAK_LOG_LEVEL=INFO
```

### Bước 2: Khởi động Keycloak với Docker

```bash
# Di chuyển vào thư mục infra
cd infra/auth

# Khởi động Keycloak
docker-compose -f keycloak.yml --env-file ../.env up -d

# Xem logs
docker-compose -f keycloak.yml logs -f keycloak
```

**Đợi khoảng 1-2 phút để Keycloak khởi động hoàn tất.**

### Bước 3: Truy cập Keycloak Admin Console

- **URL**: http://localhost:8080
- **Username**: `admin`
- **Password**: `admin123`

### Bước 4: Tạo Realm

1. Click dropdown **"master"** (góc trên bên trái)
2. Click **"Create Realm"**
3. **Realm name**: `base-realm`
4. Click **"Create"**

### Bước 5: Tạo Test Users

1. Menu **Users** → Click **"Add user"**
2. Tạo admin user:
   ```
   Username: admin
   Email: admin@example.com
   First name: Admin
   Last name: User
   Email verified: ON
   ```
3. Click **"Create"**
4. Tab **Credentials** → Click **"Set password"**
   ```
   Password: admin123
   Temporary: OFF
   ```
5. Click **"Save"**

6. Lặp lại để tạo regular user:
   ```
   Username: user
   Email: user@example.com
   Password: user123
   ```

### Bước 6: Tạo Realm Roles

1. Menu **Realm roles** → Click **"Create role"**
2. Tạo các roles sau:

   **Role 1: admin**
   ```
   Role name: admin
   Description: Administrator role with full access
   ```
   
   **Role 2: manager**
   ```
   Role name: manager
   Description: Manager role with management access
   ```
   
   **Role 3: user**
   ```
   Role name: user
   Description: Regular user role with basic access
   ```

3. Assign roles cho users:
   - Menu **Users** → Click **admin** → Tab **Role mapping**
   - Click **"Assign role"** → Chọn **admin**, **manager**, **user** → Click **"Assign"**
   - Làm tương tự cho user **user**, chỉ assign role **user**

### Bước 7: Tạo Client cho Frontend & Gateway

**Lưu ý:** Với Gateway-direct flow, tạo **1 client** cho frontend authentication và Gateway sử dụng để validate JWT.

1. Menu **Clients** → Click **"Create client"**

#### Tab 1: General Settings
- **Client type**: `OpenID Connect`
- **Client ID**: `gateway` ⚠️ **Quan trọng: Phải đúng tên này**
- Click **"Next"**

#### Tab 2: Capability config
- **Client authentication**: ✅ **ON** (quan trọng cho Gateway!)
- **Authorization**: ❌ OFF
- **Authentication flow**:
  - ✅ **Standard flow** (Authorization Code Flow cho Frontend)
  - ✅ **Direct access grants** (cho testing và mobile app)
  - ❌ Implicit flow (deprecated)
  - ❌ Service accounts roles
- Click **"Next"**

#### Tab 3: Login settings
```
Root URL: http://localhost:5238
Home URL: http://localhost:5238
Valid redirect URIs:
  http://localhost:3000/*
  http://localhost:5238/auth/signin-oidc
Valid post logout redirect URIs:
  http://localhost:3000/*
  http://localhost:5238/*
Web origins:
  http://localhost:3000
  http://localhost:5238
```
- Click **"Save"**

**Lưu ý quan trọng về Redirect URIs:**
- `http://localhost:5238/auth/signin-oidc` - Callback URL của Gateway (cho server-side flow)
- `http://localhost:3000/*` - Frontend URLs (cho client-side flow)

#### Tab 4: Advanced Settings (PKCE cho Frontend Security)
1. Vào tab **"Advanced"**
2. Scroll xuống tìm **"Proof Key for Code Exchange Code Challenge Method"**
3. Chọn: **S256** ⚠️ **Bắt buộc cho frontend security**
4. Click **"Save"**

### Bước 8: Assign Client Scopes (QUAN TRỌNG!)

⚠️ **Bước này BẮT BUỘC** để scopes `openid`, `profile`, `email` hoạt động!

1. Vào **Clients** → Click vào client `gateway` (hoặc `api-gateway`)
2. Click tab **"Client scopes"**
3. Kiểm tra **"Default Client Scopes"** - phải có:
   - ✅ `openid` (bắt buộc cho OpenID Connect)
   - ✅ `profile` (cho thông tin profile: name, given_name, family_name)
   - ✅ `email` (cho email và email_verified)
   - ✅ `roles` (cho realm roles)
   - ✅ `web-origins` (cho CORS)

4. Nếu thiếu, thêm vào:
   - Scroll xuống phần **"Available client scopes"**
   - Tìm scope cần thêm (ví dụ: `profile`, `email`)
   - Click nút **"Add"** bên cạnh scope
   - Scope sẽ được chuyển sang **"Assigned default client scopes"**

5. Click **"Save"** (nếu có)

**Lưu ý:**
- `openid` scope thường đã có sẵn trong Default Client Scopes
- Nếu không thấy `profile` hoặc `email` trong Available, chúng có thể đã được assign rồi
- Các scopes trong "Default Client Scopes" sẽ tự động được include trong mọi token request

### Bước 9: Lấy Client Secret

1. Vào **Clients** → Click vào `gateway` (hoặc `api-gateway`)
2. Click tab **"Credentials"**
3. Copy **Client Secret** (ví dụ: `gpdyurA7fL4MML2SOFu156KExv2P8NUJ`)

**Lưu ý:** Client Secret này sẽ được sử dụng cho:
- Gateway (JWT validation)
- Backend APIs (JWT validation)

**Frontend KHÔNG cần Client Secret** vì sử dụng PKCE flow.

### Bước 10: Cấu hình Attribute Permissions (Mappers)

Attribute Permissions cho phép client truy cập user attributes và đưa chúng vào JWT token. Đây là bước quan trọng để sử dụng ABAC (Attribute-Based Access Control).

#### 10.1: Tạo User Attributes (nếu chưa có)

Trước khi cấu hình permissions, bạn cần thêm custom attributes cho users:

1. Menu **Users** → Click vào user cần cấu hình (ví dụ: `admin`)
2. Tab **Attributes** → Click **"Add attribute"**
3. Thêm các attributes sau:

   **Attribute 1: department**
   ```
   Key: department
   Value: Sales
   ```

   **Attribute 2: region**
   ```
   Key: region
   Value: Hanoi
   ```

   **Attribute 3: clearance_level**
   ```
   Key: clearance_level
   Value: 5
   ```

4. Click **"Save"** sau mỗi attribute

#### 10.2: Tạo Protocol Mappers để đưa Attributes vào Token

**Cách 1: Tạo Mapper trong Client (Khuyến nghị)**

Đây là cách đơn giản nhất, mappers sẽ chỉ áp dụng cho client `gateway`.

1. Menu **Clients** → Click vào client `gateway`
2. Tab **"Mappers"** → Click **"Add mapper"** → **"By configuration"** → Chọn **"User Attribute"**

   **Mapper 1: Department Attribute**
   ```
   Name: department
   User Attribute: department
   Token Claim Name: department
   Claim JSON Type: String
   Add to ID token: ON
   Add to access token: ON
   Add to userinfo: ON
   ```

   Click **"Save"**

   **Mapper 2: Region Attribute**
   ```
   Name: region
   User Attribute: region
   Token Claim Name: region
   Claim JSON Type: String
   Add to ID token: ON
   Add to access token: ON
   Add to userinfo: ON
   ```

   Click **"Save"**

   **Mapper 3: Clearance Level Attribute**
   ```
   Name: clearance_level
   User Attribute: clearance_level
   Token Claim Name: clearance_level
   Claim JSON Type: int (hoặc String)
   Add to ID token: ON
   Add to access token: ON
   Add to userinfo: ON
   ```

   Click **"Save"**

**Cách 2: Tạo Mapper trong Client Scope (Áp dụng cho tất cả clients sử dụng scope đó)**

Nếu bạn muốn mapper áp dụng cho tất cả clients sử dụng cùng một client scope (ví dụ: `profile`), tạo mapper trong client scope.

1. Menu **Client scopes** (cấp realm, không phải trong client) → Click vào scope cần dùng (ví dụ: `profile`)
2. Tab **"Mappers"** → Click **"Add mapper"** → **"By configuration"** → Chọn **"User Attribute"**
3. Cấu hình tương tự như Cách 1 (Name, User Attribute, Token Claim Name, etc.)
4. Click **"Save"**

**Lưu ý:** Nếu client scope được map vào client (trong tab "Client scopes" của client), tất cả mappers trong scope đó sẽ tự động áp dụng cho client.

#### 9.3: Kiểm tra Attributes trong Token

Sau khi cấu hình, test lại token để xem attributes có xuất hiện không:

```bash
# Lấy token với direct access grant
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=gateway" \
  -d "client_secret=gpdyurA7fL4MML2SOFu156KExv2P8NUJ" \
  -d "username=admin" \
  -d "password=admin123"

# Decode JWT token (sử dụng jwt.io hoặc decode script)
# Token sẽ chứa:
# {
#   "department": "Sales",
#   "region": "Hanoi",
#   "clearance_level": "5"
#   ...
# }
```

**Lưu ý quan trọng:**

- **Add to access token**: ✅ Bật nếu bạn muốn attributes xuất hiện trong access token (thường dùng cho API authorization)
- **Add to ID token**: ✅ Bật nếu bạn muốn attributes xuất hiện trong ID token (dùng cho client-side)
- **Add to userinfo**: ✅ Bật nếu bạn muốn attributes có thể lấy qua UserInfo endpoint
- **Claim JSON Type**: Chọn đúng kiểu dữ liệu (`String`, `int`, `boolean`, `JSON`) để đảm bảo parsing đúng

#### 10.4: Cấu hình Permissions Claim (PBAC)

Nếu bạn muốn sử dụng PBAC với permissions claim, có thể tạo mapper cho permissions:

1. Menu **Clients** → Click vào client `gateway` → Tab **"Mappers"**
2. Click **"Add mapper"** → **"By configuration"**
3. Chọn **"User Attribute"** (khuyến nghị) hoặc **"Hardcoded claim"**

   **Option 1: User Attribute Mapper (Dynamic - Khuyến nghị)**
   
   Chọn mapper type: **"User Attribute"**
   ```
   Name: permissions
   User Attribute: permissions
   Token Claim Name: permissions
   Claim JSON Type: String
   Add to ID token: ON
   Add to access token: ON
   Add to userinfo: ON
   ```
   
   Click **"Save"**
   
   Sau đó thêm attribute `permissions` cho user (xem bước 9.1) với giá trị: `product:view product:create category:view`

   **Option 2: Hardcoded Claim Mapper (Static)**
   
   Chọn mapper type: **"Hardcoded claim"**
   ```
   Name: permissions
   Token Claim Name: permissions
   Claim value: product:view product:create category:view
   Claim JSON Type: String
   Add to ID token: ON
   Add to access token: ON
   ```
   
   Click **"Save"**

**Khuyến nghị:** Sử dụng **Option 1 (User Attribute Mapper)** cho permissions để có thể thay đổi linh hoạt theo từng user. Option 2 (Hardcoded claim) chỉ nên dùng cho testing hoặc khi permissions giống nhau cho tất cả users.

### Bước 10: Cập nhật Environment Variables

Kiểm tra file `.env` trong Gateway có đúng không:

```bash
# Keycloak Settings
KEYCLOAK_AUTHORITY=http://localhost:8080
KEYCLOAK_REALM=base-realm
KEYCLOAK_CLIENTID=gateway
KEYCLOAK_CLIENTSECRET=gpdyurA7fL4MML2SOFu156KExv2P8NUJ
KEYCLOAK_VALIDATEISSUER=true
KEYCLOAK_VALIDATEAUDIENCE=true
KEYCLOAK_VALIDATELIFETIME=true
KEYCLOAK_REQUIREHTTPSMETADATA=false
KEYCLOAK_ROLECLAIMTYPE=realm_access.roles
```

### Bước 11: Khởi động Services

Chỉ cần Gateway và Backend APIs:

```bash
# Gateway (port 5238) 
cd src/ApiGateways/ApiGateway
dotnet run

# Generate API (port 5027)
cd src/Services/Generate/Generate.API
dotnet run
```

## ✅ Kiểm tra Setup

### 1. Kiểm tra Keycloak đã chạy

```bash
curl http://localhost:8080/health/ready
```

Kết quả: `{"status":"UP"}`

### 2. Kiểm tra Realm configuration

```bash
curl http://localhost:8080/realms/base-realm/.well-known/openid-configuration
```

### 3. Test JWT Token với Direct Access Grant (cho Development)

```bash
# Lấy access token
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=gateway" \
  -d "client_secret=gpdyurA7fL4MML2SOFu156KExv2P8NUJ" \
  -d "username=admin" \
  -d "password=admin123"
```

### 4. Test Gateway với JWT

```bash
# Sử dụng token từ step 3
curl -H "Authorization: Bearer <your_access_token>" \
  http://localhost:5238/api/generate/health
```

### 5. Gateway Authentication Flow (Server-side)

**Gateway** có thể xử lý OAuth flow trực tiếp cho frontend:

```bash
# Step 1: Frontend redirect user đến Gateway login
http://localhost:5238/auth/login?returnUrl=http://localhost:3000/dashboard

# Step 2: Gateway redirect đến Keycloak với PKCE
# (Gateway tự động generate state, code_verifier, code_challenge)

# Step 3: Keycloak callback về Gateway
http://localhost:5238/auth/signin-oidc?code=xxx&state=xxx

# Step 4: Gateway exchange code → tokens, tạo session, set cookie

# Step 5: Gateway redirect về frontend với session cookie
```

**Kiểm tra Gateway Auth:**
```bash
# Test login endpoint
curl -v http://localhost:5238/auth/login

# Test user info (cần session cookie)
curl -b "session_id=xxx" http://localhost:5238/auth/user

# Test logout
curl -X POST -b "session_id=xxx" http://localhost:5238/auth/logout
```

1. **Login Flow**: Redirect đến Keycloak
```javascript
const authUrl = "http://localhost:8080/realms/base-realm/protocol/openid-connect/auth?" +
  "client_id=gateway&" +
  "response_type=code&" +
  "scope=openid profile&" +
  "redirect_uri=http://localhost:3000/callback&" +
  "code_challenge_method=S256&" +
  "code_challenge=<generated_code_challenge>";

window.location.href = authUrl;
```

2. **Token Exchange**: Đổi code lấy token
```javascript
const tokenResponse = await fetch("http://localhost:8080/realms/base-realm/protocol/openid-connect/token", {
  method: "POST",
  headers: { "Content-Type": "application/x-www-form-urlencoded" },
  body: new URLSearchParams({
    grant_type: "authorization_code",
    client_id: "gateway",
    code: authorizationCode,
    redirect_uri: "http://localhost:3000/callback",
    code_verifier: codeVerifier
  })
});
```

3. **API Calls**: Gửi token qua Gateway
```javascript
const apiResponse = await fetch("http://localhost:5238/api/generate/health", {
  headers: {
    "Authorization": `Bearer ${accessToken}`,
    "Content-Type": "application/json"
  }
});
```

---

## 🔧 Troubleshooting

### Lỗi: "Connection refused" khi truy cập Keycloak
```bash
# Kiểm tra Keycloak container có chạy không
docker ps | grep keycloak

# Xem logs
docker logs codebase_keycloak

# Khởi động lại
docker restart codebase_keycloak
```

### Lỗi: "Invalid client credentials" trong Gateway
- Kiểm tra Client Secret trong `.env` file khớp với Keycloak
- Đảm bảo Client authentication = ON trong Keycloak
- Kiểm tra Client ID = `gateway` trong Gateway config

### Lỗi: "Token validation failed"
- Kiểm tra `KEYCLOAK_AUTHORITY` đúng: `http://localhost:8080`
- Kiểm tra `KEYCLOAK_REALM` đúng: `base-realm`
- Đảm bảo token được issue bởi client `auth-client`

### Lỗi: "CORS issues" từ Frontend
- Kiểm tra Web origins trong Keycloak client có `http://localhost:3000`
- Gateway cần enable CORS cho frontend domain

### Lỗi: "Invalid audience" khi validate JWT
- Đảm bảo Gateway và APIs dùng cùng ClientId: `gateway`
- Token phải được issue cho đúng audience

### Frontend không thể login
- Kiểm tra Valid redirect URIs có `http://localhost:3000/*`
- Đảm bảo PKCE code challenge method = S256
- Kiểm tra client_id trong frontend code = `gateway`

---

**Hoàn thành!** 🎉 Bạn đã setup xong Keycloak với Gateway-direct JWT authentication.