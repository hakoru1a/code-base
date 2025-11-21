# Authorization Documentation Index

## 📚 Danh mục tài liệu

### 🎯 PBAC (Policy-Based Access Control) - **MỚI**

| Tài liệu | Mô tả | Link |
|----------|-------|------|
| **PBAC Guide** | Hướng dẫn đầy đủ: Cách sử dụng, Workflow, Implement | [📖 Xem](./pbac-guide.md) |
| **PBAC Cheat Sheet** | Tài liệu tham khảo nhanh với templates | [⚡ Xem](./pbac-cheatsheet.md) |
| **Refactor Summary** | Chi tiết về việc refactor PBAC | [📋 Xem](../../PBAC_REFACTOR_SUMMARY.md) |

### 🔐 JWT & Authentication

| Tài liệu | Mô tả | Link |
|----------|-------|------|
| **JWT Claims Authorization** | RBAC, PBAC, Hybrid policies | [📖 Xem](./jwt-claims-authorization.md) |
| **Keycloak Complete Guide** | Hướng dẫn setup Keycloak | [📖 Xem](./keycloak-complete-guide.md) |
| **Keycloak Setup** | Quick setup guide | [📖 Xem](../../KEYCLOAK_SETUP.md) |

### 🏗️ Architecture

| Tài liệu | Mô tả | Link |
|----------|-------|------|
| **BFF Architecture Flow** | Backend-for-Frontend architecture | [📖 Xem](./bff-architecture-flow.md) |

---

## 🚀 Quick Start

### Tôi muốn...

#### ✅ Tạo một policy mới
→ [PBAC Guide - Implement Policy mới](./pbac-guide.md#implement-policy-mới)  
→ [PBAC Cheat Sheet - Templates](./pbac-cheatsheet.md#templates)

#### ✅ Sử dụng policy trong controller
→ [PBAC Guide - Cách sử dụng](./pbac-guide.md#cách-sử-dụng)  
→ [PBAC Cheat Sheet - Usage in Controllers](./pbac-cheatsheet.md#usage-in-controllers)

#### ✅ Hiểu workflow của PBAC
→ [PBAC Guide - Workflow](./pbac-guide.md#workflow)  
→ [PBAC Cheat Sheet - Request Flow](./pbac-cheatsheet.md#request-flow-simple)

#### ✅ Setup Keycloak
→ [Keycloak Complete Guide](./keycloak-complete-guide.md)  
→ [Keycloak Setup](../../KEYCLOAK_SETUP.md)

#### ✅ Hiểu về JWT claims và roles
→ [JWT Claims Authorization](./jwt-claims-authorization.md)

---

## 📖 Tài liệu theo level

### 🟢 Beginner
1. [PBAC Cheat Sheet](./pbac-cheatsheet.md) - Bắt đầu ở đây!
2. [PBAC Guide - Cách sử dụng](./pbac-guide.md#cách-sử-dụng)
3. [Keycloak Setup](../../KEYCLOAK_SETUP.md)

### 🟡 Intermediate
1. [PBAC Guide - Workflow](./pbac-guide.md#workflow)
2. [PBAC Guide - Examples](./pbac-guide.md#ví-dụ-thực-tế)
3. [JWT Claims Authorization](./jwt-claims-authorization.md)

### 🔴 Advanced
1. [Keycloak Complete Guide](./keycloak-complete-guide.md)
2. [BFF Architecture Flow](./bff-architecture-flow.md)
3. [Refactor Summary](../../PBAC_REFACTOR_SUMMARY.md)

---

## 🎯 Use Cases

### 👤 User Management
- [JWT Claims Authorization - RBAC](./jwt-claims-authorization.md#rbac)
- [Keycloak - User Roles](./keycloak-complete-guide.md)

### 🛡️ Authorization Logic
- [PBAC Guide - Full Guide](./pbac-guide.md)
- [PBAC Cheat Sheet - Templates](./pbac-cheatsheet.md#templates)

### 🔧 Setup & Configuration
- [Keycloak Setup](../../KEYCLOAK_SETUP.md)
- [Keycloak Complete Guide](./keycloak-complete-guide.md)

### 🏗️ Architecture
- [BFF Architecture Flow](./bff-architecture-flow.md)
- [JWT Claims Authorization](./jwt-claims-authorization.md)

---

## 🔍 Search by Topic

### Authentication
- [Keycloak Setup](../../KEYCLOAK_SETUP.md)
- [Keycloak Complete Guide](./keycloak-complete-guide.md)
- [JWT Claims Authorization](./jwt-claims-authorization.md)

### Authorization (PBAC)
- [PBAC Guide](./pbac-guide.md) ⭐ **Recommended**
- [PBAC Cheat Sheet](./pbac-cheatsheet.md)
- [Refactor Summary](../../PBAC_REFACTOR_SUMMARY.md)

### Roles & Permissions
- [JWT Claims Authorization - RBAC](./jwt-claims-authorization.md#rbac)
- [PBAC Guide - Helper Methods](./pbac-guide.md#helper-methods-trong-basepolicy)

### Policies
- [PBAC Guide - Implement Policy mới](./pbac-guide.md#implement-policy-mới)
- [PBAC Cheat Sheet - Templates](./pbac-cheatsheet.md#templates)
- [PBAC Guide - Examples](./pbac-guide.md#ví-dụ-thực-tế)

---

## 📝 Code Examples

### Quick Examples

#### Tạo Policy mới
```csharp
[Policy("INVOICE:VIEW")]
public class InvoiceViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(...)
    {
        if (IsAuthenticated(user))
            return Task.FromResult(PolicyEvaluationResult.Allow("OK"));
        return Task.FromResult(PolicyEvaluationResult.Deny("Denied"));
    }
}
```

#### Sử dụng trong Controller
```csharp
[RequirePolicy("INVOICE:VIEW")]
public async Task<IActionResult> GetInvoice(long id) { }
```

### More Examples
- [PBAC Guide - Ví dụ thực tế](./pbac-guide.md#ví-dụ-thực-tế)
- [PBAC Cheat Sheet - Templates](./pbac-cheatsheet.md#templates)
- [PBAC Cheat Sheet - Common Patterns](./pbac-cheatsheet.md#common-patterns)

---

## 🆕 What's New?

### PBAC System - Re-implemented từ đầu ✨

**Những gì mới:**
- ✅ Attribute-based registration với `[Policy]`
- ✅ Auto-discovery policies
- ✅ Simplified BasePolicy với helper methods
- ✅ Giảm 350+ dòng code
- ✅ Không cần manual registration

**Đọc thêm:**
- [Refactor Summary](../../PBAC_REFACTOR_SUMMARY.md)
- [PBAC Guide](./pbac-guide.md)

---

## 🤝 Contributing

Khi thêm tài liệu mới, hãy update file này để maintain index!

---

## 📞 Support

Nếu có thắc mắc:
1. Check [PBAC Cheat Sheet](./pbac-cheatsheet.md) trước
2. Đọc [PBAC Guide](./pbac-guide.md)
3. Check [Troubleshooting](./pbac-guide.md#troubleshooting)

---

**Last Updated:** November 2024

