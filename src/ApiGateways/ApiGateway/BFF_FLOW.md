# BFF Authentication Flow - Mô tả chi tiết

## 📌 Tổng quan Architecture

```
Browser (User) 
    ↕️ HttpOnly Cookie: session_id (an toàn, không thể đọc bằng JS)
    
API Gateway/BFF (Backend-for-Frontend)
    ↕️ IRedisRepository (Infrastructure có sẵn)
    
Redis (Session Store)
    - Key: BFF_session:{sessionId} → UserSession object (tokens + user info)
    - Key: BFF_pkce:{state} → PkceData object (PKCE security)
    
Keycloak (Identity Provider - OAuth 2.0/OIDC)
    - Authorization Endpoint (login page)
    - Token Endpoint (exchange code → tokens)
    - Revoke Endpoint (logout)
    
Backend Services (Downstream APIs)
    ↕️ Authorization: Bearer {access_token}
    - Base API (port 5239)
    - Generate API (port 5027)
```

---

## 🔐 FLOW 1: Login - User Authentication

### Bước 1: User nhấn "Login" ở Frontend

```
Frontend (Browser)
    → GET http://gateway.com/auth/login?returnUrl=/dashboard
```

### Bước 2: Gateway khởi tạo PKCE

**AuthController.Login()** thực hiện:

1. Tạo PKCE data bằng **PkceService.GeneratePkceAsync()**:
   - Tạo `code_verifier` (random 64 chars): `abc123...xyz789`
   - Hash SHA256 thành `code_challenge`: `base64url(sha256(code_verifier))`
   - Tạo `state` (random 32 chars) cho CSRF protection: `state_xyz...`
   - Tạo `PkceData` object với verifier, challenge, state, redirectUri

2. Lưu vào Redis dùng **IRedisRepository.SetAsync()**:
   ```
   Key: BFF_pkce:state_xyz...
   Value: { codeVerifier, codeChallenge, state, redirectUri, expiresAt }
   TTL: 10 minutes
   ```

3. Build Authorization URL:
   ```
   http://keycloak.com/realms/base-realm/protocol/openid-connect/auth?
       response_type=code
       &client_id=api-gateway
       &redirect_uri=http://gateway.com/auth/signin-oidc
       &scope=openid profile email
       &state=state_xyz...
       &code_challenge=BASE64URL_SHA256
       &code_challenge_method=S256
   ```

4. Redirect browser:
   ```
   Response: 302 Redirect
   Location: [Authorization URL]
   ```

### Bước 3: User login tại Keycloak

```
Browser được redirect tới Keycloak
    → Keycloak hiển thị login page
    → User nhập username/password (VD: testuser / Test@123)
    → Keycloak validate credentials
    → Keycloak tạo authorization code: CODE_ABC123
    → Keycloak lưu association: code + code_challenge
```

### Bước 4: Keycloak redirect về Gateway với code

```
Keycloak redirect browser:
    → GET http://gateway.com/auth/signin-oidc?
        code=CODE_ABC123
        &state=state_xyz...
```

### Bước 5: Gateway exchange code lấy tokens

**AuthController.SignInCallback()** thực hiện:

1. Validate state parameter (CSRF protection):
   - So sánh state từ query với state đã lưu

2. Lấy PKCE data từ Redis bằng **PkceService.GetAndRemovePkceAsync()**:
   ```
   Key: BFF_pkce:state_xyz...
   → Lấy PkceData (chứa code_verifier)
   → XÓA key ngay (one-time use, chống replay attack)
   ```

3. Exchange code lấy tokens bằng **OAuthClient.ExchangeCodeForTokensAsync()**:
   ```
   POST http://keycloak.com/realms/base-realm/protocol/openid-connect/token
   Body (form-urlencoded):
       grant_type=authorization_code
       code=CODE_ABC123
       code_verifier=abc123...xyz789
       client_id=api-gateway
       client_secret=secret_here
       redirect_uri=http://gateway.com/auth/signin-oidc
   
   Keycloak verify:
       - Code hợp lệ chưa?
       - SHA256(code_verifier) == code_challenge đã lưu? (PKCE verify)
       - Client credentials đúng chưa?
   
   Response 200 OK:
   {
       "access_token": "eyJhbGc...",      // JWT, valid 5 minutes
       "refresh_token": "eyJhbGc...",     // JWT, valid 30 minutes
       "id_token": "eyJhbGc...",          // JWT, chứa user info
       "token_type": "Bearer",
       "expires_in": 300,
       "scope": "openid profile email"
   }
   ```

4. Tạo session bằng **SessionManager.CreateSessionAsync()**:
   
   a. Parse `access_token` (JWT) để extract user info:
   ```
   JWT Claims:
   {
       "sub": "user-uuid-123",
       "preferred_username": "testuser",
       "email": "testuser@example.com",
       "realm_access": { "roles": ["user", "admin"] }
   }
   ```
   
   b. Tạo `sessionId` random (32 bytes = 256 bits)
   
   c. Tạo `UserSession` object:
   ```json
   {
       "sessionId": "SESSION_XYZ...",
       "accessToken": "eyJhbGc...",
       "refreshToken": "eyJhbGc...",
       "idToken": "eyJhbGc...",
       "expiresAt": "2025-11-04T10:05:00Z",
       "userId": "user-uuid-123",
       "username": "testuser",
       "email": "testuser@example.com",
       "roles": ["user", "admin"],
       "createdAt": "2025-11-04T10:00:00Z",
       "lastAccessedAt": "2025-11-04T10:00:00Z"
   }
   ```
   
   d. Lưu vào Redis dùng **IRedisRepository.SetAsync()**:
   ```
   Key: BFF_session:SESSION_XYZ...
   Value: UserSession object (JSON)
   TTL: 480 minutes (8 hours absolute)
   ```

5. Set HttpOnly cookie:
   ```
   Response Headers:
   Set-Cookie: session_id=SESSION_XYZ...;
               HttpOnly;                    ← JS không đọc được
               Secure;                      ← Chỉ gửi qua HTTPS
               SameSite=Lax;               ← CSRF protection
               Path=/;
               Max-Age=28800               ← 8 hours
   ```

6. Redirect về webapp:
   ```
   Response: 302 Redirect
   Location: http://webapp.com/dashboard
   ```

### Bước 6: Browser có session cookie

```
Browser giờ có cookie: session_id=SESSION_XYZ...
Mọi request tới gateway.com sẽ tự động gửi cookie này
```

---

## 🔄 FLOW 2: API Call - Authenticated Request

### Bước 1: Frontend gọi API

```
Frontend code:
fetch('http://gateway.com/base-api/products', {
    credentials: 'include'  // ← QUAN TRỌNG: gửi cookies
})

Request:
GET http://gateway.com/base-api/products
Cookie: session_id=SESSION_XYZ...
```

### Bước 2: SessionValidationMiddleware xử lý

**Middleware chạy trước khi request tới Ocelot:**

1. Đọc cookie `session_id`:
   ```csharp
   var sessionId = httpContext.Request.Cookies["session_id"];
   if (string.IsNullOrEmpty(sessionId))
       return 401 Unauthorized;
   ```

2. Load session từ Redis bằng **SessionManager.GetSessionAsync()**:
   ```
   Key: BFF_session:SESSION_XYZ...
   → Lấy UserSession object
   → Tự động update lastAccessedAt (sliding expiration)
   ```

3. Check access token expiration:
   ```csharp
   if (session.NeedsRefresh())  // expires trong < 60s
   {
       // Cần refresh token
   }
   ```

### Bước 3: Refresh token nếu cần

**Nếu access token sắp hết hạn (< 60s):**

1. Gọi **OAuthClient.RefreshTokenAsync()**:
   ```
   POST http://keycloak.com/realms/base-realm/protocol/openid-connect/token
   Body:
       grant_type=refresh_token
       refresh_token=eyJhbGc...
       client_id=api-gateway
       client_secret=secret_here
   
   Response 200 OK:
   {
       "access_token": "NEW_eyJhbGc...",     // Token mới
       "refresh_token": "NEW_eyJhbGc...",    // Refresh token mới
       "expires_in": 300
   }
   ```

2. Update session bằng **SessionManager.UpdateSessionAsync()**:
   ```
   session.accessToken = NEW_token
   session.refreshToken = NEW_refresh_token
   session.expiresAt = now + 5 minutes
   
   → Save lại vào Redis
   Key: BFF_session:SESSION_XYZ...
   ```

3. Continue request với token mới:
   ```csharp
   httpContext.Items["AccessToken"] = session.AccessToken;
   ```

**Nếu token còn hạn:**
```csharp
httpContext.Items["AccessToken"] = session.AccessToken;
// Tiếp tục pipeline
```

### Bước 4: Ocelot Routing

**Ocelot match route từ ocelot.json:**

```json
Route matched:
{
    "UpstreamPathTemplate": "/base-api/{everything}",
    "DownstreamPathTemplate": "/api/{everything}",
    "DownstreamScheme": "http",
    "DownstreamHostAndPorts": [
        { "Host": "localhost", "Port": 5239 }
    ]
}

Transform:
/base-api/products → http://localhost:5239/api/products
```

### Bước 5: TokenDelegatingHandler inject Bearer token

**Handler tự động thêm Authorization header:**

```csharp
var accessToken = httpContext.Items["AccessToken"] as string;

request.Headers.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);
```

Request tới downstream service:
```
GET http://localhost:5239/api/products
Headers:
    Authorization: Bearer eyJhbGc...
    X-Forwarded-For: client-ip
    X-Forwarded-Proto: https
```

### Bước 6: Backend API xử lý

**Base API (port 5239):**

1. Validate JWT token:
   ```csharp
   [Authorize] attribute
   → Middleware validate JWT signature
   → Check expiration
   → Extract claims (userId, roles, permissions)
   ```

2. Check authorization (PBAC nếu cần):
   ```csharp
   if (!user.HasPermission("product.view"))
       return 403 Forbidden;
   ```

3. Process business logic:
   ```csharp
   var products = await _productService.GetAllAsync();
   return Ok(products);
   ```

4. Response:
   ```
   200 OK
   Content-Type: application/json
   Body: [{ id: 1, name: "Product 1" }, ...]
   ```

### Bước 7: Gateway forward response về Frontend

```
Gateway → Frontend:
200 OK
Body: [{ id: 1, name: "Product 1" }, ...]

Frontend nhận data và render UI
```

---

## 🚪 FLOW 3: Logout

### Bước 1: Frontend gọi logout

```
Frontend:
fetch('http://gateway.com/auth/logout', {
    method: 'POST',
    credentials: 'include'
})
```

### Bước 2: Gateway xử lý logout

**AuthController.Logout():**

1. Lấy session từ cookie và Redis:
   ```csharp
   var sessionId = Request.Cookies["session_id"];
   var session = await _sessionManager.GetSessionAsync(sessionId);
   ```

2. Revoke tokens ở Keycloak bằng **OAuthClient.RevokeTokenAsync()**:
   ```
   POST http://keycloak.com/realms/base-realm/protocol/openid-connect/revoke
   Body:
       token=refresh_token_here
       token_type_hint=refresh_token
       client_id=api-gateway
       client_secret=secret_here
   
   → Keycloak invalidate refresh token
   → Access token vẫn valid cho đến khi expire (5 min)
   ```

3. Xóa session khỏi Redis:
   ```
   Key: BFF_session:SESSION_XYZ...
   → DELETE
   ```

4. Delete cookie:
   ```
   Response Headers:
   Set-Cookie: session_id=; 
               Path=/; 
               Expires=Thu, 01 Jan 1970 00:00:00 GMT
   ```

5. Response:
   ```json
   200 OK
   { "message": "Logged out successfully" }
   ```

### Bước 3: Frontend redirect về login

```
Frontend nhận 200 OK
→ Redirect to /login page
→ User phải login lại
```

---

## 🛡️ Security Features Explained

### 1. PKCE (Proof Key for Code Exchange)

**Mục đích:** Chống code interception attack

**Flow:**
```
1. Gateway tạo code_verifier (random 64 chars)
2. Gateway hash: code_challenge = SHA256(code_verifier)
3. Gateway gửi code_challenge lên Keycloak (trong authorization request)
4. Keycloak lưu code_challenge, trả về authorization code
5. Attacker có thể intercept code, nhưng KHÔNG có code_verifier
6. Gateway gửi code + code_verifier để đổi tokens
7. Keycloak verify: SHA256(code_verifier) == code_challenge?
8. Nếu match → OK, nếu không → reject

→ Attacker không thể dùng stolen code vì thiếu code_verifier
```

### 2. HttpOnly Cookies

**Mục đích:** Chống XSS attack lấy cắp session

```
Set-Cookie: session_id=...; HttpOnly

→ JavaScript không thể đọc: document.cookie không trả về session_id
→ Chỉ browser engine mới access được
→ XSS attack không lấy được session cookie
```

### 3. State Parameter (CSRF Protection)

**Mục đích:** Chống CSRF attack trong OAuth flow

```
1. Gateway tạo random state
2. Lưu state vào Redis (liên kết với PKCE data)
3. Gửi state lên Keycloak trong authorization URL
4. Keycloak trả về state khi callback
5. Gateway so sánh: state từ callback == state trong Redis?
6. Nếu match → OK, nếu không → có thể bị CSRF attack

→ Attacker không thể fake callback request vì không biết state
```

### 4. Token Storage in Backend

**Mục đích:** Không expose tokens ra browser

```
❌ BAD - SPA lưu token ở localStorage/sessionStorage:
localStorage.setItem('token', access_token)
→ XSS có thể đọc: localStorage.getItem('token')

✅ GOOD - BFF pattern lưu token ở Redis (backend):
Redis: BFF_session:abc → { accessToken, refreshToken, ... }
Browser chỉ có: Cookie: session_id=abc
→ XSS không lấy được token
→ CSRF protection bởi SameSite=Lax
```

### 5. Automatic Token Refresh

**Mục đích:** Transparent cho frontend, không cần handle token expiration

```
Frontend không cần:
- Check token expiration
- Call refresh endpoint manually
- Handle refresh token logic

Gateway tự động:
- Check expiration trước mỗi request
- Refresh khi cần (< 60s before expiry)
- Update session in Redis
- Continue request với token mới

→ Frontend chỉ cần gọi API bình thường
```

---

## 📊 Redis Data Structure

### Session Data

```
Key: BFF_session:SESSION_ID_HERE
TTL: 28800 seconds (8 hours)

Value (JSON):
{
    "sessionId": "SESSION_ID_HERE",
    "accessToken": "eyJhbGc...",           // JWT, 5 min
    "refreshToken": "eyJhbGc...",          // JWT, 30 min
    "idToken": "eyJhbGc...",               // OIDC ID token
    "tokenType": "Bearer",
    "expiresAt": "2025-11-04T10:05:00Z",   // Access token expiry
    "createdAt": "2025-11-04T10:00:00Z",
    "lastAccessedAt": "2025-11-04T10:00:00Z",
    "userId": "user-uuid-123",
    "username": "testuser",
    "email": "testuser@example.com",
    "roles": ["user", "admin"],
    "claims": {
        "name": "Test User",
        "given_name": "Test",
        "family_name": "User"
    }
}
```

### PKCE Data

```
Key: BFF_pkce:STATE_HERE
TTL: 600 seconds (10 minutes)

Value (JSON):
{
    "codeVerifier": "abc123...xyz789",           // 64 chars random
    "codeChallenge": "BASE64URL_SHA256_HASH",
    "codeChallengeMethod": "S256",
    "state": "STATE_HERE",
    "redirectUri": "http://webapp.com/dashboard",
    "createdAt": "2025-11-04T09:55:00Z",
    "expiresAt": "2025-11-04T10:05:00Z"
}
```

---

## 🔍 Infrastructure Components Used

### 1. IRedisRepository (từ Contracts.Common.Interface)

```csharp
// String operations
Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry);
Task<string?> GetStringAsync(string key);

// Object operations (dùng JSON serialization)
Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry);
Task<T?> GetAsync<T>(string key);

// Key operations
Task<bool> DeleteAsync(string key);
Task<bool> ExistsAsync(string key);
Task<bool> ExpireAsync(string key, TimeSpan expiry);
```

**Cách dùng trong BFF:**
```csharp
// Lưu session
await _redisRepo.SetAsync(
    key: "BFF_session:abc123",
    value: userSessionObject,
    expiry: TimeSpan.FromHours(8)
);

// Lấy session
var session = await _redisRepo.GetAsync<UserSession>("BFF_session:abc123");

// Xóa session
await _redisRepo.DeleteAsync("BFF_session:abc123");
```

### 2. BffSettings (kế thừa CacheSettings từ Shared.Configurations)

```csharp
public class BffSettings : CacheSettings
{
    // Từ CacheSettings
    public string ConnectionStrings { get; set; }  // Redis connection
    
    // BFF specific
    public string InstanceName { get; set; }                      // "BFF_"
    public int SessionSlidingExpirationMinutes { get; set; }      // 60
    public int SessionAbsoluteExpirationMinutes { get; set; }     // 480
    public int PkceExpirationMinutes { get; set; }                // 10
    public int RefreshTokenBeforeExpirationSeconds { get; set; }  // 60
}
```

**Configuration (appsettings.json):**
```json
{
    "BFF": {
        "ConnectionStrings": "localhost:6379",
        "InstanceName": "BFF_",
        "SessionSlidingExpirationMinutes": 60,
        "SessionAbsoluteExpirationMinutes": 480,
        "PkceExpirationMinutes": 10,
        "RefreshTokenBeforeExpirationSeconds": 60
    }
}
```

---

## 🎯 So sánh với pattern khác

### Traditional SPA với JWT in LocalStorage

```
❌ Security Issues:
- XSS có thể steal tokens
- No automatic refresh (frontend phải handle)
- Token exposed in browser memory
- Refresh token exposed

✅ BFF Pattern giải quyết:
- HttpOnly cookies → XSS không đọc được
- Auto refresh ở backend → transparent
- Tokens chỉ ở backend (Redis)
- PKCE + CSRF protection
```

### Server-Side Session (traditional MVC)

```
✅ Tương tự BFF về security
❌ Nhược điểm:
- Không có OAuth/OIDC (phải tự implement auth)
- Không có SSO across apps
- Không có standard token format (JWT)
- Khó integrate với microservices

✅ BFF Pattern advantages:
- OAuth 2.0/OIDC standard
- SSO support (Keycloak)
- JWT tokens cho downstream services
- Scalable (Redis distributed cache)
```

---

Tài liệu này mô tả TOÀN BỘ flow của BFF pattern từ login đến logout, bao gồm cả việc reuse Infrastructure components có sẵn (IRedisRepository, CacheSettings) thay vì tạo mới.
