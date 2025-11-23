# RBAC Workflow - Luồng hoạt động

Tài liệu này giải thích chi tiết về cách một "vai trò" (role) được định nghĩa trong Keycloak được truyền đến và sử dụng bởi ứng dụng .NET của chúng ta để ra quyết định phân quyền.

## 🌊 Sơ đồ luồng hoạt động

```
+---------------+   (1) Đăng nhập   +-----------------+   (2) Cấp JWT Token   +---------------------+
|     User      | ----------------> |     Keycloak    | ------------------> |   Client (Browser)  |
+---------------+                   +-----------------+                     +----------+----------+
                                              |                                        |
                               (Token chứa claim "roles")                               | (3) Gửi Request
                                                                                       |   (với JWT Token)
                                                                                       v
                                          +------------------------------------------------------+
                                          |                    Backend API (.NET)                    |
                                          +--------------------------+-----------------------------+
                                                                     |
                                                       (4) Authentication Middleware
                                                                     |
                                  +------------------------------------------------------------------+
                                  | - Xác thực JWT (chữ ký, issuer, expiration).                       |
                                  | - Đọc claim `realm_access.roles`.                                  |
                                  | - Map các roles này vào ClaimsPrincipal.                          |
                                  +----------------------------------+-------------------------------+
                                                                     |
                                                                     v
                                                       (5) Authorization Middleware
                                                                     |
                                     +---------------------------------------------------------------+
                                     | - Kiểm tra `[Authorize(Roles = "...")]`.                       |
                                     | - So sánh role trong attribute với role của user.               |
                                     +--------------------------------+------------------------------+
                                                                      |
                                                                      v
                                                         +-------------------------+
                                                         |  Allow / Deny Request   |
                                                         +-------------------------+
```

---

## 👣 Giải thích chi tiết các bước

### Bước 1 & 2: Keycloak cấp JWT Token chứa Roles

Khi người dùng đăng nhập thành công qua Keycloak, Keycloak sẽ tạo một JWT Token. Bên trong payload của token này có một claim đặc biệt là `realm_access`, chứa các vai trò (Realm Roles) mà người dùng đó đã được gán.

**Ví dụ về payload của một JWT Token:**
```json
{
  "sub": "a1b2c3d4-e5f6-7890-1234-56789abcdef0",
  "name": "Alice",
  "realm_access": {
    "roles": [
      "default-roles-myrealm", // Một vai trò mặc định
      "ProductManager",        // Vai trò nghiệp vụ
      "user"
    ]
  },
  "iss": "https://your-keycloak-instance/auth/realms/your-realm"
}
```
Trong ví dụ này, `realm_access.roles` là một mảng chứa tất cả các vai trò của "Alice", bao gồm cả vai trò `ProductManager`.

### Bước 3 & 4: Backend API nhận và xử lý Token

Khi client gửi request đến API kèm theo JWT token trong header `Authorization`, **ASP.NET Core Authentication Middleware** sẽ thực hiện các công việc sau:

1.  **Xác thực Token**: Middleware kiểm tra xem token có hợp lệ không (chữ ký đúng, chưa hết hạn, đúng nhà cung cấp...).

2.  **Đọc và Map Claims (Bước quan trọng)**:
    *   Mặc định, .NET tìm kiếm các claim có type là `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` để nhận diện vai trò.
    *   Tuy nhiên, Keycloak lại đặt vai trò trong `realm_access.roles`.
    *   Do đó, chúng ta cần một bước "mapping" để "dạy" cho .NET cách đọc role từ đúng chỗ. Đoạn code này thường nằm trong cấu hình `AddJwtBearer` ở `Program.cs` hoặc `Startup.cs`.

    **Ví dụ về code mapping claims:**
    ```csharp
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // ... các cấu hình khác như Authority, Audience ...

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Đây là nơi chúng ta can thiệp
                    if (context.Principal.Identity is ClaimsIdentity claimsIdentity &&
                        context.Principal.HasClaim(c => c.Type == "realm_access"))
                    {
                        var realmAccessClaim = context.Principal.Claims
                            .FirstOrDefault(c => c.Type == "realm_access");
                            
                        if (realmAccessClaim != null)
                        {
                            var realmAccess = JObject.Parse(realmAccessClaim.Value);
                            var roles = realmAccess["roles"];
                            
                            if (roles != null)
                            {
                                foreach (var role in roles)
                                {
                                    // Thêm mỗi role từ Keycloak như một Role Claim của .NET
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                                }
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        });
    ```
    Nhờ đoạn code trên, ứng dụng .NET giờ đây đã "hiểu" được các vai trò đến từ Keycloak.

### Bước 5: Authorization Middleware ra quyết định

Sau khi quá trình xác thực và mapping hoàn tất, `ClaimsPrincipal` (đối tượng đại diện cho user, truy cập qua `HttpContext.User`) đã chứa các role claim chính xác.

Bây giờ, khi một request đi đến một endpoint có attribute `[Authorize(Roles = "ProductManager")]`:
1.  **Authorization Middleware** sẽ được kích hoạt.
2.  Nó sẽ kiểm tra `ClaimsPrincipal` hiện tại.
3.  Nó tìm xem `ClaimsPrincipal` có chứa một `Claim` với `Type` là `ClaimTypes.Role` và `Value` là `ProductManager` hay không.
4.  Nếu có, request được cho phép (Allow) và đi tiếp vào action của controller.
5.  Nếu không, middleware sẽ chặn request và trả về lỗi `403 Forbidden`.

Quá trình này cũng tương tự khi bạn gọi `User.IsInRole("ProductManager")` trong code, nó cũng thực hiện việc kiểm tra các role claim trong `ClaimsPrincipal`.