# Logging Workflow

Sơ đồ và giải thích dưới đây mô tả luồng hoạt động hoàn chỉnh của một bản ghi log, từ khi request được gửi đi cho đến khi nó xuất hiện trong Kibana.

## 🌊 Sơ đồ luồng dữ liệu (Data Flow)

```
            +---------------------------+
            |      Client / User        |
            +-------------+-------------+
                          |
            (1) Gửi Request (với X-Correlation-ID nếu có)
                          |
                          v
            +---------------------------+
            |      API Gateway / BFF    |
            +-------------+-------------+
                          |
            (2) LoggingContextMiddleware
                          |
           +-----------------------------+
           | - Nếu không có X-Correlation-ID -> Tạo mới |
           | - Extract Username từ JWT Claims         |
           | - Thêm CorrelationId + Username vào LogContext |
           | - Forward request + X-Correlation-ID     |
           +-----------------------------+
                          |
                          v
+-------------------------+--------------------------+--------------------------+
|      Microservice A     |     Microservice B       |      Microservice C      |
|                         |                          |                          |
| (3) Logger.LogInfo(...) | (4) Logger.LogWarn(...)  | (5) Logger.LogError(...) |
|     - Log có chứa       |     - Log có chứa        |     - Log có chứa        |
|       CorrelationId     |       CorrelationId      |       CorrelationId      |
|       Username          |       Username           |       Username           |
|       ClientIP          |       ClientIP           |       ClientIP           |
|       RequestPath       |       RequestPath        |       RequestPath        |
+------------+------------+-------------+------------+-------------+------------+
             |                           |                          |
             |           (6) Serilog Elasticsearch Sink             |
             +---------------------------+--------------------------+
                                         |
                                (7) Gửi log có cấu trúc (JSON)
                                         |
                                         v
            +------------------------------------------------------+
            |                   Elasticsearch Cluster                |
            |     (Lưu trữ và index các bản ghi log dưới dạng JSON)    |
            +-----------------------------+--------------------------+
                                          |
                              (8) User truy vấn và trực quan hóa
                                          |
                                          v
            +------------------------------------------------------+
            |                           Kibana                       |
            |      (Tìm kiếm, lọc theo CorrelationId/Username, tạo Dashboard) |
            +------------------------------------------------------+
```

## 👣 Giải thích các bước

1.  **Client gửi Request**: Người dùng hoặc một hệ thống khác gửi một HTTP request đến điểm vào của hệ thống (thường là API Gateway hoặc BFF). Request này có thể tùy chọn chứa header `X-Correlation-ID` nếu nó là một phần của một chuỗi giao dịch đã tồn tại.

2.  **LoggingContextMiddleware** (Cập nhật mới):
    *   Đây là middleware quan trọng trong pipeline xử lý request.
    *   **Correlation ID**: Kiểm tra sự tồn tại của header `X-Correlation-ID`. Nếu không có, tạo một giá trị duy nhất mới.
    *   **Username**: Extract username từ JWT claims trong `HttpContext.User` (sau khi authentication middleware đã chạy).
    *   **Request Info**: Thu thập thông tin request như ClientIP, UserAgent, RequestPath, RequestMethod.
    *   Sử dụng `LogContext.PushProperty(...)` của Serilog để thêm tất cả thông tin này vào mọi log trong scope của request.
    *   Request được chuyển tiếp đến các middleware và service tiếp theo, luôn mang theo header `X-Correlation-ID`.

3.  **Logging trong Microservice A**: Khi code trong Service A gọi `_logger.LogInformation(...)`, Serilog sẽ tự động lấy `CorrelationId`, `Username`, và các properties khác từ `LogContext` và thêm chúng vào bản ghi log.

4.  **Logging trong Microservice B, C**: Tương tự, khi request được chuyển tiếp đến các service khác (ví dụ qua HTTP Client với `LoggingDelegatingHandler`), header `X-Correlation-ID` được truyền theo. Middleware ở các service này sẽ lặp lại quy trình ở bước (2), đảm bảo `CorrelationId` và context được duy trì xuyên suốt.

5.  **Serilog Elasticsearch Sink**:
    *   Serilog không ghi log trực tiếp ra file văn bản. Thay vào đó, nó sử dụng một "Sink" đã được cấu hình.
    *   Elasticsearch Sink sẽ thu thập các bản ghi log, định dạng chúng thành JSON và gửi chúng đến Elasticsearch theo lô (batch) để tối ưu hiệu suất.

6.  **Gửi log có cấu trúc (JSON) - Định dạng mới**: Đây là một ví dụ về bản ghi log được gửi đến Elasticsearch với thông tin correlation ID và username:

    ```json
    {
      "@timestamp": "2025-11-24T10:00:00.123Z",
      "level": "Information",
      "messageTemplate": "Processing order for user {UserId} with total amount {Amount}",
      "message": "Processing order for user \"user-123\" with total amount 150.99",
      "fields": {
        "UserId": "user-123",
        "Amount": 150.99,
        "CorrelationId": "a1b2c3d4e5f6",
        "Username": "john.doe",
        "ClientIP": "192.168.1.100",
        "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)...",
        "RequestPath": "/api/orders",
        "RequestMethod": "POST",
        "Application": "OrderService",
        "MachineName": "PROD-SERVER-01",
        "Environment": "Production"
      }
    }
    ```

7.  **Elasticsearch Cluster**: Elasticsearch nhận dữ liệu JSON, phân tích (parse) và index nó. Việc này giúp cho việc tìm kiếm sau này cực kỳ nhanh chóng.

8.  **Kibana - Truy vấn nâng cao**: Người dùng (Dev, QA, DevOps) mở Kibana, kết nối đến Elasticsearch và có thể thực hiện các truy vấn mạnh mẽ:
    *   **Theo correlation ID**: `CorrelationId: "a1b2c3d4e5f6"` để xem tất cả log của một request
    *   **Theo user**: `Username: "john.doe"` để theo dõi hoạt động của một user cụ thể  
    *   **Lỗi của user**: `level: "Error" AND Username: "john.doe"` để tìm lỗi liên quan đến user
    *   **API endpoint**: `RequestPath: "/api/orders" AND RequestMethod: "POST"` để monitor endpoint cụ thể
    *   **Combine filters**: `CorrelationId: "a1b2c3d4e5f6" AND (level: "Error" OR level: "Warning")`
    *   Tạo dashboard để theo dõi user activity, error rates, request patterns theo thời gian.

## 🔍 Truy vấn Kibana Examples

### Dashboard theo User Activity
```kql
# Top users by request count
Username: * | top 10 Username

# User error rate
level: "Error" AND Username: * | stats count by Username

# Most used endpoints by user  
Username: "john.doe" | stats count by RequestPath
```

### Dashboard theo Request Tracking
```kql
# Request flow theo correlation ID
CorrelationId: "a1b2c3d4e5f6" | sort @timestamp

# Average response time theo endpoint
RequestPath: * | stats avg(responseTime) by RequestPath

# Error correlation
level: "Error" | stats count by CorrelationId | sort count desc
```
