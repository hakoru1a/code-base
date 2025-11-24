# Logging Quick Start

Hướng dẫn này giúp bạn nhanh chóng tích hợp hệ thống logging vào một service .NET mới.

## 📦 Cài đặt các thư viện cần thiết

Sử dụng NuGet Package Manager để cài đặt các gói sau:

```bash
# Thư viện Serilog chính
dotnet add package Serilog.AspNetCore

# Sink để ghi log ra Console
dotnet add package Serilog.Sinks.Console

# Sink để ghi log vào Elasticsearch
dotnet add package Serilog.Sinks.Elasticsearch

# Thư viện để tự động thêm các thuộc tính vào log
dotnet add package Serilog.Enrichers.Environment
```

## ⚙️ Cấu hình trong `appsettings.json`

Thêm đoạn cấu hình sau vào file `appsettings.json` của bạn. Đây là cấu hình tối thiểu để ghi log ra Console và Elasticsearch.

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "Elasticsearch",
      "Args": {
        "nodeUris": "http://localhost:9200", // <-- Thay đổi địa chỉ Elasticsearch của bạn ở đây
        "indexFormat": "your-app-logs-{0:yyyy.MM.dd}",
        "autoRegisterTemplate": true
      }
    }
  ],
  "Properties": {
    "Application": "YourAppName" // <-- Thay đổi tên ứng dụng của bạn
  }
}
```

## 🚀 Khởi tạo Serilog trong `Program.cs`

Trong file `Program.cs`, cấu hình để ứng dụng sử dụng Serilog.

```csharp
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        // Đọc cấu hình từ appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // Tạo logger từ cấu hình
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog() // <-- Sử dụng Serilog
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

## ✍️ Ghi một bản log đầu tiên

Trong một controller hoặc service bất kỳ, inject `ILogger` và bắt đầu ghi log.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        // Ghi log với cấu trúc
        _logger.LogInformation("Getting weather forecast for {Count} days", 5);

        // ... logic của bạn
    }
}
```

## 🔗 Thêm Correlation ID vào Log

Để theo dõi một request qua nhiều service, chúng ta cần thêm `CorrelationId`.

### 1. Tạo Middleware
Tạo một middleware để kiểm tra `Correlation-ID` trong header của request. Nếu không có, nó sẽ tạo một ID mới.

```csharp
// CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetOrSetCorrelationId(context);
        
        // Thêm CorrelationId vào LogContext để Serilog có thể tự động đính kèm vào mỗi log
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private string GetOrSetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdValues))
        {
            return correlationIdValues.FirstOrDefault();
        }
        
        var newCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers.Add(CorrelationIdHeaderName, newCorrelationId);
        return newCorrelationId;
    }
}
```

### 2. Đăng ký Middleware trong `Startup.cs`
Đăng ký middleware này vào pipeline xử lý request.

```csharp
// Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...
    
    // Thêm middleware này vào đầu pipeline
    app.UseMiddleware<CorrelationIdMiddleware>();
    
    app.UseRouting();
    // ...
}
```

**Xong!** Giờ đây, tất cả các bản ghi log được tạo trong phạm vi của một request sẽ tự động có thuộc tính `CorrelationId`.
