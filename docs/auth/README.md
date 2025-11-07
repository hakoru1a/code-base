# Authentication & Authorization Documentation

## 📚 Documentation Overview

Tài liệu về authentication và authorization trong codebase, sử dụng **BFF (Backend-for-Frontend) Pattern** với **Keycloak** và **OAuth 2.0 + PKCE**.

---

## 📋 Mục Lục Tài Liệu

### 1. [Keycloak Complete Guide](./keycloak-complete-guide.md)
**Nội dung:**
- Tổng quan BFF Architecture
- Keycloak Setup từ đầu (Docker, Realm, Client, Users)
- Permissions & Authorization Setup (Realm Roles, Client Roles, User Attributes)
- JWT Claims Structure
- Testing & Verification
- Troubleshooting

**Khi nào đọc:**
- ✅ Lần đầu setup Keycloak
- ✅ Cần config permissions cho users
- ✅ Debug authentication issues

---

### 2. [JWT Claims & Authorization](./jwt-claims-authorization.md)
**Nội dung:**
- JWT Token Structure chi tiết
- Claims Parsing Flow (Keycloak → Gateway → Services)
- RBAC (Role-Based Access Control)
- PBAC (Permission-Based/Policy-Based Access Control)
- Hybrid Authorization (RBAC + PBAC)
- Code Examples và Debugging

**Khi nào đọc:**
- ✅ Cần hiểu cách JWT claims được parse
- ✅ Implement authorization policies
- ✅ Debug permissions không hoạt động

---

### 3. [BFF Architecture & Flow](./bff-architecture-flow.md)
**Nội dung:**
- BFF Pattern Overview & So sánh với Traditional SPA
- Architecture Components (AuthController, SessionManager, etc.)
- Complete Authentication Flow (Login → Token Exchange → Session)
- API Call Flow (Session Validation → Token Injection)
- Security Features (PKCE, HttpOnly Cookies, Token Refresh)
- Redis Data Structures

**Khi nào đọc:**
- ✅ Cần hiểu toàn bộ flow authentication
- ✅ Implement BFF pattern cho app mới
- ✅ Debug flow issues (PKCE, Session, Token)

---

## 🚀 Quick Start Guide

### 1. Setup Keycloak (5 phút)

```bash
# Start Keycloak
docker run -d --name keycloak -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:latest start-dev
```

➡️ Sau đó follow [Keycloak Complete Guide](./keycloak-complete-guide.md)

### 2. Test Authentication Flow

```bash
# Bước 1: Login qua browser
http://localhost:5238/auth/login

# Bước 2: Nhập credentials tại Keycloak
Username: testuser
Password: Test@123

# Bước 3: Check cookie sau khi redirect
DevTools → Application → Cookies → session_id

# Bước 4: Test API
curl http://localhost:5238/api/products \
  -H "Cookie: session_id=YOUR_SESSION_ID"
```

➡️ Chi tiết: [BFF Architecture & Flow](./bff-architecture-flow.md)

### 3. Implement Authorization

```csharp
// Gateway Level - RBAC
[Authorize(Policy = PolicyNames.Rbac.AdminOnly)]
public class AdminController : ControllerBase { }

// Service Level - PBAC
public async Task<List<ProductDto>> GetAll()
{
    var userContext = User.ToUserClaimsContext();
    var filterContext = await _policyService.EvaluateAsync(userContext);
    // Apply filter...
}
```

➡️ Chi tiết: [JWT Claims & Authorization](./jwt-claims-authorization.md)

---

## 🎯 Common Tasks

### Task: Tạo User Mới với Permissions

**Steps:**
1. Vào Keycloak Admin Console
2. **Users** → **Add user**
3. Set password tại tab **Credentials**
4. Assign roles tại tab **Role mapping**
5. Thêm permissions tại tab **Attributes**:
   - Key: `permissions`
   - Value: `product:view product:create category:view`

➡️ Chi tiết: [Keycloak Complete Guide - Permissions Setup](./keycloak-complete-guide.md#permissions--authorization-setup)

---

### Task: Debug "Permission Denied"

**Checklist:**
- [ ] Check JWT token có claim `permissions`? (decode tại https://jwt.io)
- [ ] Check User Attribute `permissions` đã set trong Keycloak?
- [ ] Check Client Scope `permissions` đã assign cho client?
- [ ] Check Mapper có `Add to access token` = ON?
- [ ] Check logs: `[POLICY DEBUG]` trong Gateway

➡️ Chi tiết: [JWT Claims & Authorization - Debugging](./jwt-claims-authorization.md#debugging-authorization)

---

### Task: Implement Custom Policy

**Steps:**
1. Tạo Policy Context class trong `Shared/DTOs/Authorization/PolicyContexts/`
2. Tạo Policy Handler implement `IPolicyHandler<T>`
3. Register policy handler trong DI
4. Inject `IProductPolicyService` vào service
5. Call `EvaluateAsync(userContext, policyContext)`

➡️ Chi tiết: [JWT Claims & Authorization - PBAC](./jwt-claims-authorization.md#pbac-permission-basedpolicy-based-access-control)

---

## 🛡️ Security Checklist

### Keycloak Configuration
- [ ] PKCE enabled: Client → Advanced → PKCE = `S256`
- [ ] Client authentication: `ON` (confidential client)
- [ ] Valid redirect URIs configured
- [ ] Client secret không commit vào Git

### Gateway Configuration
- [ ] Redis connection working
- [ ] CORS configured với `AllowCredentials = true`
- [ ] Cookie options: `HttpOnly`, `Secure`, `SameSite=Lax`
- [ ] Session TTL hợp lý (8 hours recommended)

### Backend Services
- [ ] JWT validation enabled: `AddKeycloakAuthentication()`
- [ ] Authorization policies registered: `AddKeycloakAuthorization()`
- [ ] Roles và permissions defined trong `Shared/Identity/`

---

## 🔍 Architecture Diagram

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Browser   │◄──── Cookie ──────►│  Gateway    │◄─── OAuth 2.0 ────►│  Keycloak   │
│  (Frontend) │   (session_id)     │    (BFF)    │      (PKCE)        │    (IdP)    │
│             │                    │             │                    │             │
│  ❌ NO      │                    │  ✅ Stores  │                    │  Issues     │
│  Tokens     │                    │  - Tokens   │                    │  Tokens     │
│             │                    │  - Sessions │                    │             │
└─────────────┘                    └──────┬──────┘                    └─────────────┘
                                          │
                                  Bearer Token
                                          │
                                   ┌──────▼──────┐
                                   │   Services  │
                                   │ - Base.API  │
                                   └─────────────┘
```

---

## 📊 Flow Summary

### 1. Login Flow
```
Browser → GET /auth/login (Gateway)
         ↓ (PKCE generated & stored in Redis)
Keycloak → User login → Authorization code
         ↓
Gateway → Exchange code + verifier → Tokens
         ↓ (Session created in Redis)
Browser ← Set-Cookie: session_id (HttpOnly)
```

### 2. API Call Flow
```
Browser → GET /api/products (Cookie: session_id)
         ↓
Gateway → Validate session from Redis
         ↓ (Auto refresh if needed)
         → Inject Bearer token
         ↓
Service → Validate JWT → Check permissions
         ↓
Browser ← 200 OK + data
```

---

## 🐛 Debugging Tips

### 1. Check JWT Token
```bash
# Get token (testing only)
curl -X POST http://localhost:8080/realms/base-realm/protocol/openid-connect/token \
  -d "grant_type=password" \
  -d "client_id=api-gateway" \
  -d "client_secret=YOUR_SECRET" \
  -d "username=testuser" \
  -d "password=Test@123"

# Decode at https://jwt.io
```

### 2. Check Redis Session
```bash
docker exec -it redis redis-cli

# List sessions
KEYS BFF_session:*

# Get session data
GET BFF_session:abc123...

# Check TTL
TTL BFF_session:abc123...
```

### 3. Enable Debug Logs
```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "ApiGateway": "Debug",
      "Infrastructure": "Debug"
    }
  }
}
```

---

## 📚 Related Files in Codebase

### Gateway (BFF)
- `src/ApiGateways/ApiGateway/`
  - `Controllers/AuthController.cs` - OAuth endpoints
  - `Middlewares/SessionValidationMiddleware.cs` - Session validation
  - `Handlers/TokenDelegatingHandler.cs` - Token injection
  - `Services/PkceService.cs` - PKCE management
  - `Services/SessionManager.cs` - Session management
  - `Services/OAuthClient.cs` - Keycloak communication

### Infrastructure (Shared)
- `src/BuildingBlocks/Infrastructure/`
  - `Extensions/KeycloakAuthenticationExtensions.cs` - JWT config & policies
  - `Extensions/ClaimsPrincipalExtensions.cs` - Claims parsing
  - `Authorization/PolicyConfigurationService.cs` - PBAC setup

### Shared (Constants & DTOs)
- `src/BuildingBlocks/Shared/`
  - `Identity/Roles.cs` - Role constants
  - `Identity/Permissions.cs` - Permission constants
  - `Identity/PolicyNames.cs` - Policy name constants
  - `DTOs/Authorization/UserClaimsContext.cs` - User context DTO

### Service Implementation
- `src/Services/Base/Base.Application/`
  - `Feature/Product/Policies/` - Product PBAC policies
  - `Feature/Product/Services/ProductPolicyService.cs` - Policy service

---

## ❓ FAQ

### Q: Tại sao dùng BFF Pattern?
**A:** BFF Pattern giúp tokens không bao giờ lộ ra browser, chống XSS attacks. Gateway quản lý tokens trong Redis, browser chỉ có HttpOnly cookie.

### Q: RBAC vs PBAC - Khi nào dùng gì?
**A:** 
- **RBAC**: Gateway level, coarse-grained (admin, manager, user)
- **PBAC**: Service level, fine-grained (product:view, product:create)
- **Hybrid**: Kết hợp cả 2 cho flexibility

### Q: Làm sao để thêm custom claims vào JWT?
**A:** 
1. Add User Attribute trong Keycloak
2. Tạo Protocol Mapper trong Client Scope
3. Assign Client Scope cho Client
4. Claims sẽ tự động có trong token

### Q: Token refresh hoạt động như thế nào?
**A:** SessionValidationMiddleware tự động check token expiry trước mỗi request. Nếu < 60s before expiry, middleware gọi Keycloak refresh endpoint và update session trong Redis. Frontend không cần biết.

---

## 📞 Support

Nếu gặp vấn đề, check theo thứ tự:

1. **Keycloak logs**: `docker logs -f keycloak`
2. **Gateway logs**: Check `[POLICY DEBUG]`, `[JWT]` logs
3. **Redis data**: Verify session và PKCE data
4. **JWT token**: Decode tại https://jwt.io
5. **Troubleshooting sections** trong các docs

---

**Cập nhật lần cuối:** November 7, 2025

