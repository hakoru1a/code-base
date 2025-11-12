# Keycloak Complete Guide - Setup & Configuration

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
## 🚀 Keycloak Setup Guide

### Bước 1: Khởi động Keycloak với Docker

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:latest \
  start-dev
```

**Truy cập Keycloak Admin Console:**
- URL: http://localhost:8080
- Username: `admin`
- Password: `admin`

### Bước 2: Tạo Realm

1. Vào **Admin Console** → Click dropdown **"master"**
2. Click **"Create Realm"**
3. **Realm name**: `base-realm`
4. Click **"Create"**

### Bước 3: Tạo Client cho Auth Service

1. Vào **Clients** → Click **"Create client"**

#### General Settings
- **Client type**: `OpenID Connect`
- **Client ID**: `auth-service`
- Click **"Next"**

#### Capability Config
- **Client authentication**: ✅ ON
- **Authorization**: ❌ OFF
- **Authentication flow**:
  - ✅ Standard flow (Authorization Code Flow)
  - ✅ Direct access grants (cho testing)
  - ❌ Implicit flow
  - ❌ Service accounts roles
- Click **"Next"**

#### Login Settings
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

#### Advanced Settings (PKCE)
1. Vào tab **Advanced**
2. **Proof Key for Code Exchange Code Challenge Method**: `S256`
3. **Access Token Lifespan**: 5 minutes
4. **Refresh Token Lifespan**: 30 minutes
5. Click **"Save"**

### Bước 4: Lấy Client Secret

1. Vào **Clients** → `auth-service`
2. Tab **Credentials**
3. Copy **Client secret**

### Bước 5: Cấu hình Auth Service

**File**: `src/Services/Auth/Auth.API/appsettings.json`

```json
{
  "OAuth": {
    "Authority": "http://localhost:8080/realms/base-realm",
    "ClientId": "auth-service",
    "ClientSecret": "paste-client-secret-here",
    "RedirectUri": "http://localhost:5238/auth/signin-oidc",
    "WebAppUrl": "http://localhost:3000",
    "TokenEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/token",
    "AuthorizationEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/auth",
    "EndSessionEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/logout",
    "Scopes": ["openid", "profile", "email"],
    "UsePkce": true
  },
  "Auth": {
    "ConnectionStrings": "localhost:6379",
    "InstanceName": "Auth_",
    "SessionSlidingExpirationMinutes": 60,
    "SessionAbsoluteExpirationMinutes": 480,
    "PkceExpirationMinutes": 10,
    "RefreshTokenBeforeExpirationSeconds": 60
  }
}
```

**Bảo mật Client Secret:**
```bash
# Development - dùng User Secrets
cd src/Services/Auth/Auth.API
dotnet user-secrets set "OAuth:ClientSecret" "your-secret-here"

# Production - dùng Environment Variable
export OAuth__ClientSecret="your-secret-here"
```

### Bước 6: Tạo Test User

1. Vào **Users** → Click **"Add user"**
2. **Username**: `testuser`
3. **Email**: `testuser@example.com`
4. **First name**: `Test`
5. **Last name**: `User`
6. **Email verified**: ✅ ON
7. **Enabled**: ✅ ON
8. Click **"Create"**

#### Đặt mật khẩu:
1. Tab **Credentials** → Click **"Set password"**
2. **Password**: `Test@123`
3. **Temporary**: ❌ OFF
4. Click **"Save"**

### Bước 7: Tạo Realm Roles

1. Vào **Realm roles** → Click **"Create role"**
2. Tạo các roles:
   - `admin` - Administrator với full access
   - `user` - Default user role
   - `manager` - Manager role

#### Gán roles cho user:
1. Vào **Users** → Select `testuser`
2. Tab **Role mapping** → Click **"Assign role"**
3. Select `user`, `admin` → Click **"Assign"**

### Bước 8: Setup Permissions (Tuỳ chọn)

#### Tạo Client Scope cho Permissions:

1. Vào **Client scopes** → Click **"Create client scope"**
2. **Name**: `permissions`
3. **Protocol**: `openid-connect`
4. Click **"Save"**

#### Tạo Protocol Mapper:

1. Vào **Client scopes** → `permissions` → Tab **Mappers**
2. Click **"Configure a new mapper"** → Chọn **"User Attribute"**
3. Điền thông tin:
   - **Name**: `permissions-mapper`
   - **User Attribute**: `permissions`
   - **Token Claim Name**: `permissions`
   - **Claim JSON Type**: String
   - **Add to access token**: ✅ ON
   - **Add to userinfo**: ✅ ON
4. Click **"Save"**

#### Gán User Attribute:

1. Vào **Users** → `testuser` → Tab **Attributes**
2. **Key**: `permissions`
3. **Value**: `product:view product:create category:view`
4. Click **"Save"**

#### Assign Client Scope:

1. Vào **Clients** → `auth-service` → Tab **Client scopes**
2. Click **"Add client scope"** → Select `permissions`
3. Click **"Add" (Default)**

### Bước 9: Test Setup

#### Khởi động Services:
```bash
# Redis
docker run -d --name redis -p 6379:6379 redis:latest

# Auth Service (port 5100)
cd src/Services/Auth/Auth.API
dotnet run

# API Gateway (port 5238)
cd src/ApiGateways/ApiGateway
dotnet run
```

#### Test Login Flow:
1. Truy cập: http://localhost:5238/auth/login
2. Đăng nhập với `testuser` / `Test@123`
3. Kiểm tra cookie `session_id` được set
4. Verify Redis có session data: `docker exec -it redis redis-cli` → `KEYS Auth_*`

#### Test JWT Token:
```bash
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=auth-service" \
  -d "client_secret=YOUR_SECRET" \
  -d "username=testuser" \
  -d "password=Test@123" \
  -d "scope=openid profile email"
```

Paste `access_token` vào https://jwt.io để verify claims.

## � Troubleshooting

### ❌ "Invalid or expired state parameter"
- Auth Service chưa chạy hoặc Redis chưa kết nối
- Luôn bắt đầu từ `/auth/login`

### ❌ "PKCE validation failed"
- Check: Client → Advanced → PKCE = `S256`

### ❌ Permissions không có trong token
- Check mapper: "Add to access token" = ON
- Check user attributes có key `permissions`
- Check client scope `permissions` trong Default

---

## ✅ Checklist Hoàn thành

- [ ] Keycloak chạy tại http://localhost:8080
- [ ] Realm `base-realm` đã tạo
- [ ] Client `auth-service` đã config đúng
- [ ] Client secret đã config trong Auth Service
- [ ] Test user `testuser` đã tạo
- [ ] Realm roles và permissions đã setup
- [ ] Auth Service chạy tại http://localhost:5100
- [ ] Gateway chạy tại http://localhost:5238
- [ ] Test login thành công

