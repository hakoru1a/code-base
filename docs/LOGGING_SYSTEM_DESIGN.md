# Hệ thống Logging - Thiết kế Kỹ thuật & Yêu cầu

## 1. Tổng quan & Mục tiêu

Tài liệu này trình bày thiết kế kỹ thuật cho một hệ thống logging tập trung và có cấu trúc. Các mục tiêu chính bao gồm:
- **Truy vết Request từ đầu đến cuối (End-to-End Request Tracing)**: Theo dõi một request của người dùng khi nó đi qua API Gateway và các dịch vụ backend khác nhau.
- **Lưu trữ Log tập trung**: Tổng hợp log từ tất cả các dịch vụ vào một nơi duy nhất, có khả năng tìm kiếm.
- **Dữ liệu có cấu trúc, chi tiết**: Ghi lại các sự kiện log dưới dạng JSON có cấu trúc (thay vì văn bản thuần túy) để cho phép truy vấn và phân tích mạnh mẽ.
- **Phân tích & Trực quan hóa**: Sử dụng một giao diện trực quan để tìm kiếm, lọc và phân tích dữ liệu log nhằm mục đích gỡ lỗi, theo dõi hiệu năng và điều tra lỗi.

## 2. Các thành phần chính

| Thành phần | Công nghệ | Mục đích |
|---|---|---|
| **Tạo Log** | **Serilog** | Tạo các sự kiện log có cấu trúc bên trong ứng dụng .NET. |
| **Truy vết Request**| **Correlation ID** | Một ID duy nhất được gán cho mỗi request để liên kết tất cả các log liên quan. |
| **Lưu trữ Log** | **Elasticsearch** | Một công cụ tìm kiếm mạnh mẽ để lưu trữ, lập chỉ mục và truy vấn dữ liệu log ở quy mô lớn. |
| **Trực quan hóa Log**| **Kibana** | Giao diện web để tìm kiếm, phân tích và trực quan hóa dữ liệu được lưu trong Elasticsearch. |

---

## 3. Kế hoạch triển khai

### A. Bước 1: Triển khai Correlation ID Middleware

Chúng ta sẽ tạo một middleware tùy chỉnh để tạo và quản lý `CorrelationId`.

**Tệp cần tạo:** `src/BuildingBlocks/Infrastructure/Middlewares/CorrelationIdMiddleware.cs`

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace Infrastructure.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Cố gắng lấy Correlation ID từ header của request đến
            var correlationId = GetOrGenerateCorrelationId(context);

            // 2. Thêm Correlation ID vào HttpContext để các thành phần khác có thể truy cập
            context.Items["CorrelationId"] = correlationId;

            // 3. Thêm Correlation ID vào Serilog LogContext
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                // 4. Thêm Correlation ID vào header của response
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
                    {
                        context.Response.Headers.Add(CorrelationIdHeaderKey, correlationId);
                    }
                    return Task.CompletedTask;
                });

                await _next(context);
            }
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationId) && !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.ToString();
            }
            
            return Guid.NewGuid().ToString("D");
        }
    }
}
```

**Đăng ký Middleware trong `Program.cs` của mỗi dịch vụ (ví dụ: `Base.API`, `ApiGateway`):**
```csharp
// Trong Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();

// ... các middleware khác
app.Run();
```

### B. Bước 2: Cấu hình Serilog cho Elasticsearch

Chúng ta cần cấu hình Serilog để gửi log đến Elasticsearch.

**1. Thêm gói NuGet:**
```powershell
dotnet add package Serilog.Sinks.Elasticsearch
```

**2. Cập nhật cấu hình Serilog:**
Sửa đổi tệp `src/BuildingBlocks/Logging/SeriLogger.cs` và `appsettings.json`.

**`appsettings.json` (cho tất cả các dịch vụ):**
```json
{
  "ElasticConfiguration": {
    "Uri": "http://localhost:9200",
    "IndexFormat": "codebase-logs-{0:yyyy.MM.dd}",
    "Username": "elastic",
    "Password": "changeme" 
  }
}
```

**`SeriLogger.cs`:**
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

namespace Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
           (context, configuration) =>
           {
               var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");
               var indexFormat = context.Configuration.GetValue<string>("ElasticConfiguration:IndexFormat");

               configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        IndexFormat = indexFormat,
                        AutoRegisterTemplate = true,
                        NumberOfShards = 2,
                        NumberOfReplicas = 1
                    })
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                    .ReadFrom.Configuration(context.Configuration);
           };
    }
}
```
*Lưu ý: `Enrich.FromLogContext()` kết hợp với `LogContext.PushProperty` trong middleware chính là cơ chế thêm `CorrelationId` vào mỗi thông điệp log.*

### C. Bước 3: Cài đặt Hạ tầng với Docker

Thêm Elasticsearch và Kibana vào thiết lập Docker của dự án.

**Tệp cần tạo/cập nhật:** `infra/monitoring/elastic-search.yml`
```yaml
version: '3.8'

services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    container_name: elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false # Chỉ dùng cho môi trường development
      - 'ES_JAVA_OPTS=-Xms512m -Xmx512m'
    ports:
      - '9200:9200'
    networks:
      - monitoring-net

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    container_name: kibana
    ports:
      - '5601:5601'
    depends_on:
      - elasticsearch
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    networks:
      - monitoring-net

networks:
  monitoring-net:
    driver: bridge
```

**Chạy Docker Compose:**
```bash
docker-compose -f infra/monitoring/elastic-search.yml up -d
```

### D. Bước 4: Trực quan hóa Logs trên Kibana

**1. Truy cập Kibana:**
- Mở trình duyệt và truy cập `http://localhost:5601`.

**2. Tạo một Index Pattern:**
- Đi đến **Management > Stack Management > Kibana > Index Patterns**.
- Nhấp vào **"Create index pattern"**.
- Sử dụng pattern `codebase-logs-*` để khớp với các index được tạo bởi Serilog.
- Chọn `@timestamp` làm trường thời gian (time field).

**3. Khám phá và Truy vết Logs:**
- Chuyển đến tab **Discover**.
- Bây giờ bạn có thể thấy tất cả các thông điệp log từ các dịch vụ của mình.
- Để truy vết một request duy nhất, hãy tìm một `CorrelationId` từ một thông điệp log và sử dụng thanh tìm kiếm Kibana Query Language (KQL):
  ```kql
  CorrelationId : "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  ```
- Thao tác này sẽ lọc tất cả các mục log cho request cụ thể đó, hiển thị toàn bộ hành trình của nó.

---

## 4. Tóm tắt các công việc cần làm

- [ ] Tạo tệp `CorrelationIdMiddleware.cs`.
- [ ] Đăng ký middleware trong các tệp `Program.cs` của tất cả dịch vụ.
- [ ] Thêm gói NuGet `Serilog.Sinks.Elasticsearch` vào tất cả các dự án dịch vụ.
- [ ] Cập nhật `appsettings.json` trong tất cả các dịch vụ với `ElasticConfiguration`.
- [ ] Cập nhật `SeriLogger.cs` để bao gồm sink của Elasticsearch.
- [ ] Tạo/cập nhật tệp `infra/monitoring/elastic-search.yml`.
- [ ] Chạy tệp Docker Compose mới để khởi động Elasticsearch và Kibana.
- [ ] Cấu hình index pattern trong Kibana.
