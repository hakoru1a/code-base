# CodeBase - Enterprise .NET 9 Application

## 📋 Project Overview

**CodeBase** is a modern, enterprise-ready application foundation built with .NET 9. It provides a robust starting point for developing scalable, secure, and maintainable web services.

This project is designed with a **Clean Architecture** approach and incorporates essential backend features out-of-the-box, including:
- Multi-database support (MySQL, PostgreSQL, Oracle)
- CQRS and MediatR patterns for decoupled logic
- Advanced authentication and authorization using Keycloak and Policy-Based Access Control (PBAC)
- Distributed caching with Redis
- Event-driven architecture with MassTransit and RabbitMQ
- Comprehensive logging with Serilog and Elasticsearch

The primary goal of this repository is to provide a production-grade template that accelerates development by solving common backend challenges.

---

## 🚀 Getting Started

### Prerequisites
- **.NET 9 SDK**
- **Docker & Docker Compose**: For running infrastructure (Database, Redis, Keycloak, etc.)

### Installation
```bash
# 1. Clone repository
git clone <repository-url>
cd CodeBase

# 2. Configure Infrastructure (Database, Keycloak)
#    Follow the comprehensive guides in our documentation index.
#    All documentation is centralized for ease of use.
```

### 📚 Documentation
For all setup, architecture, and usage guides, please refer to the main documentation index:

- **[➡️ Main Documentation Index](./docs/auth/INDEX.md)**

This index includes:
- **Keycloak Setup**: A complete guide to setting up your authentication provider.
- **PBAC Guides**: How to use the powerful Policy-Based Access Control system.
- **Architecture Deep Dives**: Explanations of the BFF pattern and more.

---

## 🔧 Technology Stack

- **Core**: .NET 9, EF Core 9, ASP.NET Core
- **Architecture**: Clean Architecture, CQRS, Repository & Unit of Work, DDD
- **Messaging**: MediatR (In-Process), MassTransit (Distributed) with RabbitMQ
- **Authentication**: OAuth 2.0 / OIDC with Keycloak
- **Database**: MySQL, PostgreSQL, Oracle
- **Caching**: Redis
- **Logging**: Serilog, Elasticsearch, Kibana
- **Tooling**: Docker, Swagger, FluentValidation, AutoMapper

---

## 🏗️ Architectural Design Patterns

This project heavily utilizes modern design patterns to ensure it is robust and scalable.

1.  **Clean Architecture**: Enforces separation of concerns and dependency inversion.
2.  **CQRS**: Segregates read and write operations.
3.  **MediatR**: Decouples in-process communication.
4.  **Repository & Unit of Work**: Abstracts data access and manages transactions.
5.  **Domain-Driven Design (DDD)**: Uses entities and domain events.
6.  **Event-Driven Architecture**: For both internal and external communication.
7.  **Factory & Strategy Patterns**: For creating database providers and other services.
8.  **Business Rules Pattern**: Encapsulates domain validation logic in dedicated rule classes.

---

## 📐 Domain Layer Structure

The Domain layer follows a clean, maintainable structure with clear separation of concerns:

### Domain Entity Structure

```
Generate.Domain/
├── {Entity}/
│   ├── {Entity}.cs              # Main entity class
│   ├── I{Entity}Repository.cs    # Repository interface
│   ├── {Entity}Error.cs          # Error factory (optional)
│   ├── Rules/                     # Business validation rules
│   │   ├── {Entity}{Property}RequiredRule.cs
│   │   ├── {Entity}{Property}LengthRule.cs
│   │   └── ...
│   ├── Specifications/            # Complex business queries
│   │   └── {Entity}Specifications.cs
│   └── Events/                    # Domain events
│       └── {Entity}{Action}Event.cs
```

### Business Rules Pattern

The project uses a **Business Rules Pattern** where each validation rule is a separate class implementing `IBusinessRule`:

```csharp
public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
    string Code { get; }
}
```

**Example Rule:**
```csharp
public class ProductNameRequiredRule : IBusinessRule
{
    private readonly string _name;

    public ProductNameRequiredRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    public string Message => "Product name is required.";
    public string Code => "Product.NameRequired";
}
```

**Usage in Entity:**
```csharp
public class Product : EntityAuditBase<long>
{
    public Product(string name)
    {
        CheckRule(new ProductNameRequiredRule(name));
        CheckRule(new ProductNameLengthRule(name));
        CheckRule(new ProductNameFormatRule(name));
        
        Name = name;
    }
}
```

### Key Features

- **`CheckRule()` Method**: Available in all entities via `EntityBase<T>`, automatically throws `BusinessRuleValidationException` when rules are broken.
- **Encapsulated Rules**: Each business rule is a separate, testable class.
- **Error Codes**: Each rule provides a unique error code for programmatic handling (e.g., `"Product.NameRequired"`).
- **Composable**: Rules can be easily combined and reused.
- **Clean Entities**: Domain entities focus on business logic, not validation implementation.

### Example Domain Entities

- **Product**: Manages products with name validation, category assignment, and order item tracking.
- **Order**: Handles customer orders with item management and business constraints.
- **Category**: Organizes products with validation and product management.

---

## 📈 Features

### ✅ Implemented
- [x] Clean Architecture & CQRS
- [x] Multi-database support
- [x] OAuth 2.0 / OpenID Connect (Keycloak)
- [x] Policy-Based Access Control (PBAC)
- [x] Redis Caching & MongoDB Support
- [x] Structured Logging (Serilog + Elasticsearch)
- [x] Event-Driven core with MediatR & MassTransit
- [x] Business Rules Pattern with IBusinessRule interface
- [x] Domain-Driven Design with encapsulated validation rules

### 📋 Planned
- [ ] **Health Checks**: Integrated monitoring for all services.
- [ ] **Resilience**: Circuit Breaker and Retry patterns with Polly.
- [ ] **Outbox Pattern**: Guaranteed message delivery.
- [ ] **CI/CD Pipeline**: Automated build, test, and deployment.
- [ ] **gRPC Communication**: High-performance inter-service communication.
- [ ] **Saga Pattern**: For distributed transactions.

---

## 🙏 Acknowledgments

- Clean Architecture principles by Uncle Bob
- .NET Community for excellent libraries
- All contributors and maintainers
