# Hướng Dẫn Sử Dụng Logging System với Correlation ID và Username

## 📝 Tổng Quan

Hệ thống logging đã được nâng cấp với middleware `LoggingContextMiddleware` để tự động thêm correlation ID và username vào mọi log entry. Điều này giúp:

- **Tracking request**: Dễ dàng trace toàn bộ flow của một request qua các microservices
- **User activity**: Biết được log nào thuộc về user nào
- **Debugging**: Nhanh chóng tìm ra tất cả logs liên quan đến một issue cụ thể

## 🚀 Setup

### 1. Middleware đã được tích hợp

Middleware đã được thêm vào:
- ✅ **ApiGateway**: `f:\base\CodeBase\src\ApiGateways\ApiGateway\Program.cs`
- ✅ **Auth.API**: `f:\base\CodeBase\src\Services\Auth\Auth.API\Program.cs`  
- ✅ **Generate.API**: `f:\base\CodeBase\src\Services\Generate\Generate.API\Extensions\ApplicationExtension.cs`

### 2. Thứ tự middleware quan trọng

```csharp
app.UseRouting();
app.UseLoggingContext();           // ← Thêm correlation ID và username
app.UseAuthentication();           // ← Phải có để LoggingContext lấy được username
app.UseAuthorization();
```

## 📊 Kết Quả

### Log format mới

Mỗi log entry giờ đây sẽ có thêm các properties:

```json
{
  "@timestamp": "2025-11-24T10:30:45.123Z",
  "level": "Information", 
  "messageTemplate": "Processing order {OrderId}",
  "message": "Processing order ORD-12345",
  "fields": {
    "CorrelationId": "a1b2c3d4e5f6",     // ← Tự động thêm
    "Username": "john.doe",               // ← Tự động thêm  
    "UserAgent": "Mozilla/5.0...",        // ← Tự động thêm
    "ClientIP": "192.168.1.100",          // ← Tự động thêm
    "RequestPath": "/api/orders",         // ← Tự động thêm
    "RequestMethod": "POST",              // ← Tự động thêm
    "OrderId": "ORD-12345",              // ← Từ structured logging
    "Application": "generate-api",
    "Environment": "Development"
  }
}
```

### Response headers

Mỗi response sẽ có header:
```
X-Correlation-Id: a1b2c3d4e5f6
```

## 🔍 Cách Sử Dụng trong Code

### 1. Logging bình thường (đã có correlation ID tự động)

```csharp
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // ✅ Log này tự động có CorrelationId và Username
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);
        
        try
        {
            var order = await _orderService.CreateAsync(request);
            
            // ✅ Log success với structured data  
            _logger.LogInformation("Order created successfully {@Order}", order);
            
            return Ok(order);
        }
        catch (Exception ex)
        {
            // ✅ Log error cũng có CorrelationId và Username
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
```

### 2. Thêm custom properties vào log context

```csharp
using Serilog.Context;

[HttpPost("process/{orderId}")]
public async Task<IActionResult> ProcessOrder(string orderId)
{
    // Thêm OrderId vào log context cho tất cả logs trong method này
    using var orderScope = LogContext.PushProperty("OrderId", orderId);
    using var operationScope = LogContext.PushProperty("Operation", "ProcessOrder");
    
    _logger.LogInformation("Starting order processing");
    
    // Tất cả logs từ đây đều có OrderId và Operation
    await ValidateOrder(orderId);
    await ProcessPayment(orderId);  
    await ShipOrder(orderId);
    
    _logger.LogInformation("Order processing completed");
    
    return Ok();
}
```

### 3. Lấy correlation ID trong code

```csharp
[HttpGet("debug")]
public IActionResult GetDebugInfo()
{
    // Lấy correlation ID
    var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ??
                       HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ??
                       HttpContext.TraceIdentifier;
    
    // Lấy username  
    var username = User.Identity?.Name ?? "anonymous";
    
    return Ok(new { CorrelationId = correlationId, Username = username });
}
```

## 🔎 Query Logs trong Kibana

### 1. Tìm tất cả logs của một correlation ID

```kql
CorrelationId: "a1b2c3d4e5f6"
```

### 2. Tìm logs của một user

```kql
Username: "john.doe"
```

### 3. Tìm logs lỗi của một user

```kql
Username: "john.doe" AND level: "Error"
```

### 4. Tìm logs của một API endpoint

```kql
RequestPath: "/api/orders" AND RequestMethod: "POST"
```

### 5. Combine multiple filters

```kql
CorrelationId: "a1b2c3d4e5f6" AND (level: "Error" OR level: "Warning")
```

## 📈 Dashboard Suggestions

### 1. Request Tracking Dashboard

- **Panel 1**: Request volume by correlation ID
- **Panel 2**: Average request duration by endpoint
- **Panel 3**: Error rate by correlation ID

### 2. User Activity Dashboard  

- **Panel 1**: Top active users by request count
- **Panel 2**: User error rates
- **Panel 3**: Most used endpoints by user

### 3. Error Analysis Dashboard

- **Panel 1**: Error count by correlation ID
- **Panel 2**: Error patterns by username
- **Panel 3**: Error distribution by endpoint

## ⚡ Performance Impact

- **Memory**: ~50 bytes per request (correlation ID + username storage)
- **CPU**: Negligible (simple string operations)
- **Network**: +24 bytes per HTTP response (X-Correlation-Id header)

## 🔒 Security Notes

### Username Privacy

- Username được lấy từ JWT claims, không phải sensitive data
- Nếu cần hide username, có thể hash hoặc mask trong middleware

### Correlation ID

- Correlation ID là random GUID, không chứa thông tin nhạy cảm
- Có thể dùng để track user behavior nhưng không identify trực tiếp

## 🐛 Troubleshooting

### Username hiển thị "anonymous"

**Nguyên nhân**: JWT claims không có username hoặc middleware đặt sai thứ tự

**Giải pháp**:
1. Kiểm tra JWT có chứa claims `name`, `preferred_username`, hoặc `username`
2. Đảm bảo `app.UseLoggingContext()` đặt sau `app.UseAuthentication()`

### Correlation ID không consistent qua services

**Nguyên nhân**: `LoggingDelegatingHandler` không được đăng ký hoặc thiếu `IHttpContextAccessor`

**Giải pháp**:
1. Đảm bảo `services.AddHttpContextAccessor()` được gọi
2. Đăng ký `LoggingDelegatingHandler` cho HttpClient:

```csharp
builder.Services.AddHttpClient("MyService")
    .AddHttpMessageHandler<LoggingDelegatingHandler>();
```

### Logs không có correlation ID properties

**Nguyên nhân**: Serilog chưa được configure với `FromLogContext`

**Giải pháy**:
Kiểm tra `SeriLogger.cs` có `.Enrich.FromLogContext()`:

```csharp
configuration
    .Enrich.FromLogContext()  // ← Cần có dòng này
    .WriteTo.Elasticsearch(...)
```

## 📚 Tham Khảo

- [Serilog Structured Logging](https://serilog.net/)
- [ASP.NET Core Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Correlation ID Pattern](https://microservices.io/patterns/observability/correlation-id.html)