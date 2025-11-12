# Auth Service

Authentication service xử lý OAuth 2.0 / OpenID Connect với Keycloak.

## Mục đích

Auth service tách logic authentication ra khỏi API Gateway, tuân thủ nguyên tắc Single Responsibility Principle. Gateway chỉ làm nhiệm vụ routing, còn authentication logic được xử lý bởi Auth service.

## Kiến trúc

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Browser   │─────▶│ API Gateway  │─────▶│ Auth Service│
└─────────────┘      └──────────────┘      └─────────────┘
                            │                       │
                            │                       │
                            ▼                       ▼
                     ┌──────────────┐      ┌─────────────┐
                     │  Downstream  │      │  Keycloak   │
                     │   Services   │      │   + Redis   │
                     └──────────────┘      └─────────────┘
```

## Chức năng

### 1. Login Flow (OAuth 2.0 Authorization Code + PKCE)
- **POST /api/auth/login/initiate**: Tạo PKCE data, lưu vào Redis, trả về authorization URL
- Browser redirect tới Keycloak login
- User đăng nhập tại Keycloak
- Keycloak callback về Gateway với authorization code
- **POST /api/auth/login/callback**: Validate state, exchange code để lấy tokens, tạo session

### 2. Session Management
- **GET /api/auth/session/{sessionId}/validate**: Validate session và trả về access token
- Tự động refresh token nếu gần hết hạn
- Session được lưu trong Redis với sliding + absolute expiration

### 3. User Info
- **GET /api/auth/user/{sessionId}**: Lấy thông tin user từ session
- Parse JWT claims để extract user info, roles

### 4. Logout
- **POST /api/auth/logout**: Revoke tokens tại Keycloak, xóa session khỏi Redis

## Cấu hình

### appsettings.json

```json
{
  "Auth": {
    "ConnectionStrings": "localhost:6379",
    "InstanceName": "Auth_",
    "SessionSlidingExpirationMinutes": 60,
    "SessionAbsoluteExpirationMinutes": 480,
    "PkceExpirationMinutes": 10,
    "RefreshTokenBeforeExpirationSeconds": 60
  },
  "OAuth": {
    "Authority": "http://localhost:8080/realms/base-realm",
    "ClientId": "base-client",
    "ClientSecret": "UWP2C8XceQzG6rvdKZd0yuYfTkeisLLV",
    "RedirectUri": "http://localhost:5000/auth/signin-oidc",
    "Scopes": ["openid", "profile", "email"],
    "TokenEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/token",
    "AuthorizationEndpoint": "http://localhost:8080/realms/base-realm/protocol/openid-connect/auth"
  }
}
```

## Chạy service

### Development
```bash
cd src/Services/Auth/Auth.API
dotnet run
```

Service chạy tại: `http://localhost:5100`

### Docker
```bash
docker-compose -f infra/services/auth-api.yml up -d
```

## Dependencies

- **Redis**: Lưu trữ sessions và PKCE data
- **Keycloak**: Identity Provider (OAuth 2.0 / OIDC)

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/login/initiate` | POST | Khởi tạo login flow |
| `/api/auth/login/callback` | POST | Xử lý callback từ Keycloak |
| `/api/auth/logout` | POST | Logout user |
| `/api/auth/user/{sessionId}` | GET | Lấy thông tin user |
| `/api/auth/session/{sessionId}/validate` | GET | Validate session |
| `/api/auth/health` | GET | Health check |

## Security Features

- **PKCE (Proof Key for Code Exchange)**: Bảo vệ authorization code flow
- **State Parameter**: CSRF protection
- **HttpOnly Cookies**: Session ID không thể access từ JavaScript
- **Token Storage**: Tokens được lưu server-side (Redis), không expose ra browser
- **Automatic Token Refresh**: Tự động refresh token khi gần hết hạn
- **Session Expiration**: Sliding + Absolute expiration

## Integration với API Gateway

Gateway gọi Auth service thông qua HTTP:

1. **Login**: Gateway forward request tới Auth service
2. **Callback**: Gateway nhận authorization code, forward tới Auth service để exchange tokens
3. **Session Validation**: Middleware của Gateway gọi Auth service để validate session mỗi request
4. **Token Injection**: Gateway nhận access token từ Auth service, inject vào downstream requests

## Cấu trúc thư mục

```
Auth/
├── Auth.API/              # Web API layer
│   ├── Controllers/       # API controllers
│   ├── Program.cs         # Service configuration
│   └── appsettings.json   # Configuration
├── Auth.Application/      # Application layer
│   ├── DTOs/              # Data transfer objects
│   └── Interfaces/        # Service interfaces
├── Auth.Domain/           # Domain layer
│   ├── Models/            # Domain models
│   └── Configurations/    # Configuration classes
└── Auth.Infrastructure/   # Infrastructure layer
    └── Services/          # Service implementations
```

## Lưu ý

1. **Client Secret**: Cần cấu hình đúng client secret từ Keycloak
2. **Redis Connection**: Đảm bảo Redis đang chạy
3. **Keycloak URLs**: Cập nhật URLs phù hợp với môi trường
4. **CORS**: Cấu hình CORS nếu Gateway và Auth service khác domain
