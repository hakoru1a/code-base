# 🔐 Hướng dẫn Setup Keycloak - Quick Start

## 📋 Các bước Setup nhanh

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

---

## 🔧 Tạo Clients cho hệ thống

### **Client 1: auth-client** (cho Auth.API - User Authentication)

#### **Bước 1: Tạo Client**
1. Menu **Clients** → Click **"Create client"**

#### **Tab 1: General Settings**
```
Client type: OpenID Connect
Client ID: auth-client
Name: Base Client
Description: Client for user authentication via Auth.API
```
→ Click **"Next"**

#### **Tab 2: Capability config**
```
✅ Client authentication: ON (confidential client)
❌ Authorization: OFF
✅ Authentication flow:
   ✅ Standard flow (Authorization Code + PKCE)
   ✅ Direct access grants (for testing only)
   ❌ Implicit flow (deprecated)
   ❌ Service accounts roles
```
→ Click **"Next"**

#### **Tab 3: Login settings**
```
Root URL: http://localhost:5000
Home URL: http://localhost:5000

Valid redirect URIs:
   http://localhost:5000/*
   http://localhost:5238/auth/signin-oidc
   http://localhost:3000/*

Valid post logout redirect URIs:
   http://localhost:5000/*
   http://localhost:3000/*

Web origins:
   http://localhost:5000
   http://localhost:3000
   +
```
→ Click **"Save"**

#### **Bước 2: Lấy Client Secret**
1. Tab **Credentials**
2. Copy **Client secret** → Ví dụ: `UWP2C8XceQzG6rvdKZd0yuYfTkeisLLV`

#### **Bước 3: Advanced Settings**
1. Tab **Settings** → Scroll xuống **Advanced**
2. **Proof Key for Code Exchange Code Challenge Method**: `S256`
3. Click **"Save"**

#### **Bước 4: Configure Client Scopes & Mappers**

1. Tab **Client scopes** → Click vào `auth-client-dedicated`
2. Tab **Mappers** → Add các mappers sau:

**Mapper 1: auth-client-audience**
```
Click "Add mapper" → "By configuration" → "Audience"

Name: auth-client-audience
Mapper Type: Audience
Included Client Audience: auth-client
Add to ID token: ON
Add to access token: ON
```

**Mapper 2: api-gateway-audience** (Quan trọng!)
```
Click "Add mapper" → "By configuration" → "Audience"

Name: api-gateway-audience
Mapper Type: Audience
Included Client Audience: api-gateway
Add to ID token: OFF
Add to access token: ON
```

**Mapper 3: realm-roles**
```
Click "Add mapper" → "By configuration" → "User Realm Role"

Name: realm-roles
Mapper Type: User Realm Role
Token Claim Name: realm_access.roles
Claim JSON Type: String
Add to ID token: ON
Add to access token: ON
Add to userinfo: ON
Multivalued: ON
```

**Mapper 4: username**
```
Click "Add mapper" → "By configuration" → "User Property"

Name: username
Mapper Type: User Property
Property: username
Token Claim Name: preferred_username
Claim JSON Type: String
Add to ID token: ON
Add to access token: ON
Add to userinfo: ON
```

---

### **Client 2: api-gateway** (cho API Gateway - JWT Validation)

#### **Bước 1: Tạo Client**
1. Menu **Clients** → Click **"Create client"**

#### **Tab 1: General Settings**
```
Client type: OpenID Connect
Client ID: api-gateway
Name: API Gateway
Description: Client for API Gateway JWT validation
```
→ Click **"Next"**

#### **Tab 2: Capability config**
```
✅ Client authentication: ON (confidential client)
❌ Authorization: OFF
✅ Authentication flow:
   ✅ Standard flow
   ✅ Direct access grants
   ❌ Implicit flow
   ☑️ Service accounts roles (optional)
```
→ Click **"Next"**

#### **Tab 3: Login settings**
```
Root URL: http://localhost:5238
Home URL: http://localhost:5238

Valid redirect URIs:
   http://localhost:5238/*
   http://localhost:5238/swagger/oauth2-redirect.html

Valid post logout redirect URIs:
   http://localhost:5238/*

Web origins:
   http://localhost:3000
   +
```
→ Click **"Save"**

#### **Bước 2: Lấy Client Secret**
1. Tab **Credentials**
2. Copy **Client secret** → Ví dụ: `aN8g87sIsEpS9muePv0RFlc9qa11rYxu`

#### **Bước 3: Configure Client Scopes & Mappers**

1. Tab **Client scopes** → Click vào `api-gateway-dedicated`
2. Tab **Mappers** → Add các mappers sau:

**Mapper 1: api-gateway-audience**
```
Click "Add mapper" → "By configuration" → "Audience"

Name: api-gateway-audience
Mapper Type: Audience
Included Client Audience: api-gateway
Add to ID token: ON
Add to access token: ON
```

**Mapper 2: realm-roles**
```
Click "Add mapper" → "By configuration" → "User Realm Role"

Name: realm-roles
Mapper Type: User Realm Role
Token Claim Name: realm_access.roles
Claim JSON Type: String
Add to ID token: ON
Add to access token: ON
Add to userinfo: ON
Multivalued: ON
```

#### **Bước 4: Test Client Configuration**

1. Tab **Client scopes** → Tab **Evaluate**
2. **User**: Chọn `admin`
3. Click **"Generated access token"**
4. Verify JWT token có:
   ```json
   {
     "aud": ["api-gateway", "account"],
     "realm_access": {
       "roles": ["admin", "manager", "user"]
     },
     "preferred_username": "admin"
   }
   ```

---

## ⚙️ Cấu hình Application Settings

### **1. Auth.API Configuration**

File: `src/Services/Auth/Auth.API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Auth": {
    "ConnectionStrings": "localhost:6379",
    "InstanceName": "Auth_",
    "SessionSlidingExpirationMinutes": 60,
    "SessionAbsoluteExpirationMinutes": 480,
    "PkceExpirationMinutes": 10,
    "RefreshTokenBeforeExpirationSeconds": 60
  },
  "OAuth": {
    "Authority": "http://localhost:8080/realms/base-realm",
    "ClientId": "auth-client",
    "ClientSecret": "UWP2C8XceQzG6rvdKZd0yuYfTkeisLLV",
    "RedirectUri": "http://localhost:5000/auth/signin-oidc",
    "PostLogoutRedirectUri": "/",
    "Scopes": ["openid", "profile", "email"],
    "ResponseType": "code",
    "UsePkce": true,
    "WebAppUrl": "http://localhost:3000",
    "SessionSlidingExpirationMinutes": 60,
    "SessionAbsoluteExpirationMinutes": 480,
    "TokenEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/token",
    "AuthorizationEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/auth",
    "EndSessionEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/logout",
    "RefreshTokenBeforeExpirationSeconds": 60
  }
}
```

### **2. API Gateway Configuration**

File: `src/ApiGateways/ApiGateway/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Ocelot": "Information",
      "ApiGateway": "Debug"
    }
  },
  "AllowedHosts": "*",
  
  "Services": {
    "AuthService": {
      "Url": "http://localhost:5100"
    },
    "BaseAPI": {
      "Url": "http://localhost:5239"
    },
    "GenerateAPI": {
      "Url": "http://localhost:5027"
    }
  },
  
  "OAuth": {
    "WebAppUrl": "http://localhost:3000"
  },
  
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "base-realm",
    "ClientId": "api-gateway",
    "ClientSecret": "aN8g87sIsEpS9muePv0RFlc9qa11rYxu",
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "RequireHttpsMetadata": false,
    "RoleClaimType": "realm_access.roles",
    "UseResourceRoles": true
  }
}
```

### **3. Base.API Configuration**

File: `src/Services/Base/Base.API/appsettings.json`

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080",
    "Realm": "base-realm",
    "ClientId": "base-api",
    "ClientSecret": "OzV4p8LT6u6466A2BeESVHdKEHdxtjjw",
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "RequireHttpsMetadata": false,
    "RoleClaimType": "realm_access.roles",
    "UseResourceRoles": true
  }
}
```

### **4. Docker Compose Configuration**

File: `infra/services/auth-api.yml`

```yaml
version: '3.8'

services:
  auth-api:
    image: auth-api:latest
    container_name: auth-api
    build:
      context: ../../src/Services/Auth
      dockerfile: Auth.API/Dockerfile
    ports:
      - "5100:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      # OAuth/Keycloak configuration
      - OAuth__Authority=http://keycloak:8080/realms/base-realm
      - OAuth__ClientId=auth-client
      - OAuth__ClientSecret=UWP2C8XceQzG6rvdKZd0yuYfTkeisLLV
      - OAuth__RedirectUri=http://localhost:5000/auth/signin-oidc
      - OAuth__WebAppUrl=http://localhost:3000
    depends_on:
      - redis
      - keycloak
    networks:
      - base-network

networks:
  base-network:
    external: true
```

---

## 🔍 Kiểm tra JWT Token

### Test với curl:

```bash
# 1. Get access token
curl -X POST "http://localhost:8080/realms/base-realm/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=auth-client" \
  -d "client_secret=UWP2C8XceQzG6rvdKZd0yuYfTkeisLLV" \
  -d "username=admin" \
  -d "password=admin123" \
  -d "grant_type=password" \
  -d "scope=openid profile email"

# 2. Decode token tại https://jwt.io
# Verify có các claims:
# - aud: ["auth-client", "api-gateway", "account"]
# - realm_access.roles: ["admin", "manager", "user"]
# - preferred_username: "admin"
```

### Expected JWT Token Structure:

```json
{
  "exp": 1699876543,
  "iat": 1699876243,
  "jti": "abc123...",
  "iss": "http://localhost:8080/realms/base-realm",
  "aud": [
    "auth-client",
    "api-gateway",
    "account"
  ],
  "sub": "user-uuid",
  "typ": "Bearer",
  "azp": "auth-client",
  "session_state": "session-uuid",
  "realm_access": {
    "roles": [
      "admin",
      "manager",
      "user",
      "offline_access",
      "uma_authorization"
    ]
  },
  "resource_access": {
    "account": {
      "roles": ["manage-account", "view-profile"]
    }
  },
  "scope": "openid profile email",
  "email_verified": true,
  "name": "Admin User",
  "preferred_username": "admin",
  "given_name": "Admin",
  "family_name": "User",
  "email": "admin@example.com"
}
```

---

## ✅ Checklist

### Keycloak Setup
- ✅ Keycloak running on port 8080
- ✅ Realm `base-realm` created
- ✅ Admin user `admin` created with password `admin123`
- ✅ Regular user `user` created with password `user123`
- ✅ Roles created: `admin`, `manager`, `user`
- ✅ Roles assigned to users

### Client: auth-client
- ✅ Client ID: `auth-client`
- ✅ Client Secret: `UWP2C8XceQzG6rvdKZd0yuYfTkeisLLV`
- ✅ Client authentication: ON
- ✅ Standard flow + PKCE enabled
- ✅ Valid redirect URIs configured
- ✅ Audience mapper for `auth-client` added
- ✅ Audience mapper for `api-gateway` added
- ✅ Realm roles mapper added
- ✅ Username mapper added

### Client: api-gateway
- ✅ Client ID: `api-gateway`
- ✅ Client Secret: `aN8g87sIsEpS9muePv0RFlc9qa11rYxu`
- ✅ Client authentication: ON
- ✅ Valid redirect URIs configured
- ✅ Audience mapper added
- ✅ Realm roles mapper added

### Application Configuration
- ✅ Auth.API appsettings.json updated
- ✅ API Gateway appsettings.json updated
- ✅ Docker compose auth-api.yml updated

---

## 🚀 Quick Start Commands

```bash
# 1. Start Keycloak
cd infra/auth
docker-compose -f keycloak.yml up -d

# 2. Wait 1-2 minutes, then access:
# http://localhost:8080

# 3. Follow steps above to create realm, users, roles, clients

# 4. Start Auth.API
cd src/Services/Auth/Auth.API
dotnet run

# 5. Start API Gateway
cd src/ApiGateways/ApiGateway
dotnet run

# 6. Test authentication
# Open browser: http://localhost:5000
```

---

## 🔧 Troubleshooting

### Issue: "Invalid redirect_uri"
**Solution:** Check Valid Redirect URIs in client settings includes the exact URI

### Issue: "Audience validation failed"
**Solution:** Verify audience mappers are configured correctly:
- `auth-client-dedicated` scope has `api-gateway-audience` mapper
- Token includes `aud: ["auth-client", "api-gateway"]`

### Issue: "Roles not found in token"
**Solution:** Check realm roles mapper is configured with:
- Token Claim Name: `realm_access.roles`
- Multivalued: ON
- Add to access token: ON

### Issue: "PKCE validation failed"
**Solution:** In client Advanced settings, set:
- Proof Key for Code Exchange Code Challenge Method: `S256`

