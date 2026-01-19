# JWT Authentication Flow - Deprecated

⚠️ **DEPRECATED**: Tài liệu này đã lỗi thời. Hệ thống hiện tại đã chuyển sang **JWT-only approach**.

**Xem tài liệu mới tại**: [jwt-authentication-flow.md](./jwt-authentication-flow.md)

---

## ⚠️ Thông báo quan trọng

Hệ thống authentication đã được **đơn giản hóa** từ session-based sang **JWT-only approach**:

### Thay đổi chính:
- ❌ **Không còn session management** 
- ❌ **Không còn cookie-based authentication**
- ✅ **Trả trực tiếp JWT tokens** từ callback
- ✅ **Cache user claims** thay vì session data
- ✅ **Frontend quản lý tokens** (localStorage/sessionStorage)

### Migration Guide:
1. **Frontend**: Store JWT tokens thay vì rely on cookies
2. **API calls**: Sử dụng `Authorization: Bearer <token>` header
3. **Token refresh**: Implement refresh token flow
4. **Logout**: Revoke tokens và clear local storage

---

## 📋 Legacy Documentation (For Reference Only)

Phần dưới đây là documentation cũ về JWT & Session Flow - chỉ để tham khảo. 

**KHÔNG SỬ DỤNG** cho implementation mới.
+---------------------+--------------------+
                      | (9) Xác thực JWT
                      |     Trả về dữ liệu được bảo vệ
                      v
+---------------------+--------------------+
|                 Frontend                 |
+------------------------------------------+
```

---

## 👣 Giải thích chi tiết các bước

### 1. Bắt đầu luồng đăng nhập (Bước 1-5)
1.  Người dùng click vào nút "Đăng nhập" trên ứng dụng Frontend.
2.  Frontend tạo ra một chuỗi ngẫu nhiên (`code_verifier`), băm nó để tạo ra `code_challenge`, sau đó chuyển hướng người dùng đến trang đăng nhập của Keycloak, gửi kèm `code_challenge`.
3.  Người dùng nhập thông tin đăng nhập của họ trên giao diện của Keycloak.
4.  Keycloak xác thực thông tin. Nếu thành công, Keycloak ghi nhận `code_challenge` đã được gửi lên.
5.  Keycloak chuyển hướng người dùng trở lại địa chỉ của Frontend (redirect URI), kèm theo một `authorization_code` sử dụng một lần.

### 2. Lấy Access Token (Bước 6-7)
6.  Frontend nhận được `authorization_code`. Ngay lập tức, nó gửi một request từ "hậu trường" (backend of frontend) đến **Token Endpoint** của Keycloak. Request này chứa `authorization_code` vừa nhận được và `code_verifier` (chuỗi ngẫu nhiên gốc ở bước 2).
7.  Keycloak nhận request, kiểm tra xem `authorization_code` có hợp lệ không, và quan trọng nhất là băm `code_verifier` để so sánh với `code_challenge` đã lưu ở bước 4. Nếu khớp, Keycloak chắc chắn rằng request này đến từ chính client đã bắt đầu luồng đăng nhập. Keycloak trả về một bộ tokens:
    *   **`access_token`**: Một JWT có thời gian sống ngắn (vài phút). Dùng để xác thực khi gọi API.
    *   **`refresh_token`**: Một token có thời gian sống dài (vài giờ hoặc vài ngày). Dùng để lấy `access_token` mới mà không cần người dùng đăng nhập lại.

### 3. Lưu trữ Token và gọi API (Bước 8)
-   **Lưu trữ**: Frontend cần lưu trữ các token này. **Khuyến nghị an toàn nhất cho SPA là lưu trong bộ nhớ (in-memory)**. Việc lưu vào `localStorage` có thể bị tấn công XSS.
-   **Gọi API**: Khi gọi một API cần bảo vệ, Frontend sẽ đính kèm `access_token` vào header `Authorization`.
    ```http
    GET /api/orders
    Host: your-api.com
    Authorization: Bearer <your_access_token>
    ```

### 4. Xác thực Token tại Backend (Bước 9)
Đây là nhiệm vụ của **API Gateway** hoặc **BFF**. Khi nhận được request, middleware xác thực của .NET (`AddJwtBearer`) sẽ tự động thực hiện các bước kiểm tra sau:
1.  **Kiểm tra Chữ ký (Signature)**: Dùng public key của Keycloak (lấy từ endpoint `.well-known/openid-configuration`) để xác minh rằng token này thực sự do Keycloak ký và không bị thay đổi.
2.  **Kiểm tra Thời gian hết hạn (Expiration)**: Đọc claim `exp` và so sánh với thời gian hiện tại để đảm bảo token chưa hết hạn.
3.  **Kiểm tra Nhà cung cấp (Issuer)**: Đọc claim `iss` và đảm bảo nó khớp với địa chỉ realm của Keycloak đã cấu hình.
4.  **Kiểm tra Đối tượng (Audience)**: Đọc claim `aud` và đảm bảo nó khớp với định danh của API/client đang được gọi.

Nếu tất cả các bước trên thành công, middleware sẽ tạo ra danh tính (`ClaimsPrincipal`) cho người dùng và request được tiếp tục xử lý. Nếu thất bại, nó sẽ trả về lỗi `401 Unauthorized`.

### 5. Quản lý Session (Tùy chọn, tại BFF)
Trong một số kiến trúc, đặc biệt là với các ứng dụng web truyền thống hơn là SPA, chúng ta không muốn để JWT token ở phía trình duyệt. Thay vào đó, ta dùng mô hình BFF (Backend-for-Frontend):
1.  BFF thực hiện toàn bộ luồng OAuth với Keycloak và nhận về bộ tokens.
2.  BFF lưu trữ bộ tokens này một cách an toàn (ví dụ: trong cache hoặc database).
3.  BFF tạo ra một session cookie truyền thống (HTTP-Only, Secure) và gửi nó về cho trình duyệt.
4.  Trình duyệt sẽ tự động gửi cookie này trong các request tiếp theo đến BFF. BFF sẽ dùng thông tin session để lấy JWT token tương ứng và gọi các microservice bên dưới.

Mô hình này tăng cường bảo mật bằng cách che giấu hoàn toàn token khỏi trình duyệt.

### 6. Làm mới Token (Token Refresh)
Khi `access_token` hết hạn, API sẽ trả về lỗi `401 Unauthorized`.
1.  Frontend bắt lỗi này.
2.  Nó sẽ dùng `refresh_token` đã lưu để gọi đến **Token Endpoint** của Keycloak (với `grant_type=refresh_token`).
3.  Nếu `refresh_token` hợp lệ, Keycloak sẽ cấp một bộ `access_token` và `refresh_token` mới.
4.  Frontend lưu lại bộ tokens mới và thực hiện lại request API đã thất bại trước đó.