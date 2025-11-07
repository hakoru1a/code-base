# Documentation Migration Guide

## 🎯 Tổng quan

Documentation về authentication/authorization đã được **tái cấu trúc** và **gộp lại** để:
- ✅ Loại bỏ thông tin trùng lặp
- ✅ Tổ chức theo chủ đề rõ ràng
- ✅ Dễ dàng navigate và tìm kiếm
- ✅ Thống nhất JSON structure cho claims parsing

---

## 📁 Cấu trúc mới

### Files đã tạo mới

```
docs/auth/
├── README.md                          ← Entry point chính
├── keycloak-complete-guide.md         ← Setup Keycloak + Permissions
├── jwt-claims-authorization.md        ← JWT Claims parsing + RBAC/PBAC
├── bff-architecture-flow.md           ← BFF Pattern + Authentication Flow
└── MIGRATION_GUIDE.md                 ← Document này
```

### Files đã xóa (đã gộp vào files mới)

| File cũ | Nội dung đã chuyển sang |
|---------|-------------------------|
| `keycloak-permissions.md` | → `keycloak-complete-guide.md` (Section: Permissions & Authorization) |
| `keycloak_setup.md` | → `keycloak-complete-guide.md` (Section: Keycloak Setup) |
| `bff_flow.md` | → `bff-architecture-flow.md` (Section: Complete Authentication Flow) |
| `auth.md` | → `bff-architecture-flow.md` (Section: BFF Pattern Overview) |

---

## 🗂️ Nội dung từng file mới

### 1. README.md - Entry Point

**Mục đích:** Quick reference và navigation hub

**Nội dung:**
- Overview tất cả documents
- Quick Start Guide (5 phút setup Keycloak)
- Common Tasks (tạo user, debug permissions, implement policy)
- Security Checklist
- Architecture Diagram tổng quan
- FAQ

**Khi nào đọc:**
- Lần đầu tiếp cận codebase
- Cần quick reference
- Tìm document cụ thể

---

### 2. keycloak-complete-guide.md - Setup & Configuration

**Mục đích:** Toàn bộ setup Keycloak từ đầu

**Nội dung gộp từ:**
- ✅ `keycloak_setup.md` → Keycloak Setup (Docker, Realm, Client, Users)
- ✅ `keycloak-permissions.md` → Permissions Setup (Realm Roles, Client Roles, User Attributes)

**Sections:**
1. **Tổng quan Architecture** - BFF Pattern overview
2. **Keycloak Setup** - Từng bước setup Keycloak
3. **Permissions & Authorization Setup** - Realm Roles vs Client Roles vs User Attributes
4. **JWT Claims Structure** - Token structure example
5. **Testing & Verification** - Test login, decode token, verify Redis

**Thông tin đã loại bỏ trùng lặp:**
- ❌ Phần giải thích BFF Pattern (giữ 1 lần duy nhất)
- ❌ Phần so sánh BFF vs SPA (chuyển sang bff-architecture-flow.md)
- ❌ Phần JWT Claims parsing (chuyển sang jwt-claims-authorization.md)

---

### 3. jwt-claims-authorization.md - Claims Parsing & Authorization

**Mục đích:** Chi tiết về JWT claims và authorization logic

**Nội dung mới (không có trong docs cũ):**
- ✅ JWT Token Structure chi tiết (tất cả claims)
- ✅ Claims Parsing Flow step-by-step với code
- ✅ UserClaimsContext mapping
- ✅ RBAC implementation và use cases
- ✅ PBAC implementation và use cases
- ✅ Hybrid Authorization (RBAC + PBAC)
- ✅ Code examples thực tế
- ✅ Debugging tips

**Key Concepts được làm rõ:**
1. **Claims Extraction Flow:**
   ```
   JWT Token (Keycloak)
       ↓
   MapKeycloakRoles() extracts roles
       ↓
   ToUserClaimsContext() creates UserClaimsContext
       ↓
   Authorization Check (RBAC/PBAC)
   ```

2. **UserClaimsContext Structure:**
   ```csharp
   {
       UserId: "...",
       Roles: ["admin", "user"],
       Permissions: ["product:view", "product:create"],
       Claims: { ... },
       CustomAttributes: { department: "Sales" }
   }
   ```

3. **When to use what:**
   - RBAC: Gateway level, coarse-grained
   - PBAC: Service level, fine-grained
   - Hybrid: Permission OR Role

---

### 4. bff-architecture-flow.md - BFF Pattern & Flow

**Mục đích:** Hiểu toàn bộ architecture và authentication flow

**Nội dung gộp từ:**
- ✅ `bff_flow.md` → Complete Authentication Flow (Login, Token Exchange, API Call)
- ✅ `auth.md` → BFF vs Traditional SPA comparison

**Sections:**
1. **BFF Pattern Overview** - What, Why, BFF vs SPA
2. **Architecture Components** - AuthController, SessionManager, PkceService, etc.
3. **Complete Authentication Flow** - Step-by-step với sequence diagram
4. **API Call Flow** - Session validation, token refresh, injection
5. **Security Features** - PKCE, HttpOnly Cookies, Token Refresh
6. **Redis Data Structures** - Session & PKCE data structure

**Chi tiết flow được làm rõ:**

**Login Flow:**
```
Browser → GET /auth/login
       ↓ (PKCE generated)
Gateway → Store PKCE in Redis
       ↓
Keycloak → User login → Auth code
       ↓
Gateway → Exchange code + verifier → Tokens
       ↓ (Session created)
Browser ← Set-Cookie: session_id
```

**API Call Flow:**
```
Browser → GET /api/products (Cookie: session_id)
       ↓
Middleware → Validate session from Redis
       ↓ (Auto refresh if needed)
Handler → Inject Bearer token
       ↓
Service → Validate JWT → Check permissions
       ↓
Browser ← 200 OK + data
```

---

## 🔄 Migration Path

### Nếu bạn đang đọc docs cũ:

| Docs cũ | Section | Đọc docs mới |
|---------|---------|--------------|
| `keycloak_setup.md` | Setup Keycloak | → `keycloak-complete-guide.md` (Section 2) |
| `keycloak-permissions.md` | Gán permissions | → `keycloak-complete-guide.md` (Section 3) |
| `keycloak-permissions.md` | Khi nào dùng Realm Role/Client Role/User Attribute | → `keycloak-complete-guide.md` (Section 3.1) |
| `bff_flow.md` | Login flow | → `bff-architecture-flow.md` (Section 3) |
| `bff_flow.md` | API call flow | → `bff-architecture-flow.md` (Section 4) |
| `bff_flow.md` | Redis structure | → `bff-architecture-flow.md` (Section 6) |
| `auth.md` | BFF vs SPA | → `bff-architecture-flow.md` (Section 1) |
| (NEW) | JWT Claims parsing | → `jwt-claims-authorization.md` |
| (NEW) | RBAC/PBAC implementation | → `jwt-claims-authorization.md` |

---

## 📊 So sánh trước/sau

### Trước (4 files, nhiều trùng lặp)

```
docs/auth/
├── keycloak-permissions.md    (732 lines)
│   ├── Client Scope là gì? (trùng với keycloak_setup.md)
│   ├── BFF Pattern overview (trùng với auth.md, bff_flow.md)
│   ├── Khi nào dùng Realm Role/Client Role/User Attribute
│   └── Setup permissions
│
├── keycloak_setup.md          (829 lines)
│   ├── BFF Architecture overview (trùng với auth.md, bff_flow.md)
│   ├── Security Benefits (trùng)
│   ├── Setup Keycloak
│   └── Testing flow
│
├── bff_flow.md                (669 lines)
│   ├── BFF Architecture (trùng)
│   ├── Login flow chi tiết
│   ├── API call flow
│   └── Redis structure
│
└── auth.md                    (367 lines)
    ├── BFF vs SPA comparison (trùng với keycloak_setup.md)
    ├── Security benefits (trùng)
    └── Diagrams (trùng với bff_flow.md)
```

**Issues:**
- ❌ Thông tin trùng lặp (BFF overview xuất hiện 4 lần)
- ❌ Không có document về JWT claims parsing
- ❌ Không có document về RBAC/PBAC implementation
- ❌ Khó tìm information cụ thể
- ❌ Không có entry point rõ ràng

### Sau (4 files, không trùng lặp, có structure)

```
docs/auth/
├── README.md                          (Entry point + Quick reference)
│   ├── Overview tất cả docs
│   ├── Quick Start Guide
│   ├── Common Tasks
│   └── FAQ
│
├── keycloak-complete-guide.md         (Keycloak setup + Permissions)
│   ├── Tổng quan Architecture (1 lần)
│   ├── Keycloak Setup (consolidated)
│   ├── Permissions Setup (consolidated)
│   └── Testing & Verification
│
├── jwt-claims-authorization.md        (JWT + Authorization - NEW!)
│   ├── JWT Token Structure
│   ├── Claims Parsing Flow (step-by-step)
│   ├── RBAC implementation
│   ├── PBAC implementation
│   ├── Hybrid Authorization
│   └── Code Examples + Debugging
│
└── bff-architecture-flow.md           (BFF Pattern + Flow)
    ├── BFF Pattern Overview (1 lần)
    ├── Architecture Components
    ├── Complete Authentication Flow
    ├── API Call Flow
    └── Security Features
```

**Benefits:**
- ✅ Không còn thông tin trùng lặp
- ✅ Mỗi topic có document riêng
- ✅ README.md làm entry point
- ✅ JWT claims parsing được document chi tiết
- ✅ RBAC/PBAC implementation có code examples
- ✅ Dễ dàng tìm kiếm và navigate

---

## 🎯 Cấu trúc JSON Claims (Chuẩn hóa)

### JWT Token Structure (Keycloak)

```json
{
  "exp": 1699095600,
  "iat": 1699095300,
  "iss": "http://localhost:8080/realms/base-realm",
  "aud": "api-gateway",
  "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "typ": "Bearer",
  "scope": "openid profile email",
  
  "preferred_username": "testuser",
  "email": "testuser@example.com",
  
  "realm_access": {
    "roles": ["admin", "user", "manager"]
  },
  
  "resource_access": {
    "api-gateway": {
      "roles": ["api-admin", "api-user"]
    }
  },
  
  "permissions": "product:view product:create category:view",
  
  "department": "Sales",
  "region": "Hanoi"
}
```

### UserClaimsContext Structure (Application)

```csharp
public class UserClaimsContext
{
    public string UserId { get; set; }           // từ "sub"
    public List<string> Roles { get; set; }      // từ realm_access + resource_access
    public Dictionary<string, string> Claims { get; set; }  // tất cả claims
    public List<string> Permissions { get; set; }  // từ "permissions" (space-separated)
    public Dictionary<string, object> CustomAttributes { get; set; }  // department, region, etc.
}
```

### Parsing Flow (Unified)

```
JWT Token (Keycloak)
    ↓
[JwtBearerAuthentication validates signature]
    ↓
[KeycloakAuthenticationExtensions.MapKeycloakRoles()]
    - Extract realm_access.roles → Add to ClaimTypes.Role
    - Extract resource_access.{client}.roles → Add to ClaimTypes.Role
    - Extract "scope" → Add to "permissions" claim
    ↓
ClaimsPrincipal with all claims
    ↓
[ClaimsPrincipalExtensions.ToUserClaimsContext()]
    - Extract UserId from "sub"
    - Collect Roles from ClaimTypes.Role
    - Parse Permissions from "permissions" (split by space)
    - Extract CustomAttributes (department, region, etc.)
    ↓
UserClaimsContext { UserId, Roles, Permissions, Claims, CustomAttributes }
    ↓
Authorization Check (RBAC/PBAC/Hybrid)
```

---

## ✅ What's Improved

### 1. Clear Documentation Structure

**Before:** 4 files with overlapping content
**After:** 4 files with distinct purposes + README entry point

### 2. No More Duplicate Information

**Before:** BFF Pattern overview repeated 4 times
**After:** BFF overview in 1 place (bff-architecture-flow.md), referenced from README

### 3. JWT Claims Parsing Documented

**Before:** Không có document chi tiết về cách parse JWT claims
**After:** jwt-claims-authorization.md với step-by-step flow và code

### 4. RBAC/PBAC Implementation Guide

**Before:** Chỉ có mention về policies, không có implementation guide
**After:** Chi tiết RBAC, PBAC, Hybrid với code examples

### 5. Unified JSON Structure

**Before:** Không có document chuẩn về claims structure
**After:** Đã định nghĩa rõ JWT Token structure và UserClaimsContext mapping

### 6. Better Navigation

**Before:** Không biết bắt đầu từ đâu
**After:** README.md với clear table of contents và "When to read" guide

---

## 📚 Reading Order (Recommended)

### For New Developers

1. **Start:** `README.md` - Hiểu overview và architecture
2. **Setup:** `keycloak-complete-guide.md` - Setup Keycloak từ đầu
3. **Understanding Flow:** `bff-architecture-flow.md` - Hiểu authentication flow
4. **Implementing Auth:** `jwt-claims-authorization.md` - Implement authorization

### For Debugging

1. **Start:** `README.md` → FAQ section
2. **If Keycloak issue:** `keycloak-complete-guide.md` → Troubleshooting
3. **If JWT/Claims issue:** `jwt-claims-authorization.md` → Debugging
4. **If Flow issue:** `bff-architecture-flow.md` → Troubleshooting

### For Implementing Features

1. **Gateway RBAC:** `jwt-claims-authorization.md` → RBAC section
2. **Service PBAC:** `jwt-claims-authorization.md` → PBAC section
3. **Custom Policy:** `jwt-claims-authorization.md` → Code Examples

---

## 🔗 Quick Links

| Task | Document | Section |
|------|----------|---------|
| Setup Keycloak từ đầu | [keycloak-complete-guide.md](./keycloak-complete-guide.md) | Section 2 |
| Gán permissions cho user | [keycloak-complete-guide.md](./keycloak-complete-guide.md) | Section 3 |
| Hiểu JWT claims parsing | [jwt-claims-authorization.md](./jwt-claims-authorization.md) | Section 2 |
| Implement RBAC | [jwt-claims-authorization.md](./jwt-claims-authorization.md) | Section 3 |
| Implement PBAC | [jwt-claims-authorization.md](./jwt-claims-authorization.md) | Section 4 |
| Hiểu authentication flow | [bff-architecture-flow.md](./bff-architecture-flow.md) | Section 3 |
| Debug permissions | [jwt-claims-authorization.md](./jwt-claims-authorization.md) | Section 7 |
| Quick reference | [README.md](./README.md) | All |

---

**Migration completed:** November 7, 2025

