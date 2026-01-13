# Mục lục tài liệu về Xác thực & Phân quyền (Authentication & Authorization)

Chào mừng bạn đến với tài liệu hướng dẫn về các hệ thống xác thực và phân quyền. Tài liệu này cung cấp thông tin chi tiết về cách người dùng được xác định và cách quyền truy cập tài nguyên được quản lý trong hệ thống của chúng ta.

---

## 🚀 Quick Start

Bắt đầu nhanh với authentication tại API Gateway:
*   **Quick Start Guide**: Hướng dẫn thiết lập và sử dụng authentication trong 10 phút.
    *   [QUICK-START.md](./QUICK-START.md)

---

## 🔑 Xác thực (Authentication)

Authentication hiện được xử lý **trực tiếp tại API Gateway** theo kiến trúc BFF (Backend-for-Frontend) đơn giản hóa.

*   **Gateway Authentication Flow**: Hướng dẫn chi tiết về authentication flow mới tại API Gateway.
    *   [GATEWAY-AUTH-FLOW.md](./GATEWAY-AUTH-FLOW.md)
*   **Keycloak Guide**: Hướng dẫn chi tiết về cách tích hợp và sử dụng Keycloak làm Identity Provider.
    *   [keycloak-guide.md](./authentication/keycloak-guide.md)
*   **JWT & Session Flow**: Mô tả luồng xử lý JWT (JSON Web Token) và quản lý phiên (session) trong hệ thống.
    *   [jwt-session-flow.md](./authentication/jwt-session-flow.md)

---

## 🔒 Phân quyền (Authorization)

Phần này tập trung vào việc quản lý quyền truy cập của người dùng đến các tài nguyên và hành động khác nhau.

### 🛡️ Policy-Based Access Control (PBAC)

PBAC là một mô hình phân quyền mạnh mẽ, cho phép định nghĩa các chính sách linh hoạt dựa trên nhiều thuộc tính.

*   **PBAC Quick Start**: Hướng dẫn nhanh để bắt đầu với PBAC, bao gồm ví dụ cơ bản và cách sử dụng FilterContext.
    *   [quick_start.md](./authorization/pbac/quick_start.md)
*   **PBAC Workflow**: Giải thích chi tiết luồng xử lý của PBAC, từ request đến quyết định phân quyền.
    *   [workflow.md](./authorization/pbac/workflow.md)
*   **PBAC Guide**: Hướng dẫn tổng quan và chuyên sâu về PBAC, các ví dụ thực tế và best practices.
    *   [guide.md](./authorization/pbac/guide.md)

### 👥 Role-Based Access Control (RBAC)

RBAC là mô hình phân quyền dựa trên vai trò, nơi quyền hạn được gán cho các vai trò, và người dùng được gán các vai trò đó.

*   **RBAC Quick Start**: Hướng dẫn nhanh để bắt đầu với RBAC.
    *   [quick_start.md](./authorization/rbac/quick_start.md)
*   **RBAC Workflow**: Giải thích chi tiết luồng xử lý của RBAC.
    *   [workflow.md](./authorization/rbac/workflow.md)
*   **RBAC Guide**: Hướng dẫn tổng quan về RBAC.
    *   [guide.md](./authorization/rbac/guide.md)

### 📊 JWT Claims cho Phân quyền

Tài liệu này giải thích cách sử dụng các claims trong JWT để đưa ra các quyết định phân quyền.

*   **JWT Claims**: Cách sử dụng các claims trong JWT để phân quyền.
    *   [jwt-claims.md](./authorization/jwt-claims.md)

---

## 🏛️ Kiến trúc (Architecture)

Phần này bao gồm các tài liệu mô tả kiến trúc liên quan đến hệ thống xác thực và phân quyền.

*   **BFF Architecture Flow**: Giải thích luồng hoạt động của kiến trúc Backend-for-Frontend (BFF) với authentication tại API Gateway.
    *   [bff-flow.md](./architecture/bff-flow.md)

---

## ⚡ Tóm tắt thay đổi kiến trúc

### ❌ Kiến trúc cũ (Phức tạp)
```
Browser → Gateway → Auth Service → Keycloak
                       ↓
                     Redis
```
**Vấn đề:** Nhiều network hops, phức tạp, Auth Service là single point of failure

### ✅ Kiến trúc mới (Đơn giản)
```
Browser → Gateway → Keycloak
            ↓
          Redis
```
**Lợi ích:** 
- Giảm latency
- Đơn giản hóa architecture  
- Dễ maintain
- Vẫn giữ security
- Tiết kiệm resources

---

## 🔒 Cải tiến Bảo mật

*   **Security Improvements**: Tài liệu chi tiết về các cải tiến bảo mật đã thực hiện
    *   [security-improvements.md](./security-improvements.md)
*   **Troubleshooting**: Hướng dẫn khắc phục sự cố liên quan đến cải tiến bảo mật
    *   [troubleshooting-security-improvements.md](./troubleshooting-security-improvements.md)

---

## 📚 Tài liệu liên quan

*   **API Gateway README**: Chi tiết về cấu hình và sử dụng API Gateway
    *   [src/ApiGateways/ApiGateway/README.md](../../src/ApiGateways/ApiGateway/README.md)
