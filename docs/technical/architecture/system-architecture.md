# E-commerce Platform - System Architecture

## Architecture Overview

The e-commerce platform follows a **microservices architecture** built on the existing CodeBase foundation, leveraging Clean Architecture principles and Domain-Driven Design (DDD). The system is designed for scalability, maintainability, and resilience.

## High-Level Architecture

```mermaid
graph TB
    subgraph "External"
        Client[Web/Mobile Clients]
        AdminUI[Admin Dashboard]
        External[External Services<br/>PayPal, Stripe, Email]
    end
    
    subgraph "API Layer"
        Gateway[API Gateway<br/>YARP]
        Auth[Keycloak<br/>Identity Provider]
    end
    
    subgraph "Microservices"
        subgraph "Customer Domain"
            CustomerAPI[Customer Service]
            AuthAPI[Auth Service ✓]
        end
        
        subgraph "Product Domain" 
            CatalogAPI[Catalog Service<br/>Base→Enhanced ✓]
            InventoryAPI[Inventory Service]
            SearchAPI[Search Service]
        end
        
        subgraph "Sales Domain"
            CartAPI[Cart Service]
            OrderAPI[Order Service] 
            PaymentAPI[Payment Service]
        end
        
        subgraph "Support Domain"
            NotificationAPI[Notification Service]
            GenerateAPI[Generate Service ✓]
        end
    end
    
    subgraph "Data Layer"
        PostgreSQL[(PostgreSQL<br/>Primary DB)]
        MySQL[(MySQL<br/>Secondary DB)]
        Redis[(Redis<br/>Cache & Sessions)]
        Elasticsearch[(Elasticsearch<br/>Search Engine)]
    end
    
    subgraph "Infrastructure"
        RabbitMQ[RabbitMQ<br/>Message Bus]
        Docker[Docker<br/>Containerization]
        K8s[Kubernetes<br/>Orchestration]
    end
    
    Client --> Gateway
    AdminUI --> Gateway
    Gateway --> Auth
    Gateway --> CustomerAPI
    Gateway --> CatalogAPI
    Gateway --> CartAPI
    Gateway --> OrderAPI
    Gateway --> PaymentAPI
    
    CustomerAPI --> PostgreSQL
    CatalogAPI --> PostgreSQL
    OrderAPI --> PostgreSQL
    PaymentAPI --> PostgreSQL
    CartAPI --> Redis
    SearchAPI --> Elasticsearch
    
    OrderAPI --> RabbitMQ
    PaymentAPI --> RabbitMQ
    NotificationAPI --> RabbitMQ
    InventoryAPI --> RabbitMQ
    
    PaymentAPI --> External
    NotificationAPI --> External
```

## Architectural Principles

### 1. Domain-Driven Design (DDD)
- **Bounded Contexts**: Each service represents a distinct business domain
- **Ubiquitous Language**: Consistent terminology within each domain
- **Aggregate Design**: Encapsulate business rules and maintain consistency
- **Domain Events**: Communicate business state changes

### 2. Clean Architecture
- **Dependency Inversion**: Inner layers don't depend on outer layers
- **Separation of Concerns**: Clear boundaries between layers
- **Testability**: Business logic independent of frameworks
- **Framework Independence**: Core business logic isolated from external concerns

### 3. Microservices Patterns
- **Database per Service**: Each service owns its data
- **API Gateway**: Single entry point for external clients
- **Event-Driven Communication**: Asynchronous messaging for loose coupling
- **Circuit Breaker**: Resilience against cascading failures

## Service Architecture Details

### 1. API Gateway (YARP)
**Current Implementation**: ✅ **Enhanced for E-commerce**

```yaml
# Enhanced Gateway Configuration
Routes:
  # Authentication
  - Path: /api/auth/*
    Destination: auth-service
    
  # Product Catalog
  - Path: /api/products/*
    Destination: catalog-service
    AuthRequired: false
    
  - Path: /api/categories/*
    Destination: catalog-service
    
  # Shopping & Orders
  - Path: /api/cart/*
    Destination: cart-service
    AuthRequired: true
    
  - Path: /api/orders/*
    Destination: order-service
    AuthRequired: true
    
  # Payments
  - Path: /api/payments/*
    Destination: payment-service
    AuthRequired: true
    
  # Admin Routes
  - Path: /api/admin/*
    Destination: various-services
    Roles: [Admin, Manager]

Policies:
  - RateLimit: 100 requests/minute
  - Cors: Enabled
  - RequestLogging: Enabled
  - ResponseCaching: Selected endpoints
```

**Key Features**:
- JWT token validation
- Route-based authentication
- Rate limiting per endpoint
- Request/response logging
- CORS handling
- Health checks aggregation

### 2. Auth Service (Enhanced)
**Current Implementation**: ✅ **Integrated with Keycloak**

**Enhancements for E-commerce**:
```csharp
// Enhanced User Roles
public enum UserRole
{
    Customer = 1,
    Admin = 2,
    Manager = 3,
    Support = 4,
    Guest = 5
}

// E-commerce Specific Claims
public static class EcommerceClaims
{
    public const string CustomerId = "customer_id";
    public const string CustomerTier = "customer_tier";
    public const string ShoppingPreferences = "shopping_preferences";
    public const string CartId = "cart_id";
}
```

**Integration Points**:
- Customer registration/login
- Social authentication (Google, Facebook)
- Guest user handling
- Session management
- Permission-based access control

### 3. Catalog Service (Enhanced Base Service)
**Current Implementation**: ✅ **Base Service → Enhanced to Catalog Service**

**Enhanced Domain Model**:
```csharp
// Product Aggregate (Enhanced)
public class Product : AggregateRoot<ProductId>
{
    // Basic Properties (Existing in Base)
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string SKU { get; private set; }
    
    // E-commerce Enhancements
    public Money Price { get; private set; }
    public Money ComparePrice { get; private set; }
    public CategoryId CategoryId { get; private set; }
    public Brand Brand { get; private set; }
    public List<ProductVariant> Variants { get; private set; }
    public List<ProductImage> Images { get; private set; }
    public ProductSEO SEO { get; private set; }
    public ProductStatus Status { get; private set; }
    public InventoryTracking Inventory { get; private set; }
    
    // Domain Methods
    public void UpdatePricing(Money price, Money comparePrice)
    public void AddVariant(string sku, ProductAttributes attributes)
    public void UpdateInventory(int quantity, string reason)
    public void Publish() 
    public void Archive()
}
```

**Database Schema (Enhanced)**:
```sql
-- Enhanced Products table
ALTER TABLE products ADD COLUMN category_id UUID;
ALTER TABLE products ADD COLUMN brand_id UUID;
ALTER TABLE products ADD COLUMN compare_price DECIMAL(10,2);
ALTER TABLE products ADD COLUMN status VARCHAR(20) DEFAULT 'draft';
ALTER TABLE products ADD COLUMN seo_title VARCHAR(255);
ALTER TABLE products ADD COLUMN seo_description TEXT;

-- New tables for e-commerce
CREATE TABLE categories (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(200) UNIQUE NOT NULL,
    description TEXT,
    parent_id UUID REFERENCES categories(id),
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT true
);

CREATE TABLE product_variants (
    id UUID PRIMARY KEY,
    product_id UUID REFERENCES products(id),
    sku VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(200),
    price DECIMAL(10,2),
    attributes JSONB,
    inventory_quantity INT DEFAULT 0
);

CREATE TABLE product_images (
    id UUID PRIMARY KEY,
    product_id UUID REFERENCES products(id),
    url VARCHAR(500) NOT NULL,
    alt_text VARCHAR(200),
    display_order INT DEFAULT 0,
    is_primary BOOLEAN DEFAULT false
);
```

### 4. Cart Service (New)
**Technology Stack**:
- **.NET 8** Web API
- **Redis** for session-based storage
- **PostgreSQL** for persistent cart data

**Architecture**:
```csharp
// Cart Aggregate
public class ShoppingCart : AggregateRoot<CartId>
{
    public CustomerId? CustomerId { get; private set; }
    public string SessionId { get; private set; }
    public List<CartItem> Items { get; private set; }
    public CartTotals Totals { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    
    public void AddItem(ProductVariantId variantId, int quantity, Money unitPrice)
    public void UpdateQuantity(CartItemId itemId, int newQuantity)
    public void RemoveItem(CartItemId itemId)
    public void ApplyDiscountCode(string code)
    public void MergeWith(ShoppingCart guestCart)
}

// Redis Storage Strategy
public class RedisCartRepository : ICartRepository
{
    public async Task<ShoppingCart> GetBySessionAsync(string sessionId)
    public async Task<ShoppingCart> GetByCustomerAsync(CustomerId customerId)
    public async Task SaveAsync(ShoppingCart cart)
    public async Task DeleteAsync(CartId cartId)
}
```

### 5. Order Service (New)
**Technology Stack**:
- **.NET 8** Web API
- **PostgreSQL** for transactional data
- **Saga Pattern** for distributed transactions

**Architecture**:
```csharp
// Order Aggregate
public class Order : AggregateRoot<OrderId>
{
    public string OrderNumber { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderTotals Totals { get; private set; }
    public Address ShippingAddress { get; private set; }
    public PaymentInfo PaymentInfo { get; private set; }
    
    public void Confirm(PaymentId paymentId)
    public void Ship(string trackingNumber)
    public void Deliver()
    public void Cancel(string reason)
}

// Order Processing Saga
public class OrderProcessingSaga : Saga<OrderProcessingState>
{
    // 1. Validate order items
    // 2. Reserve inventory
    // 3. Process payment
    // 4. Confirm order
    // 5. Send notifications
}
```

### 6. Payment Service (New)
**Technology Stack**:
- **.NET 8** Web API
- **PostgreSQL** for transaction records
- **Stripe/PayPal SDKs** for payment processing

**Architecture**:
```csharp
// Payment Aggregate
public class Payment : AggregateRoot<PaymentId>
{
    public OrderId OrderId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string ExternalTransactionId { get; private set; }
    
    public void Authorize(PaymentProvider provider)
    public void Capture()
    public void Refund(Money amount, string reason)
    public void Fail(string errorMessage)
}

// Payment Provider Strategy
public interface IPaymentProvider
{
    Task<PaymentResult> AuthorizeAsync(PaymentRequest request);
    Task<PaymentResult> CaptureAsync(string transactionId);
    Task<RefundResult> RefundAsync(string transactionId, Money amount);
}
```

### 7. Inventory Service (New)
**Technology Stack**:
- **.NET 8** Web API
- **PostgreSQL** for inventory data
- **Event Sourcing** for inventory history

**Architecture**:
```csharp
// Inventory Aggregate
public class InventoryItem : AggregateRoot<InventoryItemId>
{
    public ProductVariantId ProductVariantId { get; private set; }
    public int QuantityAvailable { get; private set; }
    public int QuantityReserved { get; private set; }
    public int LowStockThreshold { get; private set; }
    
    public bool CanReserve(int quantity)
    public void Reserve(int quantity, OrderId orderId)
    public void Release(int quantity, OrderId orderId)
    public void Adjust(int quantity, string reason)
}
```

### 8. Search Service (New)
**Technology Stack**:
- **.NET 8** Web API
- **Elasticsearch** for search engine
- **NEST** client for .NET

**Architecture**:
```csharp
// Search Document
public class ProductSearchDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public List<string> Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Search Service
public class ProductSearchService
{
    public async Task<SearchResults<ProductSearchDocument>> SearchAsync(SearchQuery query)
    public async Task IndexProductAsync(ProductSearchDocument document)
    public async Task DeleteProductAsync(Guid productId)
    public async Task<List<string>> GetSuggestionsAsync(string term)
}
```

### 9. Notification Service (New)
**Technology Stack**:
- **.NET 8** Web API
- **RabbitMQ** for event consumption
- **SendGrid/Twilio** for delivery

**Architecture**:
```csharp
// Notification Aggregate
public class Notification : AggregateRoot<NotificationId>
{
    public RecipientId RecipientId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Content { get; private set; }
    public NotificationStatus Status { get; private set; }
    
    public void Send()
    public void MarkDelivered()
    public void MarkFailed(string reason)
}

// Event Handlers
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent @event)
    {
        // Send order confirmation email
        // Send SMS notification if enabled
    }
}
```

## Communication Patterns

### 1. Synchronous Communication (HTTP/gRPC)
- **Client ↔ API Gateway**: REST APIs
- **API Gateway ↔ Services**: HTTP with circuit breaker
- **Service ↔ Service**: Direct HTTP for immediate queries

```csharp
// HTTP Client with Polly Circuit Breaker
public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    public async Task<ProductInfo> GetProductAsync(Guid productId)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.GetAsync($"/api/products/{productId}"));
            
        return await response.Content.ReadAsAsync<ProductInfo>();
    }
}
```

### 2. Asynchronous Communication (Message Bus)
- **Domain Events**: Within service boundaries
- **Integration Events**: Cross-service communication
- **Command Events**: Saga orchestration

```csharp
// Integration Event
public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
}

// Event Bus Configuration
services.AddRabbitMQEventBus(configuration =>
{
    configuration.HostName = "rabbitmq";
    configuration.UserName = "guest";
    configuration.Password = "guest";
    configuration.VirtualHost = "/";
    
    // Exchange configuration
    configuration.Exchanges.Add("order.events", ExchangeType.Topic);
    configuration.Exchanges.Add("inventory.events", ExchangeType.Topic);
    configuration.Exchanges.Add("notification.events", ExchangeType.Fanout);
});
```

## Data Architecture

### 1. Database per Service Pattern
```yaml
Services:
  Customer Service:
    Database: PostgreSQL
    Schema: customer_db
    
  Catalog Service:
    Database: PostgreSQL  
    Schema: catalog_db
    
  Order Service:
    Database: PostgreSQL
    Schema: order_db
    
  Payment Service:
    Database: PostgreSQL
    Schema: payment_db
    
  Cart Service:
    Primary: Redis (sessions)
    Secondary: PostgreSQL (persistence)
    
  Inventory Service:
    Database: PostgreSQL
    Schema: inventory_db
    
  Search Service:
    Database: Elasticsearch
    Indexes: products, categories
```

### 2. Data Consistency Strategies
- **Strong Consistency**: Within aggregates (single database transaction)
- **Eventual Consistency**: Across services (event-driven updates)
- **Saga Pattern**: For distributed transactions
- **Event Sourcing**: For audit trails and replay capability

### 3. Caching Strategy
```csharp
// Redis Caching Layers
public class CacheConfiguration
{
    public static readonly TimeSpan ProductCacheTTL = TimeSpan.FromHours(1);
    public static readonly TimeSpan CategoryCacheTTL = TimeSpan.FromDays(1);
    public static readonly TimeSpan PriceCacheTTL = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan CartCacheTTL = TimeSpan.FromDays(7);
    public static readonly TimeSpan SessionCacheTTL = TimeSpan.FromMinutes(30);
}
```

## Security Architecture

### 1. Authentication & Authorization
- **Identity Provider**: Keycloak with OAuth 2.0/OpenID Connect
- **Token Strategy**: JWT with refresh tokens
- **API Security**: Bearer token validation
- **Role-Based Access**: Fine-grained permissions

### 2. Data Protection
- **Encryption in Transit**: TLS 1.3 for all communications
- **Encryption at Rest**: Database encryption for sensitive data
- **PCI Compliance**: Payment data isolation and tokenization
- **GDPR Compliance**: Data privacy and consent management

## Deployment Architecture

### 1. Containerization
```dockerfile
# Standard Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ServiceName.API.dll"]
```

### 2. Kubernetes Deployment
```yaml
# Service Deployment Template
apiVersion: apps/v1
kind: Deployment
metadata:
  name: catalog-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: catalog-service
  template:
    spec:
      containers:
      - name: catalog-service
        image: ecommerce/catalog-service:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: database-secret
              key: connection-string
```

### 3. Infrastructure as Code
- **Terraform**: Infrastructure provisioning
- **Helm Charts**: Kubernetes deployments
- **Azure DevOps**: CI/CD pipelines
- **Docker Compose**: Local development

## Monitoring & Observability

### 1. Logging Strategy
- **Structured Logging**: Serilog with JSON formatting
- **Centralized Logs**: ELK Stack (Elasticsearch, Logstash, Kibana)
- **Correlation IDs**: Request tracing across services
- **Log Levels**: Appropriate logging for each environment

### 2. Metrics Collection
- **Application Metrics**: Performance counters, business metrics
- **Infrastructure Metrics**: CPU, memory, network, disk usage
- **Custom Metrics**: Domain-specific KPIs
- **Prometheus + Grafana**: Metrics collection and visualization

### 3. Distributed Tracing
- **OpenTelemetry**: Standardized tracing
- **Jaeger**: Distributed tracing system
- **Request Flow**: End-to-end request tracking
- **Performance Analysis**: Bottleneck identification

This architecture provides a solid foundation for a scalable, maintainable e-commerce platform while leveraging the existing CodeBase infrastructure and following industry best practices.