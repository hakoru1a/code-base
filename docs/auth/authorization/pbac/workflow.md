# PBAC Workflow

## 🔄 Luồng xử lý PBAC

Sơ đồ dưới đây mô tả luồng xử lý của một request từ khi được gửi từ client cho đến khi được Policy-Based Access Control (PBAC) xử lý.

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Client gửi request + JWT Token                          │
│    (Token chứa thông tin user, roles, và các claims khác)   │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. Authentication Middleware                                │
│    - Validate JWT token (signature, expiration, issuer).    │
│    - Nếu token hợp lệ -> Trích xuất claims và tạo UserIdentity.│
│    - Gán UserIdentity vào HttpContext.                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. PolicyAuthorizationMiddleware                            │
│    - Kiểm tra sự tồn tại của attribute [RequirePolicy].      │
│    - Lấy policy name (e.g., "PRODUCT:VIEW").                │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. PolicyEvaluator                                          │
│    - Dựa vào policy name, tìm policy tương ứng trong Registry.│
│    - Lấy instance của policy từ Dependency Injection.         │
│    - Chuẩn bị UserClaimsContext và context data.            │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. YourPolicy.EvaluateAsync(user, context)                  │
│    - **Đây là nơi logic phân quyền của bạn được thực thi.**    │
│    - Kiểm tra business logic (dựa trên role, permission, claim, context data).│
│    - Trả về PolicyEvaluationResult.Allow() hoặc .Deny().     │
│    - (Nâng cao) Có thể trả về FilterContext để lọc dữ liệu.   │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
         ┌─────────────┴─────────────┐
         │                           │
         ▼                           ▼
    ✅ Allow                    ❌ Deny
    - Request tiếp tục vào         - Middleware chặn request.
      Controller Action.           - Trả về 403 Forbidden.
    - (Nâng cao) FilterContext     - Response chứa lý do từ chối.
      được lưu vào HttpContext.
                                 {
                                   "error": "Forbidden",
                                   "message": "Lý do từ chối...",
                                   "policy": "PRODUCT:VIEW"
                                 }
```

## Token Claims và Data Flow

Để workflow trên hoạt động, token JWT phải chứa các claims cần thiết cho việc phân quyền.

### Ví dụ về Token Claims

Một token JWT sau khi được giải mã có thể có payload như sau. Các claims này được Keycloak hoặc một Identity Provider khác thêm vào lúc user đăng nhập.

```json
{
  "sub": "a1b2c3d4-e5f6-7890-1234-56789abcdef0", // User ID
  "realm_access": {
    "roles": [
      "sales_rep",
      "premium_user"
    ]
  },
  "permissions": [
    "category:view:electronics",
    "category:view:books"
  ],
  "department": "sales",
  "max_product_price": "50000000",
  "iss": "https://your-keycloak-instance/auth/realms/your-realm",
  "exp": 1672531199
}
```

### Dữ liệu được truyền vào Policy như thế nào?

`PolicyAuthorizationMiddleware` sẽ đọc các claims từ token và đóng gói chúng vào `UserClaimsContext`:

```csharp
// Dữ liệu được tạo bởi Middleware và truyền vào hàm EvaluateAsync
var userClaimsContext = new UserClaimsContext
{
    UserId = "a1b2c3d4-e5f6-7890-1234-56789abcdef0",
    Roles = new List<string> { "sales_rep", "premium_user" },
    Permissions = new List<string> { "category:view:electronics", "category:view:books" },
    Claims = new Dictionary<string, string>
    {
        { "max_product_price", "50000000" }
        // các claims gốc khác từ token
    },
    CustomAttributes = new Dictionary<string, object>
    {
        { "department", "sales" }
    }
};

// Sau đó, hàm EvaluateAsync của bạn sẽ được gọi
YourPolicy.EvaluateAsync(userClaimsContext, context);
```

Bằng cách này, policy của bạn có đầy đủ thông tin về người dùng và các quyền hạn của họ để đưa ra quyết định `Allow` hay `Deny`.
