# 📚 Hướng dẫn Kiến trúc Dự án TLBiomass

> **Phiên bản:** 1.0  
> **Ngày cập nhật:** 2026-01-20  
> **Tài liệu này mô tả toàn bộ quy tắc, cấu trúc và patterns của hệ thống backend**

---

## 📋 Mục lục

1. [Tổng quan Kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Cấu trúc Thư mục](#2-cấu-trúc-thư-mục)
3. [Domain Layer](#3-domain-layer)
4. [Application Layer](#4-application-layer)
5. [Infrastructure Layer](#5-infrastructure-layer)
6. [API Layer](#6-api-layer)
7. [BuildingBlocks (Shared Libraries)](#7-buildingblocks-shared-libraries)
8. [Quy tắc Đặt tên](#8-quy-tắc-đặt-tên)
9. [Flow xử lý Request](#9-flow-xử-lý-request)
10. [Hướng dẫn Tạo Feature Mới](#10-hướng-dẫn-tạo-feature-mới)
11. [Best Practices](#11-best-practices)

---

## 1. Tổng quan Kiến trúc

### 1.1 Các Pattern được sử dụng

| Pattern | Mô tả | Vị trí |
|---------|-------|--------|
| **Clean Architecture** | Tách biệt concerns thành các layers độc lập | Toàn bộ project |
| **Domain-Driven Design (DDD)** | Rich domain model với business logic | Domain Layer |
| **CQRS** | Tách riêng Commands (Write) và Queries (Read) | Application Layer |
| **Repository Pattern** | Abstract data access | Domain + Infrastructure |
| **Specification Pattern** | Encapsulate query conditions | Domain Layer |
| **MediatR** | Request/Response pipeline, decoupling | Application Layer |
| **Unit of Work** | Transaction management | Infrastructure Layer |

### 1.2 Dependency Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                                │
│                    (Presentation Layer)                          │
│         Phụ thuộc vào: Application, Infrastructure               │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                           │
│                     (Use Cases / CQRS)                           │
│                  Phụ thuộc vào: Domain                           │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Domain Layer                               │
│                  (Business Logic - Core)                         │
│              KHÔNG phụ thuộc vào layer nào khác                  │
└─────────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────┴───────────────────────────────────┐
│                    Infrastructure Layer                          │
│               (Persistence, External Services)                   │
│             Implements Domain interfaces                         │
└─────────────────────────────────────────────────────────────────┘
```

> ⚠️ **QUAN TRỌNG:** Domain Layer KHÔNG BAO GIỜ phụ thuộc vào layer khác. Đây là nguyên tắc cốt lõi của Clean Architecture.

---

## 2. Cấu trúc Thư mục

### 2.1 Tổng quan cấu trúc

```
src/
├── ApiGateways/              # API Gateway (nếu có microservices)
├── BuildingBlocks/           # Shared libraries
│   ├── Contracts/            # Interfaces và Domain primitives
│   ├── Infrastructure/       # Common infrastructure code
│   ├── Logging/              # Logging utilities
│   └── Shared/               # Shared DTOs, Attributes, Configs
└── Services/                 # Microservices
    └── {ServiceName}/        # Mỗi service là 1 bounded context
        ├── {ServiceName}.API/
        ├── {ServiceName}.Application/
        ├── {ServiceName}.Domain/
        └── {ServiceName}.Infrastructure/
```

### 2.2 Cấu trúc một Service (Ví dụ: Generate)

```
Generate/
├── Generate.API/                    # ← Presentation Layer
│   ├── Controllers/
│   │   ├── ProductController.cs
│   │   ├── CategoryController.cs
│   │   └── OrderController.cs
│   ├── Extensions/
│   ├── Filters/
│   ├── Program.cs
│   └── appsettings.json
│
├── Generate.Application/            # ← Application Layer
│   ├── Common/
│   │   └── Mappings/
│   └── Features/
│       ├── Products/
│       │   ├── Commands/
│       │   │   ├── CreateProduct/
│       │   │   ├── UpdateProduct/
│       │   │   └── DeleteProduct/
│       │   ├── Queries/
│       │   │   ├── GetProducts/
│       │   │   └── GetProductById/
│       │   ├── EventHandlers/
│       │   └── Policies/
│       ├── Categories/
│       └── Orders/
│
├── Generate.Domain/                 # ← Domain Layer (Core)
│   ├── Products/
│   │   ├── Product.cs              # Aggregate Root
│   │   ├── ProductDetail.cs        # Entity con
│   │   ├── ProductError.cs         # Business Exceptions
│   │   ├── Interfaces/
│   │   │   └── IProductRepository.cs
│   │   ├── Rules/
│   │   │   ├── ProductCanBeDeletedRule.cs
│   │   │   └── ProductCategoryRequiredRule.cs
│   │   └── Specifications/
│   │       ├── BelongsToCategorySpecification.cs
│   │       └── CanBeDeletedSpecification.cs
│   ├── Categories/
│   └── Orders/
│
└── Generate.Infrastructure/         # ← Infrastructure Layer
    ├── ConfigureServices.cs
    ├── Persistences/
    │   ├── GenerateContext.cs      # DbContext
    │   ├── Configurations/         # Entity Type Configurations
    │   └── Migrations/
    └── Repositories/
        ├── ProductRepository.cs
        ├── CategoryRepository.cs
        └── OrderRepository.cs
```

---

## 3. Domain Layer

### 3.1 Nguyên tắc cơ bản

> **Domain Layer là trái tim của hệ thống. Mọi business logic phải nằm ở đây.**

- ✅ **KHÔNG** phụ thuộc vào framework (EF Core, ASP.NET, etc.)
- ✅ **KHÔNG** phụ thuộc vào infrastructure (Database, External API)
- ✅ **CHỈ** sử dụng interfaces từ `Contracts` project
- ✅ **CHỨA** tất cả business rules và domain logic

### 3.2 Entity (Aggregate Root)

```csharp
// ✅ ĐÚNG: Rich Domain Model
public class Product : EntityAuditBase<long>
{
    // 1. Properties với private setter - Encapsulation
    public string Name { get; private set; } = string.Empty;
    public virtual Category? Category { get; private set; }
    
    // 2. Private collection - Không expose trực tiếp
    private readonly List<OrderItem> _orderItems = new();
    public virtual IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
    
    // 3. Private constructor cho ORM
    private Product() { }
    
    // 4. Factory Method - Controlled object creation
    public static Product Create(string name, Category? category = null)
    {
        return new Product(name, category);
    }
    
    // 5. Business Methods với Rules validation
    public void AssignToCategory(Category category)
    {
        CheckRule(new ProductCategoryRequiredRule(category));
        Category = category;
    }
    
    // 6. Domain Query methods
    public bool CanBeDeleted()
    {
        var rule = new ProductCanBeDeletedRule(_orderItems);
        return !rule.IsBroken();
    }
}

// ❌ SAI: Anemic Domain Model (chỉ có data, không có behavior)
public class BadProduct
{
    public long Id { get; set; }
    public string Name { get; set; }  // Public setter = vi phạm encapsulation
    public long? CategoryId { get; set; }
}
```

### 3.3 Base Classes cho Entity

```csharp
// EntityBase<T> - Base cho tất cả entities
public abstract class EntityBase<T> : IEntityBase<T>
{
    public T Id { get; set; } = default!;
    
    // Kiểm tra business rule
    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
    
    // Kiểm tra specification
    public bool SatisfiesSpecification<TEntity>(ISpecification<TEntity> spec) 
        where TEntity : EntityBase<T>
    {
        return this is TEntity entity && spec.IsSatisfiedBy(entity);
    }
}

// EntityAuditBase<T> - Entity với audit fields
public abstract class EntityAuditBase<T> : EntityBase<T>, IAuditable<T>
{
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
    public T? CreatedBy { get; set; }
    public T? LastModifiedBy { get; set; }
}
```

### 3.4 Business Rules

> **Business Rule đóng gói một điều kiện nghiệp vụ có thể validate độc lập**

```csharp
// Interface
public interface IBusinessRule
{
    bool IsBroken();        // True nếu rule bị vi phạm
    string Message { get; } // Thông báo lỗi
    string Code { get; }    // Mã lỗi: {Entity}.{RuleName}
}

// Implementation
public class ProductCanBeDeletedRule : IBusinessRule
{
    private readonly List<OrderItem> _orderItems;

    public ProductCanBeDeletedRule(List<OrderItem> orderItems)
    {
        _orderItems = orderItems;
    }

    public bool IsBroken() => _orderItems.Any();  // Có orders = không xóa được
    
    public string Message => "Cannot delete product that has existing orders.";
    
    public string Code => "Product.CannotDeleteWithOrders";
}
```

### 3.5 Composite Rules (Kết hợp nhiều rules)

```csharp
// Sử dụng And/Or để kết hợp rules
public void AddOrderItem(OrderItem orderItem)
{
    CheckRule(
        new ProductOrderItemRequiredRule(orderItem)
            .And(new ProductOrderItemNotExistsRule(_orderItems, orderItem))
            .And(new ProductPriceMustBePositiveRule(orderItem.Price))
    );
    
    _orderItems.Add(orderItem);
}

// Extension methods
public static class RuleExtensions
{
    public static IBusinessRule And(this IBusinessRule left, IBusinessRule right)
        => new CompositeRule.AndRule(left, right);
    
    public static IBusinessRule Or(this IBusinessRule left, IBusinessRule right)
        => new CompositeRule.OrRule(left, right);
}
```

### 3.6 Specifications

> **Specification đóng gói query conditions, có thể combine và reuse**

```csharp
// Interface
public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
}

// Implementation
public class BelongsToCategorySpecification : ISpecification<Product>
{
    private readonly Category _category;

    public BelongsToCategorySpecification(Category category)
    {
        _category = category ?? throw ProductError.CategoryCannotBeNull();
    }

    public bool IsSatisfiedBy(Product product)
    {
        return product.Category != null && product.Category.Id == _category.Id;
    }
}

// Sử dụng
var specification = new BelongsToCategorySpecification(electronics);
var electronicsProducts = products.Where(p => specification.IsSatisfiedBy(p));
```

### 3.7 Domain Errors

```csharp
// Static class chứa factory methods cho exceptions
public static class ProductError
{
    public static BusinessException NameCannotBeEmpty()
        => new("Product name cannot be empty");

    public static BusinessException CategoryNotFound(long categoryId)
        => new($"Category with ID {categoryId} not found");

    public static BusinessException CannotDeleteProductWithOrders()
        => new("Cannot delete product that has existing orders");
}

// Sử dụng
if (string.IsNullOrEmpty(name))
    throw ProductError.NameCannotBeEmpty();
```

### 3.8 Repository Interface (trong Domain)

```csharp
// Định nghĩa contract, KHÔNG implementation
public interface IProductRepository : IRepositoryBaseAsync<Product, long>
{
    // Thêm domain-specific methods nếu cần
    // Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId);
}
```

---

## 4. Application Layer

### 4.1 Nguyên tắc cơ bản

> **Application Layer chứa use cases, orchestrate domain objects**

- ✅ Sử dụng **CQRS**: Commands cho Write, Queries cho Read
- ✅ Mỗi use case = 1 folder với Command/Query + Handler + Validator
- ✅ **KHÔNG** chứa business logic → delegate cho Domain
- ✅ Validate input tại đây (FluentValidation)

### 4.2 Cấu trúc CQRS

```
Features/
└── Products/
    ├── Commands/            # Write operations
    │   ├── CreateProduct/
    │   │   ├── CreateProductCommand.cs      # Request DTO
    │   │   ├── CreateProductCommandHandler.cs
    │   │   └── CreateProductValidator.cs
    │   ├── UpdateProduct/
    │   └── DeleteProduct/
    ├── Queries/             # Read operations
    │   ├── GetProducts/
    │   │   ├── GetProductsQuery.cs
    │   │   └── GetProductsQueryHandler.cs
    │   └── GetProductById/
    ├── EventHandlers/       # Domain Event Handlers
    └── Policies/            # Authorization Policies
```

### 4.3 Command (Write)

```csharp
// 1. Command - Request DTO
public class CreateProductCommand : IRequest<long>  // Returns ID
{
    public string Name { get; set; } = string.Empty;
    public long? CategoryId { get; set; }
}

// 2. Handler - Xử lý logic
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, long>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<long> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Load related entities
        Category? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
        }

        // 2. Use Domain Factory Method
        var product = Product.Create(request.Name, category);

        // 3. Persist
        var result = await _productRepository.CreateAsync(product);
        await _productRepository.SaveChangesAsync();

        return result;
    }
}

// 3. Validator - FluentValidation
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0")
            .When(x => x.CategoryId.HasValue);
    }
}
```

### 4.4 Query (Read)

```csharp
// 1. Query - Request DTO
public class GetProductsQuery : IRequest<List<ProductResponseDto>>
{
    public string? SearchTerm { get; set; }
    public long? CategoryId { get; set; }
}

// 2. Handler
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductResponseDto>> Handle(
        GetProductsQuery request, 
        CancellationToken ct)
    {
        IQueryable<Product> query = _productRepository
            .FindAll()
            .Include(p => p.Category);

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.Category != null && 
                                    p.Category.Id == request.CategoryId);
        }

        var products = await query.ToListAsync(ct);
        
        // Map to DTO
        return products.Adapt<List<ProductResponseDto>>();
    }
}
```

### 4.5 Event Handlers

> **Xử lý side effects sau khi domain events được publish**

```csharp
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Product Created: {ProductId} - {Name} at {CreatedDate}",
            notification.ProductId, 
            notification.Name, 
            notification.CreatedDate);

        // Side effects:
        // - Invalidate cache
        // - Update search index
        // - Send notifications
        // - etc.

        return Task.CompletedTask;
    }
}
```

### 4.6 Authorization Policies

```csharp
[Policy("PRODUCT:VIEW", Description = "View products with dynamic filtering")]
public class ProductViewPolicy : BasePolicy
{
    public override Task<PolicyEvaluationResult> EvaluateAsync(
        UserClaimsContext user,
        Dictionary<string, object> context)
    {
        // 1. Check authentication
        if (!IsAuthenticated(user))
        {
            return Task.FromResult(PolicyEvaluationResult.Deny(
                "User must be authenticated"));
        }

        // 2. Admin/Manager có full access
        if (HasAnyRole(user, "admin", "manager"))
        {
            var filterContext = new ProductFilterContext { CanViewAll = true };
            return Task.FromResult(PolicyEvaluationResult.Allow(
                "Admin/Manager can view all", filterContext));
        }

        // 3. Apply dynamic filters based on JWT claims
        var filterContext = new ProductFilterContext();
        
        if (user.Claims.TryGetValue("max_product_price", out var maxPrice))
        {
            filterContext.MaxPrice = decimal.Parse(maxPrice);
        }

        return Task.FromResult(PolicyEvaluationResult.Allow(
            "User authenticated with filters", filterContext));
    }
}
```

### 4.7 MediatR Pipeline Behaviors

```csharp
// Validation Behavior - Tự động validate tất cả Commands/Queries
public class ValidationBehaviour<TRequest, TResponse> : 
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

---

## 5. Infrastructure Layer

### 5.1 Nguyên tắc cơ bản

> **Infrastructure Layer implement các interfaces từ Domain**

- ✅ Implement Domain repository interfaces
- ✅ Chứa DbContext và Entity configurations
- ✅ Kết nối với external services
- ✅ DI registration

### 5.2 Repository Implementation

```csharp
public class ProductRepository : 
    RepositoryBaseAsync<Product, long, GenerateContext>, 
    IProductRepository
{
    public ProductRepository(
        GenerateContext dbContext, 
        IUnitOfWork<GenerateContext> unitOfWork) 
        : base(dbContext, unitOfWork)
    {
    }

    // Domain-specific methods
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId)
    {
        return await FindAll(p => p.Category != null && p.Category.Id == categoryId)
            .ToListAsync();
    }
}
```

### 5.3 DbContext

```csharp
public class GenerateContext : DbContext
{
    private IMediator _mediator;
    private List<BaseEvent>? _events;

    public GenerateContext(DbContextOptions<GenerateContext> options, IMediator mediator) 
        : base(options)
    {
        _mediator = mediator;
    }

    // DbSets
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ignore domain events
        modelBuilder.Ignore<BaseEvent>();
        
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Collect domain events before save
        SaveEventBeforeSaveChanges();

        // 2. Auto-set audit fields
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is IDateTracking entity)
            {
                entity.CreatedDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified && entry.Entity is IDateTracking modEntity)
            {
                modEntity.LastModifiedDate = DateTime.UtcNow;
            }
        }

        // 3. Save changes
        var result = await base.SaveChangesAsync(ct);

        // 4. Dispatch domain events AFTER save
        if (_events?.Any() == true)
        {
            await _mediator.DispatchDomainEventAsync(_events);
        }

        return result;
    }
}
```

### 5.4 Dependency Injection Registration

```csharp
public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add common infrastructure (Database + Redis)
        services.AddCommonInfrastructure<GenerateContext>(configuration);

        // Register repositories using Domain interfaces
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
```

---

## 6. API Layer

### 6.1 Nguyên tắc cơ bản

> **API Layer là thin layer, chỉ map requests và delegate cho Application**

- ✅ **Thin Controllers** - Không chứa logic
- ✅ Map DTOs → Commands/Queries
- ✅ Sử dụng `ApiControllerBase<T>` 
- ✅ Policy-based Authorization

### 6.2 Controller Template

```csharp
[ApiVersion("1.0")]
public class ProductController : ApiControllerBase<ProductController>
{
    private const string EntityName = "Product";

    public ProductController(IMediator mediator, ILogger<ProductController> logger)
        : base(mediator, logger)
    {
    }

    [HttpGet]
    [RequirePolicy("PRODUCT:VIEW")]
    [ProducesResponseType(typeof(ApiSuccessResult<List<ProductResponseDto>>), 200)]
    public async Task<IActionResult> GetList()
    {
        var query = new GetProductsQuery();
        return await HandleGetAllAsync<GetProductsQuery, ProductResponseDto>(query, EntityName);
    }

    [HttpGet("{id}")]
    [RequirePolicy("PRODUCT:VIEW")]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetProductByIdQuery { Id = id };
        return await HandleGetByIdAsync<GetProductByIdQuery, ProductResponseDto>(
            query, id, EntityName);
    }

    [HttpPost]
    [RequirePolicy("PRODUCT:CREATE")]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        var command = new CreateProductCommand
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId
        };
        return await HandleCreateAsync(command, EntityName, dto.Name);
    }

    [HttpPut("{id}")]
    [RequirePolicy("PRODUCT:UPDATE")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductUpdateDto dto)
    {
        var command = new UpdateProductCommand
        {
            Id = dto.Id,
            Name = dto.Name,
            CategoryId = dto.CategoryId
        };
        return await HandleUpdateAsync(command, id, dto.Id, EntityName);
    }

    [HttpDelete("{id}")]
    [RequirePolicy("PRODUCT:DELETE")]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteProductCommand { Id = id };
        return await HandleDeleteAsync(command, id, EntityName);
    }
}
```

### 6.3 ApiControllerBase Helper Methods

```csharp
public abstract class ApiControllerBase<T> : ControllerBase
{
    protected readonly IMediator Mediator;
    protected readonly ILogger<T> Logger;

    // GET all
    protected async Task<IActionResult> HandleGetAllAsync<TQuery, TResponse>(
        TQuery query, string entityName)
        where TQuery : IRequest<List<TResponse>>;

    // GET paged
    protected async Task<IActionResult> HandleGetPagedAsync<TQuery, TResponse>(
        TQuery query, string entityName, int pageNumber, int pageSize)
        where TQuery : IRequest<PagedList<TResponse>>;

    // GET by ID
    protected async Task<IActionResult> HandleGetByIdAsync<TQuery, TResponse>(
        TQuery query, long id, string entityName)
        where TQuery : IRequest<TResponse?>;

    // POST create
    protected async Task<IActionResult> HandleCreateAsync<TCommand>(
        TCommand command, string entityName, string entityIdentifier)
        where TCommand : IRequest<long>;

    // PUT update
    protected async Task<IActionResult> HandleUpdateAsync<TCommand>(
        TCommand command, long id, long dtoId, string entityName)
        where TCommand : IRequest<bool>;

    // DELETE
    protected async Task<IActionResult> HandleDeleteAsync<TCommand>(
        TCommand command, long id, string entityName)
        where TCommand : IRequest<bool>;
}
```

### 6.4 API Response Format

```csharp
// Success Response
{
    "isSuccess": true,
    "message": "Product created successfully",
    "data": 123,
    "timestamp": "2026-01-20T07:30:00Z"
}

// Success with pagination
{
    "isSuccess": true,
    "message": "Items retrieved successfully",
    "data": [...],
    "metadata": {
        "currentPage": 1,
        "pageSize": 10,
        "totalItems": 100,
        "totalPages": 10,
        "hasNextPage": true,
        "hasPreviousPage": false
    },
    "timestamp": "2026-01-20T07:30:00Z"
}

// Error Response
{
    "isSuccess": false,
    "message": "Product with ID 123 was not found",
    "errors": ["validation error 1", "validation error 2"],
    "timestamp": "2026-01-20T07:30:00Z"
}
```

---

## 7. BuildingBlocks (Shared Libraries)

### 7.1 Contracts

Chứa interfaces và domain primitives dùng chung:

```
Contracts/
├── Common/
│   └── Interface/
│       ├── IRepositoryBaseAsync.cs    # Repository interface
│       ├── IUnitOfWork.cs             # Unit of Work
│       └── IEventEntity.cs            # Domain Events
├── Domain/
│   ├── EntityBase.cs                  # Base entity
│   ├── EntityAuditBase.cs             # Entity với audit
│   ├── Interface/
│   │   ├── IBusinessRule.cs           # Business Rule interface
│   │   └── ISpecification.cs          # Specification interface
│   └── Rules/
│       └── CompositeRule.cs           # And/Or rules
└── Exceptions/
    ├── BusinessException.cs
    ├── ValidationException.cs
    └── NotFoundException.cs
```

### 7.2 Infrastructure

Chứa common infrastructure code:

```
Infrastructure/
├── Authorization/
│   ├── BasePolicy.cs              # Base class cho policies
│   └── PolicyEvaluator.cs
├── Common/
│   ├── ApiControllerBase.cs       # Base controller
│   ├── Behavior/
│   │   ├── ValidationBehaviour.cs
│   │   └── PerformanceBehaviour.cs
│   └── Repository/
│       └── RepositoryBaseAsync.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Middlewares/
    └── ExceptionHandlingMiddleware.cs
```

### 7.3 Shared

Chứa DTOs và shared utilities:

```
Shared/
├── Attributes/
│   ├── PolicyAttribute.cs         # [Policy("NAME")]
│   └── RequirePolicyAttribute.cs  # [RequirePolicy("NAME")]
├── DTOs/
│   ├── Product/
│   │   ├── ProductCreateDto.cs
│   │   ├── ProductUpdateDto.cs
│   │   └── ProductResponseDto.cs
│   └── Authorization/
│       └── UserClaimsContext.cs
├── Events/
│   └── Product/
│       ├── ProductCreatedEvent.cs
│       └── ProductUpdatedEvent.cs
└── SeedWork/
    ├── ApiResult.cs
    ├── ApiSuccessResult.cs
    ├── ApiErrorResult.cs
    ├── PagedList.cs
    └── ResponseMessages.cs
```

---

## 8. Quy tắc Đặt tên

### 8.1 Naming Conventions

| Loại | Convention | Ví dụ |
|------|------------|-------|
| Entities | PascalCase, danh từ số ít | `Product`, `Order`, `Customer` |
| DTOs | PascalCase + Dto suffix | `ProductCreateDto`, `ProductResponseDto` |
| Commands | PascalCase + Command suffix | `CreateProductCommand` |
| Queries | PascalCase + Query suffix | `GetProductsQuery` |
| Handlers | Command/Query + Handler | `CreateProductCommandHandler` |
| Validators | Command/Query + Validator | `CreateProductValidator` |
| Repositories | I + Entity + Repository | `IProductRepository` |
| Rules | Entity + RuleName + Rule | `ProductCanBeDeletedRule` |
| Specifications | ConditionName + Specification | `BelongsToCategorySpecification` |
| Policies | Entity + Action + Policy | `ProductViewPolicy` |
| Events | Entity + Action + Event | `ProductCreatedEvent` |

### 8.2 Folder Structure Convention

```
Features/
└── {EntityPlural}/              # Products, Orders, Categories
    ├── Commands/
    │   └── {Action}{Entity}/    # CreateProduct, UpdateProduct
    ├── Queries/
    │   └── Get{Entity}ById/     # GetProductById
    │   └── Get{EntityPlural}/   # GetProducts
    ├── EventHandlers/
    └── Policies/

Domain/
└── {EntityPlural}/              # Products
    ├── {Entity}.cs              # Product.cs
    ├── {Entity}Error.cs         # ProductError.cs
    ├── Interfaces/
    │   └── I{Entity}Repository.cs
    ├── Rules/
    │   └── {Entity}{RuleName}Rule.cs
    └── Specifications/
        └── {ConditionName}Specification.cs
```

### 8.3 Policy Naming

```
Format: {RESOURCE}:{ACTION}

Ví dụ:
- PRODUCT:VIEW
- PRODUCT:CREATE
- PRODUCT:UPDATE
- PRODUCT:DELETE
- ORDER:APPROVE
- REPORT:EXPORT
```

---

## 9. Flow xử lý Request

### 9.1 Create Product Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. CLIENT: POST /api/products                                   │
│    Body: { "name": "iPhone 15", "categoryId": 1 }              │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. ProductController                                            │
│    - Map ProductCreateDto → CreateProductCommand                │
│    - await HandleCreateAsync(command, ...)                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. MediatR Pipeline                                             │
│    - ValidationBehaviour: Validate với CreateProductValidator   │
│    - PerformanceBehaviour: Log slow requests                    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. CreateProductCommandHandler (Application Layer)             │
│    - Load Category từ repository                                │
│    - Gọi Product.Create() - Factory Method                      │
│    - Save via ProductRepository                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 5. Product.Create() (Domain Layer)                              │
│    - Tạo Product instance                                       │
│    - Apply business rules nếu cần                               │
│    - Raise ProductCreatedEvent (domain event)                   │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 6. ProductRepository.CreateAsync() (Infrastructure)            │
│    - _dbContext.Products.Add(product)                           │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 7. DbContext.SaveChangesAsync()                                 │
│    - Auto-set CreatedDate                                       │
│    - Save to database                                           │
│    - Dispatch domain events (ProductCreatedEvent)               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 8. ProductCreatedEventHandler                                   │
│    - Log event                                                  │
│    - Invalidate cache                                           │
│    - Other side effects                                         │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│ 9. Response: 201 Created                                        │
│    { "isSuccess": true, "data": 123, "message": "..." }        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 10. Hướng dẫn Tạo Feature Mới

### 10.1 Checklist tạo Entity mới

Giả sử tạo entity `Supplier`:

#### Step 1: Domain Layer

```
□ 1.1 Tạo Supplier.cs (Entity)
□ 1.2 Tạo SupplierError.cs (Business Exceptions)
□ 1.3 Tạo Interfaces/ISupplierRepository.cs
□ 1.4 Tạo Rules/ (nếu cần business rules)
□ 1.5 Tạo Specifications/ (nếu cần query specs)
```

#### Step 2: Application Layer

```
□ 2.1 Tạo Features/Suppliers/Commands/CreateSupplier/
    - CreateSupplierCommand.cs
    - CreateSupplierCommandHandler.cs
    - CreateSupplierValidator.cs

□ 2.2 Tạo Features/Suppliers/Commands/UpdateSupplier/
    - UpdateSupplierCommand.cs
    - UpdateSupplierCommandHandler.cs
    - UpdateSupplierValidator.cs

□ 2.3 Tạo Features/Suppliers/Commands/DeleteSupplier/
    - DeleteSupplierCommand.cs
    - DeleteSupplierCommandHandler.cs

□ 2.4 Tạo Features/Suppliers/Queries/GetSuppliers/
    - GetSuppliersQuery.cs
    - GetSuppliersQueryHandler.cs

□ 2.5 Tạo Features/Suppliers/Queries/GetSupplierById/
    - GetSupplierByIdQuery.cs
    - GetSupplierByIdQueryHandler.cs

□ 2.6 Tạo Features/Suppliers/Policies/ (nếu cần authorization)
    - SupplierViewPolicy.cs
    - SupplierCreatePolicy.cs
```

#### Step 3: Infrastructure Layer

```
□ 3.1 Tạo Repositories/SupplierRepository.cs
□ 3.2 Tạo Persistences/Configurations/SupplierConfiguration.cs
□ 3.3 Update GenerateContext.cs - thêm DbSet<Supplier>
□ 3.4 Update ConfigureServices.cs - đăng ký DI
□ 3.5 Tạo Migration
```

#### Step 4: API Layer

```
□ 4.1 Tạo Controllers/SupplierController.cs
```

#### Step 5: Shared (BuildingBlocks)

```
□ 5.1 Tạo DTOs/Supplier/SupplierCreateDto.cs
□ 5.2 Tạo DTOs/Supplier/SupplierUpdateDto.cs
□ 5.3 Tạo DTOs/Supplier/SupplierResponseDto.cs
□ 5.4 Tạo Events/Supplier/ (nếu cần domain events)
```

---

## 11. Best Practices

### 11.1 Domain Layer

```csharp
// ✅ DO: Use Factory Methods
public static Product Create(string name) => new Product(name);

// ❌ DON'T: Public constructor với public setters
public Product() { }

// ✅ DO: Encapsulate collections
private readonly List<OrderItem> _items = new();
public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

// ❌ DON'T: Expose mutable collections
public List<OrderItem> Items { get; set; }

// ✅ DO: Validate with Business Rules
public void UpdatePrice(decimal price)
{
    CheckRule(new PriceMustBePositiveRule(price));
    _price = price;
}

// ❌ DON'T: Skip validation
public void UpdatePrice(decimal price)
{
    _price = price;  // No validation!
}
```

### 11.2 Application Layer

```csharp
// ✅ DO: Keep handlers focused
public async Task<long> Handle(CreateProductCommand request, CancellationToken ct)
{
    var product = Product.Create(request.Name);
    await _repository.CreateAsync(product);
    return product.Id;
}

// ❌ DON'T: Put business logic in handlers
public async Task<long> Handle(CreateProductCommand request, CancellationToken ct)
{
    if (string.IsNullOrEmpty(request.Name))  // Should be in Domain
        throw new Exception("Name required");
    // ...
}

// ✅ DO: Use FluentValidation for input validation
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```

### 11.3 API Layer

```csharp
// ✅ DO: Thin controllers
[HttpPost]
public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
{
    var command = new CreateProductCommand { Name = dto.Name };
    return await HandleCreateAsync(command, EntityName, dto.Name);
}

// ❌ DON'T: Business logic in controllers
[HttpPost]
public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
{
    if (dto.Price < 0)  // Should be in Validator
        return BadRequest("Invalid price");
        
    var product = new Product { Name = dto.Name };  // Should use Factory
    // ...
}
```

### 11.4 General

```csharp
// ✅ DO: Constructor injection
public class ProductRepository
{
    private readonly GenerateContext _context;
    
    public ProductRepository(GenerateContext context)
    {
        _context = context;
    }
}

// ✅ DO: Use async/await consistently
public async Task<Product?> GetByIdAsync(long id)
{
    return await _context.Products.FindAsync(id);
}

// ✅ DO: Use CancellationToken
public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
{
    return await _context.Products.ToListAsync(ct);
}
```

---

## 📎 Tài liệu tham khảo

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design - Eric Evans](https://domainlanguage.com/ddd/)
- [CQRS Pattern - Microsoft](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)

---

> 📝 **Lưu ý:** Tài liệu này được cập nhật ngày 2026-01-20. Vui lòng kiểm tra với team lead nếu có thay đổi.
