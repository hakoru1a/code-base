# Hướng dẫn Setup Keycloak cho BFF Pattern

## 📋 Mục tiêu

Setup Keycloak Identity Provider để hỗ trợ OAuth 2.0 Authorization Code Flow + PKCE cho API Gateway với Backend-for-Frontend (BFF) Pattern.

## 🏗️ BFF Architecture Overview

### Tại sao cần BFF Pattern?

Trong kiến trúc BFF, **API Gateway đóng vai trò trung gian** giữa browser/frontend và identity provider (Keycloak). Điều này mang lại nhiều lợi ích về security:

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
                                          │
                                          ▼
                                   ┌─────────────┐
                                   │ Backend APIs│
                                   │  Services   │
                                   └─────────────┘
```

### Security Benefits

1. **Tokens không bao giờ expose ra browser**
   - Access tokens, refresh tokens lưu trong Redis (backend)
   - Browser chỉ nhận HttpOnly cookie (không thể access từ JavaScript)
   - Không rủi ro XSS attacks đánh cắp tokens

2. **PKCE data được quản lý ở backend**
   - `code_verifier` lưu trong Redis, không gửi lên browser
   - `code_challenge` được generate và gửi tới Keycloak
   - Chống code interception attacks

3. **Session-based authentication**
   - Browser gửi session cookie (simple, secure)
   - Gateway tự động attach Bearer token khi forward requests
   - Centralized session management (logout, revoke, ...)

### ⚠️ CRITICAL: OAuth Flow PHẢI đi qua Gateway

**✅ ĐÚNG:**
```
Browser → GET /auth/login (Gateway) → Redirect to Keycloak
         ↓ (Gateway tạo và lưu PKCE vào Redis)
Keycloak → User login → Callback to /auth/signin-oidc (Gateway)
         ↓ (Gateway lấy PKCE từ Redis)
Gateway → Exchange code + verifier → Get tokens → Create session
```

**❌ SAI:**
```
Browser → Trực tiếp Keycloak authorization endpoint
         ↓ (PKCE data không tồn tại trong Redis!)
Keycloak → Callback to /auth/signin-oidc (Gateway)
         ↓ (Gateway không tìm thấy PKCE data)
ERROR: "Invalid or expired state parameter"
```

### Key Differences vs Traditional OAuth

| Aspect | Traditional OAuth (SPA) | BFF Pattern |
|--------|------------------------|-------------|
| Token storage | LocalStorage/SessionStorage (browser) | Redis (backend) |
| Token visibility | JavaScript có thể access | Không expose ra browser |
| Authentication | Bearer token gửi từ browser | HttpOnly cookie + Gateway inject token |
| PKCE verifier | Stored in browser memory | Stored in Redis |
| Security | Dễ bị XSS attacks | Protected from XSS |
| Session management | Client-side | Server-side (centralized) |

## 🚀 Quick Start với Docker

### 1. Start Keycloak Container

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

## 🔧 Configuration Steps

### Step 1: Tạo Realm

1. Login vào Admin Console
2. Click dropdown **"master"** ở góc trên bên trái
3. Click **"Create Realm"**
4. Nhập:
   - **Realm name**: `base-realm`
   - **Enabled**: ON
5. Click **"Create"**

### Step 2: Tạo Client cho API Gateway

1. Vào **Clients** → Click **"Create client"**
2. **General Settings**:
   - **Client type**: `OpenID Connect`
   - **Client ID**: `api-gateway`
   - Click **"Next"**

3. **Capability config**:
   - **Client authentication**: ON (confidential client)
   - **Authorization**: OFF
   - **Authentication flow**:
     - ✅ Standard flow (Authorization Code Flow)
     - ✅ Direct access grants (optional, for testing)
     - ❌ Implicit flow (not secure)
     - ❌ Service accounts roles
   - Click **"Next"**

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

### Step 3: Configure Client Settings

1. Vào **Clients** → `api-gateway`
2. Tab **Settings**:
   - **Access settings**:
     - **Root URL**: `http://localhost:5238`
     - **Valid redirect URIs**: `http://localhost:5238/*`
     - **Valid post logout redirect URIs**: `http://localhost:5238/*`, `http://localhost:3000/*`
     - **Web origins**: `http://localhost:5238`, `http://localhost:3000`
   
   - **Capability config**:
     - **Client authentication**: ON
     - **Authorization**: OFF
     - **Standard flow**: ON
     - **Direct access grants**: ON (optional)
     - **Implicit flow**: OFF
     - **OAuth 2.0 Device Authorization Grant**: OFF
     - **OIDC CIBA Grant**: OFF
   
   - **Login settings**:
     - **Login theme**: keycloak (default)
     - **Consent required**: OFF
     - **Display client on consent screen**: ON
     - **Client consent screen text**: (empty)

3. Tab **Advanced**:
   - **Proof Key for Code Exchange Code Challenge Method**: `S256` (REQUIRED!)
   - **Access Token Lifespan**: 5 minutes (default)
   - **Client Session Idle**: 30 minutes
   - **Client Session Max**: 10 hours
   - **Client Offline Session Idle**: 30 days

4. Click **"Save"**

### Step 4: Lấy Client Secret

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

⚠️ **LƯU Ý**: Không commit client secret vào Git. Dùng environment variables hoặc User Secrets trong production.

### Step 5: Tạo Test Users

1. Vào **Users** → Click **"Add user"**
2. **Create user**:
   - **Username**: `testuser`
   - **Email**: `testuser@example.com`
   - **First name**: `Test`
   - **Last name**: `User`
   - **Email verified**: ON
   - **Enabled**: ON
3. Click **"Create"**

4. **Set password**:
   - Vào tab **Credentials**
   - Click **"Set password"**
   - **Password**: `Test@123`
   - **Password confirmation**: `Test@123`
   - **Temporary**: OFF (không bắt đổi password lần đầu)
   - Click **"Save"**
   - Confirm **"Save password"**

### Step 6: Tạo Roles (Optional)

1. Vào **Realm roles** → Click **"Create role"**
2. Tạo các roles:
   - `user` (default role)
   - `admin`
   - `manager`
   - `premium_user`

3. **Assign role cho user**:
   - Vào **Users** → Select `testuser`
   - Tab **Role mapping**
   - Click **"Assign role"**
   - Filter by realm roles
   - Select `user`, `admin`
   - Click **"Assign"**

### Step 7: Tạo Admin User (Optional)

Repeat Step 5 với:
- **Username**: `admin`
- **Email**: `admin@example.com`
- **Password**: `Admin@123`
- **Roles**: `admin`, `user`

## 🔍 Verify Setup

### Keycloak Endpoints Reference

Keycloak cung cấp các endpoints sau (với realm `base-realm`):

```
# OpenID Configuration (metadata)
http://localhost:8080/realms/base-realm/.well-known/openid-configuration

# Authorization Endpoint (Gateway sẽ gọi, KHÔNG gọi trực tiếp)
http://localhost:8080/realms/base-realm/protocol/openid-connect/auth

# Token Endpoint (Gateway sẽ gọi)
http://localhost:8080/realms/base-realm/protocol/openid-connect/token

# Userinfo Endpoint
http://localhost:8080/realms/base-realm/protocol/openid-connect/userinfo

# Logout Endpoint (Gateway sẽ gọi)
http://localhost:8080/realms/base-realm/protocol/openid-connect/logout

# Token Revocation (Gateway sẽ gọi)
http://localhost:8080/realms/base-realm/protocol/openid-connect/revoke
```

⚠️ **QUAN TRỌNG**: Trong BFF pattern, **KHÔNG BAO GIỜ** gọi trực tiếp các Keycloak endpoints từ browser/frontend. 
Tất cả OAuth flow phải đi qua API Gateway để Gateway quản lý PKCE và tokens.

### Test Keycloak Configuration

#### 1. Verify OpenID Configuration

```bash
# Kiểm tra Keycloak configuration
curl http://localhost:8080/realms/base-realm/.well-known/openid-configuration | jq

# Kết quả phải chứa:
# - "authorization_endpoint"
# - "token_endpoint" 
# - "code_challenge_methods_supported": ["S256", "plain"]
```

#### 2. Verify Client Configuration (Optional)

Nếu muốn test trực tiếp Keycloak Token Endpoint với **Direct Access Grant** (chỉ dùng để debug):

```bash
# Direct Password Grant (testing only, không dùng trong production)
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=api-gateway" \
  -d "client_secret=YOUR_CLIENT_SECRET" \
  -d "username=testuser" \
  -d "password=Test@123" \
  -d "scope=openid profile email"

# Response:
{
  "access_token": "eyJhbGc...",
  "expires_in": 300,
  "refresh_expires_in": 1800,
  "refresh_token": "eyJhbGc...",
  "token_type": "Bearer",
  "id_token": "eyJhbGc...",
  "session_state": "..."
}
```

⚠️ **LƯU Ý**: Direct Password Grant chỉ dùng để test Keycloak configuration. Trong production, luôn dùng Authorization Code Flow + PKCE qua Gateway.

#### 3. Decode JWT Token (để debug)

Copy `access_token` từ bước 2 và paste vào https://jwt.io để xem claims:

```json
{
  "exp": 1699095600,
  "iat": 1699095300,
  "iss": "http://localhost:8080/realms/base-realm",
  "sub": "user-uuid",
  "preferred_username": "testuser",
  "email": "testuser@example.com",
  "email_verified": true,
  "realm_access": {
    "roles": ["user", "admin"]
  },
  "scope": "openid profile email"
}
```

Verify rằng:
- `iss` (issuer) đúng với Keycloak realm URL
- `preferred_username`, `email` đúng với user
- `realm_access.roles` chứa các roles đã assign
- `exp` (expiration) hợp lệ (5 minutes từ `iat`)

## 🎨 Keycloak Themes (Optional)

Để customize login page, tạo custom theme:

1. Vào **Realm settings** → **Themes** tab
2. **Login theme**: keycloak (hoặc custom theme nếu có)
3. **Account theme**: keycloak.v3
4. **Email theme**: keycloak

## 🔐 Security Best Practices

### 1. Production Settings

**Realm settings** → **Security defenses**:
- **Headers**: 
  - X-Frame-Options: SAMEORIGIN
  - Content-Security-Policy: frame-src 'self'; frame-ancestors 'self'
  - X-Content-Type-Options: nosniff
- **Brute Force Detection**: ON
  - Max login failures: 5
  - Wait increment: 60 seconds
  - Max wait: 900 seconds (15 minutes)

### 2. Token Lifespan

**Realm settings** → **Tokens** tab:
- **Access Token Lifespan**: 5 minutes
- **Access Token Lifespan For Implicit Flow**: 15 minutes
- **Client Login Timeout**: 1 minute
- **Login Timeout**: 5 minutes
- **Login Action Timeout**: 5 minutes

### 3. Client Settings

**Clients** → `api-gateway` → **Advanced**:
- **PKCE**: S256 (REQUIRED)
- **Access Token Lifespan**: 5 minutes
- **Client Session Idle**: 30 minutes
- **Client Session Max**: 10 hours

### 4. Password Policy

**Realm settings** → **Authentication** → **Policies** tab:
- Minimum length: 8
- Not username: ON
- Special chars: 1
- Uppercase chars: 1
- Lowercase chars: 1
- Digits: 1
- Not recently used: 3

## 🐳 Docker Compose Setup

Tạo `docker-compose.yml` để chạy Keycloak + Redis:

```yaml
version: '3.8'

services:
  keycloak:
    image: quay.io/keycloak/keycloak:latest
    container_name: keycloak
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: keycloak
    command:
      - start-dev
    depends_on:
      - postgres
    networks:
      - codebase_network

  postgres:
    image: postgres:16-alpine
    container_name: keycloak_postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: keycloak
      POSTGRES_PASSWORD: keycloak
    volumes:
      - keycloak_db:/var/lib/postgresql/data
    networks:
      - codebase_network

  redis:
    image: redis:7-alpine
    container_name: redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - codebase_network

networks:
  codebase_network:
    driver: bridge

volumes:
  keycloak_db:
  redis_data:
```

Start tất cả:
```bash
docker-compose up -d
```

## 🧪 Testing Flow với Gateway

### 1. Start Services

```bash
# Terminal 1: Redis
docker run -d -p 6379:6379 redis:latest

# Terminal 2: Keycloak
docker run -d -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:latest start-dev

# Terminal 3: API Gateway
cd src/ApiGateways/ApiGateway
dotnet run
```

### 2. Test Complete OAuth Flow (Authorization Code + PKCE)

⚠️ **QUAN TRỌNG**: Phải bắt đầu từ Gateway endpoint `/auth/login`, KHÔNG được gọi trực tiếp Keycloak!

#### Bước 1: Khởi tạo Login Flow

**Qua Browser** (Recommended):

```bash
# Mở browser và truy cập:
http://localhost:5238/auth/login?returnUrl=http://localhost:3000/dashboard
```

**Hoặc qua cURL** (để xem redirect):

```bash
curl -i "http://localhost:5238/auth/login?returnUrl=http://localhost:3000/dashboard"

# Response sẽ là 302 Redirect tới Keycloak:
# Location: http://localhost:8080/realms/base-realm/protocol/openid-connect/auth?
#   response_type=code&
#   client_id=api-gateway&
#   redirect_uri=http://localhost:5238/auth/signin-oidc&
#   scope=openid%20profile%20email&
#   state=<GENERATED_STATE>&
#   code_challenge=<GENERATED_CHALLENGE>&
#   code_challenge_method=S256
```

**Điều gì xảy ra ở backend**:
1. Gateway tạo PKCE data:
   - `code_verifier`: random 64 chars
   - `code_challenge`: SHA256(code_verifier) 
   - `state`: random 32 chars
2. Gateway lưu PKCE data vào Redis:
   ```
   Key: BFF_pkce:{state}
   Value: { codeVerifier, codeChallenge, state, redirectUri, expiresAt }
   TTL: 10 minutes
   ```
3. Gateway redirect browser tới Keycloak với `code_challenge`

#### Bước 2: User Login tại Keycloak

- Browser sẽ tự động redirect tới Keycloak login page
- Nhập credentials: `testuser` / `Test@123`
- Click "Sign In"

#### Bước 3: Callback và Token Exchange

Sau khi login thành công, Keycloak redirect về:

```
http://localhost:5238/auth/signin-oidc?code=<AUTH_CODE>&state=<STATE>
```

**Điều gì xảy ra ở backend**:
1. Gateway nhận callback với `code` và `state`
2. Gateway lấy PKCE data từ Redis bằng `state`
3. Gateway extract `code_verifier` từ PKCE data
4. Gateway gọi Keycloak Token Endpoint:
   ```
   POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token
   Body:
     - grant_type=authorization_code
     - code=<AUTH_CODE>
     - code_verifier=<FROM_REDIS>
     - redirect_uri=http://localhost:5238/auth/signin-oidc
     - client_id=api-gateway
     - client_secret=<CLIENT_SECRET>
   ```
5. Keycloak verify: `SHA256(code_verifier) == code_challenge`
6. Keycloak trả về tokens (access_token, refresh_token, id_token)
7. Gateway tạo session và lưu vào Redis:
   ```
   Key: BFF_session:{sessionId}
   Value: { userId, username, accessToken, refreshToken, expiresAt }
   TTL: 30 minutes (sliding)
   ```
8. Gateway set cookie: `session_id=<SESSION_ID>` (HttpOnly, Secure, SameSite=Lax)
9. Gateway redirect browser về `returnUrl` (http://localhost:3000/dashboard)

#### Bước 4: Test Authenticated APIs

Sau khi login thành công, browser sẽ có cookie `session_id`. Test các APIs:

```bash
# 1. Get current user info
curl -i http://localhost:5238/auth/user \
  -b "session_id=YOUR_SESSION_ID_FROM_COOKIE" \
  --cookie-jar cookies.txt

# Response:
{
  "userId": "uuid...",
  "username": "testuser",
  "email": "testuser@example.com",
  "roles": ["user", "admin"],
  "sessionId": "...",
  "expiresAt": "2024-01-01T12:00:00Z"
}

# 2. Call downstream API (sẽ tự động có Bearer token)
curl http://localhost:5238/api/products \
  -b "session_id=YOUR_SESSION_ID_FROM_COOKIE"

# Gateway tự động:
# - Lấy session từ Redis bằng session_id
# - Extract access_token từ session
# - Add header: Authorization: Bearer <access_token>
# - Forward request tới downstream service

# 3. Logout
curl -X POST http://localhost:5238/auth/logout \
  -b "session_id=YOUR_SESSION_ID_FROM_COOKIE"

# Response: 200 OK
# Cookie session_id sẽ bị xóa
```

### 3. Verify Flow với Browser DevTools

1. Mở browser DevTools (F12)
2. Tab **Network**, enable "Preserve log"
3. Navigate tới: `http://localhost:5238/auth/login`
4. Quan sát requests:
   ```
   1. GET /auth/login                        → 302 Redirect
   2. GET /realms/base-realm/.../auth?...    → Keycloak login page (200)
   3. POST /realms/base-realm/.../login      → Submit credentials
   4. GET /auth/signin-oidc?code=...&state=... → Gateway callback (302)
   5. GET http://localhost:3000/dashboard    → Redirect về webapp (200)
   ```
5. Tab **Application** → **Cookies** → Check cookie `session_id`:
   - Value: random session ID
   - HttpOnly: ✅ (không thể access từ JavaScript)
   - Secure: ✅ (nếu HTTPS)
   - SameSite: Lax

### 4. Debug với Redis

Kiểm tra data trong Redis:

```bash
# Connect tới Redis
docker exec -it redis redis-cli

# List all keys
KEYS *

# Output:
# 1) "BFF_session:abc123..."
# 2) "BFF_pkce:state_xyz..." (chỉ tồn tại 10 phút hoặc cho đến khi callback)

# Get session data
GET BFF_session:abc123...

# Get PKCE data (before callback)
GET BFF_pkce:state_xyz...

# Check TTL
TTL BFF_session:abc123...
# Output: 1800 (30 minutes)
```

### 5. Common Issues & Troubleshooting

#### ❌ Lỗi: "Invalid or expired state parameter"

**Nguyên nhân**: 
- PKCE data không tồn tại trong Redis
- User gọi trực tiếp Keycloak authorization endpoint (bỏ qua `/auth/login`)
- PKCE data đã expire (> 10 phút)

**Giải pháp**:
- ✅ Luôn bắt đầu từ `/auth/login`
- ✅ Complete flow trong vòng 10 phút
- ❌ KHÔNG mở trực tiếp Keycloak URL

#### ❌ Lỗi: "PKCE validation failed"

**Nguyên nhân**:
- `code_verifier` không match với `code_challenge`
- Keycloak PKCE setting chưa enable S256

**Giải pháp**:
- Verify Keycloak Client Settings → Advanced → PKCE = S256
- Check Redis có đúng PKCE data không

#### ❌ Lỗi: "Unauthorized" khi call API

**Nguyên nhân**:
- Session đã expire
- Cookie không được gửi (CORS issue)
- Token đã expire và refresh failed

**Giải pháp**:
- Check cookie `session_id` còn tồn tại không
- Verify CORS settings: `AllowCredentials = true`
- Check Gateway logs xem token refresh có thành công không

## 📊 Monitoring & Logging

### Enable Keycloak Logging

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  -e KC_LOG_LEVEL=DEBUG \
  quay.io/keycloak/keycloak:latest \
  start-dev
```

### View Logs

```bash
docker logs -f keycloak
```

### Keycloak Events

1. Vào **Realm settings** → **Events** tab
2. **User events settings**:
   - **Save Events**: ON
   - **Expiration**: 1 day
   - **Saved Types**: Login, Logout, Register, etc.
3. **Admin events settings**:
   - **Save Events**: ON
   - **Include Representation**: ON

View events: **Events** → **Login events** / **Admin events**

## 📝 Quick Reference

### Gateway Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/auth/login` | GET | Khởi tạo OAuth login flow (PHẢI gọi đầu tiên) |
| `/auth/signin-oidc` | GET | OAuth callback endpoint (Keycloak redirect về) |
| `/auth/logout` | POST | Logout và xóa session |
| `/auth/user` | GET | Lấy thông tin user hiện tại |
| `/health` | GET | Health check (Gateway + Redis) |

### Redis Keys

| Key Pattern | Description | TTL |
|-------------|-------------|-----|
| `BFF_pkce:{state}` | PKCE data (code_verifier, code_challenge) | 10 minutes |
| `BFF_session:{sessionId}` | User session (tokens, user info) | 30 minutes (sliding) |

### Flow Summary

```
1. Browser → GET /auth/login (Gateway)
   ↓ Gateway tạo PKCE → lưu Redis → redirect to Keycloak
   
2. User login tại Keycloak
   ↓ Keycloak redirect về /auth/signin-oidc?code=...&state=...
   
3. Gateway callback handler:
   ↓ Lấy PKCE từ Redis bằng state
   ↓ Exchange code + verifier → tokens
   ↓ Tạo session → lưu Redis
   ↓ Set cookie session_id
   ↓ Redirect về webapp
   
4. Browser có cookie session_id
   ↓ Mọi API calls tự động attach Bearer token từ session
```

### Important Configuration

**Keycloak Client Settings:**
- **Client ID**: `api-gateway`
- **Client authentication**: ON (confidential)
- **Standard flow**: ON (Authorization Code)
- **Direct access grants**: ON (optional, for testing)
- **PKCE Code Challenge Method**: `S256` (REQUIRED!)
- **Valid redirect URIs**: `http://localhost:5238/*`
- **Web origins**: `http://localhost:5238`, `http://localhost:3000`

**appsettings.json:**
```json
{
  "OAuth": {
    "Authority": "http://localhost:8080/realms/base-realm",
    "ClientId": "api-gateway",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "/auth/signin-oidc",
    "Scopes": "openid profile email"
  },
  "BFF": {
    "ConnectionStrings": "localhost:6379",
    "PkceExpirationMinutes": 10,
    "SessionAbsoluteExpirationMinutes": 30
  }
}
```

### Testing Checklist

- [ ] Keycloak đang chạy: `http://localhost:8080`
- [ ] Redis đang chạy: `docker ps | grep redis`
- [ ] Gateway đang chạy: `http://localhost:5238/health`
- [ ] Test users đã tạo: `testuser` / `Test@123`
- [ ] Client secret đã configure trong `appsettings.json`
- [ ] PKCE = S256 trong Keycloak Client Advanced settings
- [ ] Test login: `http://localhost:5238/auth/login`
- [ ] Check cookie sau login: DevTools → Application → Cookies
- [ ] Check Redis data: `docker exec -it redis redis-cli KEYS "*"`

### Common Mistakes ❌

1. **Gọi trực tiếp Keycloak authorization endpoint**
   - ❌ SAI: Mở browser `http://localhost:8080/realms/.../auth?...`
   - ✅ ĐÚNG: Mở browser `http://localhost:5238/auth/login`

2. **Quên enable PKCE S256 trong Keycloak**
   - ❌ Lỗi: "PKCE validation failed"
   - ✅ Fix: Client → Advanced → PKCE Code Challenge Method = S256

3. **CORS issue khi call từ frontend**
   - ❌ Cookie không được gửi
   - ✅ Fix: CORS policy phải có `AllowCredentials = true`

4. **Client secret không đúng**
   - ❌ Lỗi: "Unauthorized client"
   - ✅ Fix: Copy đúng client secret từ Keycloak Credentials tab

5. **Redis không chạy**
   - ❌ Lỗi: "Connection timeout"
   - ✅ Fix: `docker run -d -p 6379:6379 redis:latest`

## 🔗 Resources

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OAuth 2.0 PKCE](https://oauth.net/2/pkce/)
- [OpenID Connect](https://openid.net/connect/)
- [BFF Pattern](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps)

---

**Tip**: Sau khi setup xong, export realm configuration để backup:
- **Realm settings** → **Action** → **Partial export**
- Check tất cả options → **Export**
- Lưu file JSON để import lại sau này

