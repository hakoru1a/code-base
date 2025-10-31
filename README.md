# CodeBase - Enterprise .NET 9 Application

## 📋 Tổng quan

Đây là một ứng dụng enterprise được xây dựng với .NET 9, tuân thủ **Clean Architecture** và các design patterns hiện đại. Ứng dụng được thiết kế để hỗ trợ đa database, caching với Redis, logging toàn diện và xử lý lỗi chuyên nghiệp.

## 🏗️ Kiến trúc tổng thể

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Base.API (Presentation)                  │
│  • Controllers                                              │
│  • API Endpoints                                           │
│  • Swagger Documentation                                   │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                 Base.Application (Application)              │
│  • CQRS Commands & Queries                                  │
│  • MediatR Handlers                                         │
│  • AutoMapper Profiles                                      │
│  • FluentValidation                                         │
│  • Pipeline Behaviors                                       │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   Base.Domain (Domain)                      │
│  • Entities                                                 │
│  • Domain Events                                            │
│  • Business Logic                                           │
│  • Repository Interfaces                                    │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│               Base.Infrastructure (Infrastructure)          │
│  • Repository Implementations                               │
│  • Database Context                                         │
│  • External Service Integrations                           │
└─────────────────────────────────────────────────────────────┘
```

### Shared Libraries

- **Contracts**: Common interfaces, events, exceptions
- **Infrastructure**: Cross-cutting concerns, database providers
- **Logging**: Centralized logging with Serilog
- **Shared**: Common utilities, configurations, DTOs

## 🎯 Design Patterns được sử dụng

### 1. **Clean Architecture**
- **Separation of Concerns**: Mỗi layer có trách nhiệm riêng biệt
- **Dependency Inversion**: Domain không phụ thuộc vào Infrastructure
- **Independence**: Có thể thay đổi database, UI mà không ảnh hưởng business logic

### 2. **CQRS (Command Query Responsibility Segregation)**
```csharp
// Commands - Thay đổi dữ liệu
public class CreateProductCommand : IRequest<long>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string SKU { get; set; }
}

// Queries - Đọc dữ liệu
public class GetProductsQuery : IRequest<List<ProductDto>>
{
    // Query parameters
}
```

### 3. **MediatR Pattern**
- **Decoupling**: Controllers không gọi trực tiếp services
- **Pipeline Behaviors**: Validation, Logging, Performance monitoring
- **Request/Response**: Type-safe communication

### 4. **Repository Pattern**
```csharp
public interface IProductRepository : IRepositoryBaseAsync<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<List<Product>> GetLowStockProductsAsync(int threshold);
}
```

### 5. **Unit of Work Pattern**
- **Transaction Management**: Đảm bảo consistency
- **Change Tracking**: Quản lý entity states
- **Domain Events**: Publish events sau khi save changes

### 6. **Domain Events Pattern**
```csharp
public class Product : AuditableEventEntity<long>
{
    public static Product Create(string name, string description, decimal price, int stock, string sku)
    {
        var product = new Product(name, description, price, stock, sku);
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.SKU, product.Price, product.Stock));
        return product;
    }
}
```

### 7. **Event-Driven Architecture với Mediator & MassTransit**
Ứng dụng hỗ trợ cả **Mediator** (in-memory) và **MassTransit** (distributed messaging) để xử lý events:

#### **Mediator Pattern** - Xử lý Domain Events trong cùng Application
```csharp
// Trong Service/Controller
public class OrderService
{
    private readonly IMediator _mediator;
    
    public async Task ProcessOrder(Order order)
    {
        // Xử lý domain events trong cùng application
        var domainEvent = new OrderCreatedEvent { OrderId = order.Id };
        await _mediator.Publish(domainEvent);
    }
}
```

#### **MassTransit Pattern** - Gửi Messages đến External Services
```csharp
// Trong Service/Controller
public class NotificationService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    
    public async Task SendNotification(NotificationRequest request)
    {
        // Publish event đến tất cả consumers
        await _publishEndpoint.Publish(new NotificationEvent { ... });
        
        // Send command đến queue cụ thể
        await _sendEndpointProvider.SendCommandAsync(command, "notification-queue");
    }
}
```

#### **Khi nào sử dụng gì:**
| **Mediator** | **MassTransit** |
|--------------|-----------------|
| ✅ Domain events trong cùng app | ✅ Integration events giữa services |
| ✅ Business logic validation | ✅ Commands đến specific queues |
| ✅ In-memory processing | ✅ Reliable message delivery |
| ✅ Fast, synchronous | ✅ Asynchronous, scalable |

### 8. **OAuth 2.0 / OpenID Connect Authentication Flow (Keycloak)**

Ứng dụng hỗ trợ xác thực OAuth 2.0 / OpenID Connect với Keycloak, cho phép đăng nhập an toàn và phân quyền truy cập API.

#### **Authentication Flow:**

```
[1] User mở Client (SPA/React)
      ↓
[2] Client redirect user đến Provider (Keycloak / IdentityServer / Google)
      URL: /authorize?client_id=webapp&redirect_uri=https://client.com/callback&scope=openid profile api1&response_type=code&code_challenge=xxxx

[3] Provider hiển thị trang đăng nhập
      → User nhập username/password (hoặc login Google, Facebook…)

[4] Provider xác thực user thành công
      → Redirect về client kèm theo "authorization code"
      https://client.com/callback?code=abc123&state=xyz

[5] Client gọi POST /token (server side)
      Gửi code để đổi token:
      {
        code: "abc123",
        redirect_uri: "https://client.com/callback",
        client_id: "webapp",
        code_verifier: "xxxx"
      }

[6] Provider xác thực code hợp lệ → trả về:
      {
        access_token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        id_token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        refresh_token: "def456"
      }

[7] Client lưu token (thường là access_token & id_token trong memory/session)

[8] Mỗi lần gọi API:
      Authorization: Bearer <access_token>

[9] Gateway hoặc API verify JWT → cho phép truy cập
```

#### **Cấu hình Keycloak:**

```json
{
  "KeycloakSettings": {
    "Authority": "https://keycloak.example.com/realms/your-realm",
    "ClientId": "webapp",
    "ClientSecret": "your-client-secret",
    "MetadataAddress": "https://keycloak.example.com/realms/your-realm/.well-known/openid-configuration"
  }
}
```

#### **Security Middleware Integration:**

```csharp
// Trong Program.cs hoặc Startup.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = configuration["KeycloakSettings:Authority"];
        options.Audience = configuration["KeycloakSettings:ClientId"];
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Protect controllers
app.UseAuthentication();
app.UseAuthorization();
```

### 9. **Factory Pattern**
```csharp
public static class DatabaseProviderFactory
{
    public static IDatabaseProvider CreateProvider(IConfiguration configuration)
    {
        var providerName = configuration["DatabaseSettings:DBProvider"] ?? "MySQL";
        return providerName.ToUpperInvariant() switch
        {
            "MYSQL" => new MySqlDatabaseProvider(),
            "ORACLE" => new OracleDatabaseProvider(),
            "POSTGRESQL" => new PostgreSqlDatabaseProvider(),
            _ => throw new NotSupportedException($"Database provider '{providerName}' is not supported.")
        };
    }
}
```

### 10. **Strategy Pattern**
- **Multi-Database Support**: MySQL, Oracle, PostgreSQL
- **Caching Strategies**: Redis, MongoDB
- **Logging Strategies**: Serilog với multiple sinks

## 🗄️ Database Support

### Multi-Database Architecture
Ứng dụng hỗ trợ nhiều database providers:

- **MySQL** (Pomelo.EntityFrameworkCore.MySql)
- **Oracle** (Oracle.EntityFrameworkCore)
- **PostgreSQL** (Npgsql.EntityFrameworkCore.PostgreSQL)

### Configuration
```json
{
  "DatabaseSettings": {
    "DBProvider": "MySQL", // MySQL, Oracle, PostgreSQL
    "ConnectionStrings": "Server=localhost;Database=CodeBase;Uid=root;Pwd=password;"
  }
}
```

### Entity Framework Core
- **Code First**: Migrations tự động
- **Change Tracking**: Optimistic concurrency
- **Domain Events**: Tích hợp với MediatR

## 🚀 Caching Strategy

### Redis Integration
```csharp
public interface IRedisRepository
{
    // String Operations
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetStringAsync(string key);
    
    // Object Operations
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    
    // Hash, List, Set Operations
    // Batch Operations
}
```

### MongoDB Support
- **Document Storage**: Flexible schema
- **Collection Management**: Auto-discovery
- **Read Preferences**: Primary/Secondary

## 📊 Logging & Monitoring

### Serilog Integration
```csharp
builder.Host.UseSerilog(SeriLogger.Configure);
```

### Pipeline Behaviors
1. **ValidationBehaviour**: FluentValidation integration
2. **PerformanceBehaviour**: Request timing
3. **UnhandledExceptionBehaviour**: Error logging

### Error Handling
```csharp
public class ErrorWrappingMiddleware
{
    // Centralized exception handling
    // Custom error responses
    // HTTP status code mapping
}
```

## 🔧 Technology Stack

### Core Technologies
- **.NET 9**: Latest framework
- **Entity Framework Core 9**: ORM
- **MediatR**: CQRS implementation
- **AutoMapper**: Object mapping
- **FluentValidation**: Input validation

### Databases & Caching
- **MySQL/PostgreSQL/Oracle**: Primary databases
- **Redis**: Caching layer
- **MongoDB**: Document storage

### Additional Libraries
- **Serilog**: Structured logging với Elasticsearch
- **Swagger/OpenAPI**: API documentation
- **MailKit**: Email services
- **Hangfire**: Background jobs
- **MassTransit**: Message queuing với RabbitMQ
- **MediatR**: In-memory messaging
- **Keycloak**: OAuth 2.0 / OpenID Connect authentication

## 📁 Project Structure

```
CodeBase/
├── Base.API/                    # Presentation Layer
│   ├── Controllers/
│   ├── Extensions/
│   └── Program.cs
├── Base.Application/            # Application Layer
│   ├── Feature/
│   │   └── Product/
│   │       ├── Commands/
│   │       ├── Queries/
│   │       └── EventHandlers/
│   ├── Common/
│   └── ConfigureServices.cs
├── Base.Domain/                 # Domain Layer
│   ├── Entities/
│   └── Interfaces/
├── Base.Infrastructure/         # Infrastructure Layer
│   ├── Persistence/
│   └── Repositories/
├── Contracts/                   # Shared Contracts
│   ├── Common/
│   ├── Domain/
│   └── Exceptions/
├── Infrastructure/              # Cross-cutting Infrastructure
│   ├── DatabaseProviders/
│   ├── Common/
│   └── Middlewares/
├── Logging/                     # Logging Infrastructure
└── Shared/                      # Shared Utilities
```

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- MySQL/PostgreSQL/Oracle (chọn một)
- Redis (optional)
- MongoDB (optional)
- RabbitMQ (cho MassTransit)
- Elasticsearch (cho logging)
- Keycloak (cho authentication)

### Installation
```bash
# Clone repository
git clone <repository-url>
cd CodeBase

# Restore packages
dotnet restore

# Update database
dotnet ef database update -p Base.Infrastructure -s Base.API

# Run application
dotnet run --project Base.API
```

### Configuration
1. Cập nhật `appsettings.json` với connection strings
2. Chọn database provider trong `DatabaseSettings:DBProvider`
3. Cấu hình Redis connection (nếu sử dụng)
4. Cấu hình RabbitMQ cho MassTransit
5. Cấu hình Elasticsearch cho logging
6. **Cấu hình Keycloak cho authentication** → [📖 Hướng dẫn setup Keycloak Realm](docs/auth/KEYCLOAK-QUICK-START.md)

#### **appsettings.json Example:**
```json
{
  "DatabaseSettings": {
    "DBProvider": "MySQL",
    "ConnectionStrings": "Server=localhost;Database=CodeBase;Uid=root;Pwd=password;"
  },
  "CacheSettings": {
    "ConnectionStrings": "localhost:6379"
  },
  "ElasticConfiguration": {
    "Uri": "http://localhost:9200",
    "Username": "elastic",
    "Password": "changeme"
  },
  "MassTransit": {
    "RabbitMQ": {
      "Host": "localhost",
      "Username": "guest",
      "Password": "guest",
      "VirtualHost": "/"
    }
  },
  "KeycloakSettings": {
    "Authority": "https://keycloak.example.com/realms/your-realm",
    "ClientId": "webapp",
    "ClientSecret": "your-client-secret",
    "MetadataAddress": "https://keycloak.example.com/realms/your-realm/.well-known/openid-configuration"
  }
}
```

## 📈 Features

### ✅ Implemented
- [x] Clean Architecture
- [x] CQRS with MediatR
- [x] Multi-database support
- [x] Redis caching
- [x] MongoDB support
- [x] Structured logging với Elasticsearch
- [x] Error handling
- [x] API documentation
- [x] Domain events với MediatR
- [x] Validation pipeline
- [x] MassTransit integration
- [x] Event-driven architecture
- [x] OAuth 2.0 / OpenID Connect (Keycloak)

### 🔄 In Progress
- [ ] Background jobs (Hangfire)
- [ ] JWT token refresh flow
- [ ] API versioning
- [ ] Role-based authorization 

### 📋 Planned
- [ ] Microservices support
- [ ] Docker containerization
- [ ] Kubernetes deployment
- [ ] Performance monitoring
- [ ] Health checks


## 📖 Documentation

### Authentication & Authorization
- **[Keycloak Quick Start](docs/auth/KEYCLOAK-QUICK-START.md)** - Setup Keycloak realm trong 15 phút ⚡
- **[Keycloak Realm Setup](docs/auth/KEYCLOAK-REALM-SETUP.md)** - Hướng dẫn đầy đủ về Keycloak configuration
- **[PBAC Authorization](docs/auth/README.md)** - Policy-Based Access Control documentation 🔐

### Infrastructure
- **[Docker Infrastructure Setup](infra/README.md)** - Setup toàn bộ infrastructure với Docker Compose

### Services
- API Gateway - API Gateway documentation (coming soon)
- Base Service - Base service documentation (coming soon)
- Generate Service - Code generation service documentation (coming soon)


## 🙏 Acknowledgments

- Clean Architecture principles by Uncle Bob
- .NET Community for excellent libraries
- All contributors and maintainers



