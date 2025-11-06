# API Gateway - Backend for Frontend (BFF) Pattern

## 📋 Tổng quan

API Gateway này implement **BFF (Backend-for-Frontend) Pattern** với các tính năng:

- ✅ **OAuth 2.0 Authorization Code Flow + PKCE** 
- ✅ **Session-based authentication** với Redis
- ✅ **HttpOnly cookies** để bảo mật tokens
- ✅ **Automatic token refresh**
- ✅ **CSRF protection** với state parameter
- ✅ **Ocelot routing** tới downstream services

## 🏗️ Kiến trúc

```
Browser (User) 
    ↓ (HttpOnly Cookie: session_id)
API Gateway/BFF 
    ↓ (Redis: Session Storage)
    ↓ (Keycloak: OAuth Provider)
    ↓ (Bearer Token)
Backend Services (Base API, Generate API)
```

### Security Flow

```
1. Browser → GET /auth/login
2. Gateway tạo PKCE (code_verifier, code_challenge) → lưu Redis
3. Gateway redirect → Keycloak login page (với code_challenge)
4. User login tại Keycloak
5. Keycloak redirect → /auth/signin-oidc?code=xxx&state=yyy
6. Gateway validate state, lấy PKCE từ Redis
7. Gateway exchange code + code_verifier → tokens
8. Gateway lưu tokens vào Redis với session_id
9. Gateway set HttpOnly cookie: session_id
10. Browser → API requests (tự động có cookie)
11. Gateway lấy session từ Redis → add Bearer token → forward to services
```

## 📁 Cấu trúc Project

```
ApiGateway/
├── Configurations/          # Settings classes
│   ├── RedisSettings.cs    # Cấu hình Redis
│   └── OAuthSettings.cs    # Cấu hình OAuth/OIDC
│
├── Models/                  # Data models
│   ├── UserSession.cs      # Session data structure
│   ├── PkceData.cs         # PKCE data structure
│   └── TokenResponse.cs    # OAuth token response
│
├── Services/                # Business logic
│   ├── IPkceService.cs     # PKCE interface
│   ├── PkceService.cs      # PKCE implementation
│   ├── ISessionManager.cs  # Session interface
│   ├── SessionManager.cs   # Session implementation
│   ├── IOAuthClient.cs     # OAuth client interface
│   └── OAuthClient.cs      # OAuth client implementation
│
├── Middlewares/             # Request pipeline
│   └── SessionValidationMiddleware.cs  # Validate & refresh tokens
│
├── Handlers/                # Ocelot handlers
│   └── TokenDelegatingHandler.cs       # Inject Bearer token
│
├── Controllers/             # API endpoints
│   └── AuthController.cs   # Auth endpoints
│
├── Program.cs              # Application setup
├── ocelot.json            # Ocelot routing config
└── appsettings.json       # Application config
```

## 🚀 Setup Instructions

### 1. Prerequisites

- **.NET 9.0**
- **Redis** (local hoặc Docker)
- **Keycloak** (local hoặc Docker)

### 2. Start Redis

```bash
# Docker
docker run -d --name redis -p 6379:6379 redis:latest

# Hoặc local Redis instance
redis-server
```

### 3. Configure Keycloak

#### 3.1 Tạo Realm và Client

1. Vào Keycloak Admin Console: `http://localhost:8080`
2. Tạo realm mới: `base-realm`
3. Tạo client mới: `api-gateway`
4. Configure client:
   - Client Type: `Confidential`
   - Valid Redirect URIs: `http://localhost:5238/auth/signin-oidc`
   - Web Origins: `http://localhost:3000` (webapp URL)
   - Enable: `Standard Flow`, `Direct Access Grants`
   - PKCE: `S256` (required)

#### 3.2 Lấy Client Secret

1. Vào `Clients` → `api-gateway` → `Credentials`
2. Copy `Client Secret`
3. Update vào `appsettings.json`:

```json
{
  "OAuth": {
    "ClientSecret": "your-client-secret-here"
  }
}
```

### 4. Configure Application

Update `appsettings.json` hoặc `appsettings.Development.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "BFF_",
    "SessionExpirationMinutes": 60,
    "PkceExpirationMinutes": 10
  },
  
  "OAuth": {
    "Authority": "http://localhost:8080/realms/base-realm",
    "ClientId": "api-gateway",
    "ClientSecret": "your-client-secret-from-keycloak",
    "WebAppUrl": "http://localhost:3000",
    "TokenEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/token",
    "AuthorizationEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/auth"
  }
}
```

### 5. Run Application

```bash
cd src/ApiGateways/ApiGateway
dotnet restore
dotnet run
```

Gateway sẽ chạy tại: `http://localhost:5238`

## 🔌 API Endpoints

### Authentication Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/auth/login` | GET | Khởi tạo OAuth login flow |
| `/auth/signin-oidc` | GET | Callback từ Keycloak (internal) |
| `/auth/logout` | POST | Logout user |
| `/auth/user` | GET | Lấy thông tin user hiện tại |
| `/auth/health` | GET | Health check |

### Proxied API Routes (via Ocelot)

| Gateway Route | Downstream Service | Description |
|---------------|-------------------|-------------|
| `/base-api/*` | Base API (port 5239) | Base service endpoints |
| `/generate-api/*` | Generate API (port 5027) | Generate service endpoints |

## 🔐 Security Features

### 1. PKCE (Proof Key for Code Exchange)

- **Code Verifier**: Random string (64 chars) được tạo và lưu trong Redis
- **Code Challenge**: SHA256 hash của code_verifier
- Prevent authorization code interception attack

### 2. Session Management

- **Session ID**: Random 256-bit value
- **Storage**: Redis với sliding + absolute expiration
- **Cookie**: HttpOnly, Secure (HTTPS), SameSite=Lax

### 3. Token Storage

- **Access Token**: Lưu trong Redis, KHÔNG expose ra browser
- **Refresh Token**: Lưu trong Redis, dùng để refresh access token
- **ID Token**: User claims, lưu trong Redis

### 4. Automatic Token Refresh

- Middleware tự động check token expiration
- Refresh token trước 60s khi expire
- Transparent cho frontend (không cần handle)

### 5. CSRF Protection

- **State Parameter**: Random string được validate trong callback
- One-time use: Sau khi validate, state bị xóa khỏi Redis

## 📊 Redis Data Structure

### Session Key

```
Key: BFF_session:{session_id}
Value: JSON của UserSession
TTL: Sliding 60m, Absolute 480m (8h)
```

Example:
```json
{
  "sessionId": "abc123...",
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "idToken": "eyJhbGc...",
  "expiresAt": "2025-11-04T10:30:00Z",
  "userId": "user-uuid",
  "username": "john.doe",
  "email": "john@example.com",
  "roles": ["user", "admin"]
}
```

### PKCE Key

```
Key: BFF_pkce:{state}
Value: JSON của PkceData
TTL: 10 minutes (absolute)
```

Example:
```json
{
  "codeVerifier": "random-64-chars...",
  "codeChallenge": "base64url-sha256...",
  "codeChallengeMethod": "S256",
  "state": "random-state-value",
  "redirectUri": "http://localhost:3000/dashboard",
  "expiresAt": "2025-11-04T09:50:00Z"
}
```

## 🔄 Request Flow

### Login Flow

```
1. Frontend → GET /auth/login?returnUrl=/dashboard
2. Gateway:
   - Tạo PKCE: code_verifier, code_challenge, state
   - Lưu Redis: pkce:{state} → PkceData
3. Gateway → 302 Redirect
4. Browser → Keycloak login page
5. User nhập credentials
6. Keycloak → 302 Redirect → /auth/signin-oidc?code=xxx&state=yyy
7. Gateway:
   - Validate state
   - Lấy PKCE từ Redis (và xóa)
   - POST Keycloak Token Endpoint (code + code_verifier)
   - Nhận tokens
   - Tạo session, lưu Redis: session:{session_id} → UserSession
   - Set cookie: session_id
8. Gateway → 302 Redirect → /dashboard
9. Browser → Có session cookie → Authenticated!
```

### API Request Flow

```
1. Frontend → GET /base-api/products (Cookie: session_id=xxx)
2. SessionValidationMiddleware:
   - Lấy session_id từ cookie
   - Load session từ Redis
   - Check token expiration
   - Refresh nếu cần
   - Set HttpContext.Items["AccessToken"]
3. Ocelot Routing:
   - Match route: /base-api/* → http://localhost:5239/api/*
4. TokenDelegatingHandler:
   - Lấy AccessToken từ HttpContext.Items
   - Add header: Authorization: Bearer {token}
5. Forward → Base API
6. Base API validate JWT → Process request → Return response
7. Gateway → Return response → Frontend
```

### Logout Flow

```
1. Frontend → POST /auth/logout (Cookie: session_id=xxx)
2. Gateway:
   - Lấy session từ Redis
   - POST Keycloak Revoke Endpoint (refresh_token)
   - Xóa session khỏi Redis
   - Delete cookie: session_id
3. Gateway → 200 OK
4. Frontend → Redirect to login page
```

## 🧪 Testing

### 1. Test Login Flow

```bash
# 1. Start login
curl -v http://localhost:5238/auth/login

# Sẽ redirect tới Keycloak, login qua browser

# 2. Sau khi login, check cookie
curl -v http://localhost:5238/auth/user \
  -H "Cookie: session_id=your-session-id"

# Response:
{
  "userId": "...",
  "username": "...",
  "email": "...",
  "roles": ["user", "admin"]
}
```

### 2. Test API Proxy

```bash
# Call proxied API (cần session cookie)
curl http://localhost:5238/base-api/products \
  -H "Cookie: session_id=your-session-id"

# Gateway sẽ:
# - Validate session
# - Add Bearer token
# - Forward tới Base API
```

### 3. Test Logout

```bash
curl -X POST http://localhost:5238/auth/logout \
  -H "Cookie: session_id=your-session-id"

# Response:
{
  "message": "Logged out successfully"
}
```

## 🐛 Troubleshooting

### 1. Redis Connection Failed

```
Error: Unable to connect to Redis
```

**Fix:**
- Check Redis is running: `redis-cli ping` (should return `PONG`)
- Update `appsettings.json` → `Redis:ConnectionString`

### 2. Token Exchange Failed

```
Error: Failed to exchange code for tokens
```

**Fix:**
- Check Keycloak is running: `http://localhost:8080`
- Verify Client Secret in `appsettings.json`
- Check Keycloak client config: Valid Redirect URIs

### 3. CORS Error

```
Error: CORS policy blocked
```

**Fix:**
- Update `appsettings.json` → `OAuth:WebAppUrl` với frontend URL
- Frontend phải gửi credentials: `fetch(url, { credentials: 'include' })`

### 4. Session Not Found

```
Error: Session expired. Please login again.
```

**Causes:**
- Session hết hạn (60 phút sliding, 8 giờ absolute)
- Redis bị restart (mất data)
- Cookie bị clear

**Fix:**
- Login lại: `/auth/login`

## 📚 Best Practices

### 1. Production Configuration

```json
{
  "Redis": {
    "ConnectionString": "redis-prod:6379,password=xxx",
    "UseSsl": true
  },
  
  "OAuth": {
    "Authority": "https://keycloak.prod.com/realms/base-realm",
    "RequireHttpsMetadata": true,
    "WebAppUrl": "https://webapp.prod.com"
  }
}
```

### 2. Environment Variables

Dùng environment variables cho sensitive data:

```bash
export OAuth__ClientSecret="real-secret-here"
export Redis__ConnectionString="redis-prod:6379,password=xxx"
```

### 3. Monitoring

- Log tất cả authentication events
- Monitor Redis connection health
- Track token refresh rate
- Alert on authentication failures

### 4. Scaling

- Redis cluster cho high availability
- Multiple Gateway instances (share sessions via Redis)
- Load balancer với sticky sessions (optional)

## 🔗 Related Documentation

- [OAuth 2.0 RFC 6749](https://datatracker.ietf.org/doc/html/rfc6749)
- [PKCE RFC 7636](https://datatracker.ietf.org/doc/html/rfc7636)
- [OpenID Connect](https://openid.net/connect/)
- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)

## 📝 License

MIT License - Code này được tạo để học tập và phát triển dự án internal.

---

**Lưu ý:** Documentation này được viết để người mới join dự án có thể hiểu và maintain code dễ dàng. Nếu có thắc mắc, vui lòng liên hệ team lead hoặc tạo issue.

