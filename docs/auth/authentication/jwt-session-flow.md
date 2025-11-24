# JWT & Session Flow - Luồng Xác thực

Tài liệu này mô tả toàn bộ luồng hoạt động của quá trình xác thực, từ khi người dùng đăng nhập cho đến khi backend xác thực thành công và trả về dữ liệu được bảo vệ.

## 🎯 Tổng quan

Luồng xác thực của chúng ta sử dụng tiêu chuẩn **OAuth 2.0** và **OpenID Connect (OIDC)**, với **Keycloak** làm Identity Provider. Kết quả của quá trình xác thực là một **JSON Web Token (JWT)**, đóng vai trò như một "giấy thông hành" mà client sử dụng để chứng minh danh tính khi gọi các API.

**Các thành phần tham gia:**
-   **User**: Người dùng cuối.
-   **Client (Browser/Frontend)**: Ứng dụng web mà người dùng tương tác (e.g., React, Angular).
-   **Keycloak**: Máy chủ xác thực, chịu trách nhiệm xác minh danh tính người dùng và cấp token.
-   **Backend API (BFF/API Gateway)**: Điểm cuối của hệ thống, nơi tiếp nhận và xác thực token.

## 🌊 Sơ đồ luồng (Authorization Code Flow with PKCE)

Đây là luồng được khuyến nghị cho các ứng dụng web và SPA vì tính bảo mật cao.

```
+--------+   (1) Bấm nút "Đăng nhập"   +----------+
|  User  | -------------------------> | Frontend |
+--------+                            +----+-----+
                                           | (2) Tạo code_verifier, code_challenge
                                           |     Redirect đến Keycloak với code_challenge
                                           v
+--------+   (3) Nhập username/password   +----------+
|  User  | -----------------------------> | Keycloak |
+--------+   (4) Xác thực thành công      +----+-----+
                                                | (5) Redirect về Frontend với "authorization_code"
                                                v
+------------------------------------------+---+
|                 Frontend                 |
+---------------------+--------------------+
                      | (6) Gửi "authorization_code" + "code_verifier"
                      |     đến Keycloak Token Endpoint
                      v
+---------------------+--------------------+
|                    Keycloak              |
+---------------------+--------------------+
                      | (7) Xác minh code & verifier
                      |     Trả về Access Token (JWT) + Refresh Token
                      v
+---------------------+--------------------+
|                 Frontend                 |
+---------------------+--------------------+
                      | (8) Lưu trữ Tokens
                      |     Gọi API Backend với Access Token
                      v
+---------------------+--------------------+
|               Backend API                |
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