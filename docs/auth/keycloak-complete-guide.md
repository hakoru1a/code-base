# Keycloak Complete Guide - Architecture, Setup & Configuration

## 🏗️ Tổng quan Architecture

### BFF Pattern Overview

Trong kiến trúc BFF (Backend-for-Frontend), **API Gateway đóng vai trò routing đơn giản** giữa browser/frontend và các services. **Auth Service** chịu trách nhiệm xử lý OAuth 2.0 và quản lý session.

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Browser   │◄──── Cookie ──────►│  Gateway    │◄── Session Val ───►│Auth Service │
│  (Frontend) │   (session_id)     │  (Routing)  │                    │   (OAuth)   │
│             │                    │             │                    │             │
│  ❌ NO      │                    │  ✅ Simple  │                    │  ✅ Handles │
│  Tokens     │                    │  - Routing  │                    │  - OAuth    │
│             │                    │  - RBAC     │                    │  - PKCE     │
│             │                    │  - Proxy    │                    │  - Tokens   │
│             │                    │             │                    │  - Sessions │
└─────────────┘                    └─────────────┘                    └──────┬──────┘
                                          │                                   │
                                          │ Bearer Token              OAuth 2.0 + PKCE
                                          ▼                                   ▼
                                   ┌─────────────┐                    ┌─────────────┐
                                   │ Backend APIs│                    │  Keycloak   │
                                   │  Services   │                    │    (IdP)    │
                                   │   (PBAC)    │                    └─────────────┘
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

### Bước 7: Tạo Client

**Lưu ý:** Với BFF (Backend-for-Frontend) flow, chỉ cần tạo **1 client duy nhất** cho tất cả services.

1. Menu **Clients** → Click **"Create client"**

#### Tab 1: General Settings
- **Client type**: `OpenID Connect`
- **Client ID**: `auth-client` ⚠️ **Quan trọng: Phải đúng tên này**
- Click **"Next"**

#### Tab 2: Capability config
- **Client authentication**: ✅ **ON** (quan trọng!)
- **Authorization**: ❌ OFF
- **Authentication flow**:
  - ✅ **Standard flow** (Authorization Code Flow)
  - ✅ **Direct access grants** (cho testing)
  - ❌ Implicit flow
  - ❌ Service accounts roles
- Click **"Next"**

#### Tab 3: Login settings
```
Root URL: http://localhost:5238
Home URL: http://localhost:5238
Valid redirect URIs:
  http://localhost:5238/*
  http://localhost:3000/*
Valid post logout redirect URIs:
  http://localhost:5238/*
  http://localhost:3000/*
Web origins:
  http://localhost:5238
  http://localhost:3000
```
- Click **"Save"**

#### Tab 4: Advanced Settings (PKCE)
1. Vào tab **"Advanced"**
2. Scroll xuống tìm **"Proof Key for Code Exchange Code Challenge Method"**
3. Chọn: **S256** ⚠️ **Bắt buộc cho BFF flow**
4. Click **"Save"**

### Bước 8: Lấy Client Secret

1. Vào **Clients** → Click vào `auth-client`
2. Click tab **"Credentials"**
3. Copy **Client Secret** (ví dụ: `gpdyurA7fL4MML2SOFu156KExv2P8NUJ`)

**Lưu ý:** Client Secret này sẽ được sử dụng trong tất cả services:
- Auth Service (OAuth flow)
- Gateway (JWT validation)
- Base API (JWT validation)
- Generate API (JWT validation)

### Bước 9: Cấu hình Permissions (Tùy chọn - cho PBAC)

#### 9.1. Tạo Client Scope

1. Vào **Client scopes** → Click **"Create client scope"**
2. **Name**: `permissions`
3. **Type**: `Optional`
4. **Protocol**: `OpenID Connect`
5. **Display on consent screen**: OFF
6. Click **"Save"**

#### 9.2. Tạo Protocol Mapper

1. Trong client scope `permissions` → Tab **"Mappers"**
2. Click **"Add mapper"** → **"By configuration"**
3. Chọn **"User Attribute"**
4. Điền thông tin:
   - **Name**: `permissions-mapper`
   - **User Attribute**: `permissions`
   - **Token Claim Name**: `permissions`
   - **Claim JSON Type**: `String`
   - **Add to ID token**: ✅ ON
   - **Add to access token**: ✅ ON
   - **Add to userinfo**: ✅ ON
   - **Multivalued**: ❌ OFF
5. Click **"Save"**

#### 9.3. Assign Client Scope to Client

1. Vào **Clients** → Click `auth-client`
2. Tab **"Client scopes"**
3. Click **"Add client scope"**
4. Chọn `permissions`
5. Click **"Add"** → **"Default"** (quan trọng: phải là Default, không phải Optional)

#### 9.4. Thêm Permissions cho User

1. Vào **Users** → Click `admin` (hoặc user khác)
2. Tab **"Attributes"**
3. Click **"Add an attribute"**
4. **Key**: `permissions`
5. **Value**: `product:view product:create product:update product:delete category:view category:create`
6. Click **"Save"**

### Bước 10: Cập nhật appsettings.json

Tất cả services đã được cấu hình sẵn với:
- **ClientId**: `auth-client`
- **ClientSecret**: `gpdyurA7fL4MML2SOFu156KExv2P8NUJ`

**Kiểm tra các file:**
- ✅ `src/Services/Auth/Auth.API/appsettings.json`
- ✅ `src/ApiGateways/ApiGateway/appsettings.json`
- ✅ `src/Services/Base/Base.API/appsettings.json`
- ✅ `src/Services/Generate/Generate.API/appsettings.json`

**Nếu Client Secret khác**, cập nhật trong Keycloak Admin Console → Clients → `auth-client` → Credentials → Copy secret mới.

### Bước 11: Khởi động Redis (cần thiết cho Auth Service)

```bash
docker run -d --name redis -p 6379:6379 redis:latest
```

### Bước 12: Chạy Services

```bash
# Auth Service (port 5100)
cd src/Services/Auth/Auth.API
dotnet run

# Gateway (port 5238)
cd src/ApiGateways/ApiGateway
dotnet run

# Base API (port 5239)
cd src/Services/Base/Base.API
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

### 3. Test Login Flow

**URL**: http://localhost:5100/auth/login

Hoặc qua Gateway: http://localhost:5238/auth/login

Hoặc test qua Swagger UI tại: http://localhost:5100/swagger

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

### Lỗi: "Invalid client credentials"
- Kiểm tra Client Secret trong appsettings.json khớp với Keycloak
- Đảm bảo Client authentication = ON trong Keycloak
- Kiểm tra Client ID = `auth-client` trong tất cả services

### Lỗi: "PKCE validation failed"
- Vào Client → Advanced → Proof Key for Code Exchange = S256

### Lỗi: "Invalid redirect URI"
- Kiểm tra Valid redirect URIs trong Keycloak có `http://localhost:5238/*`
- Đảm bảo RedirectUri trong AuthService config = `http://localhost:5238/auth/signin-oidc`

### Permissions không có trong token
- Kiểm tra Client Scope `permissions` đã assign vào client chưa
- Phải là **Default**, không phải **Optional**
- Kiểm tra user có attribute `permissions` chưa

### Lỗi: "Invalid audience" khi validate JWT
- Đảm bảo tất cả services dùng cùng ClientId: `auth-client`
- Kiểm tra tokens được issue bởi client `auth-client`

---

**Hoàn thành!** 🎉 Bạn đã setup xong Keycloak với BFF flow.