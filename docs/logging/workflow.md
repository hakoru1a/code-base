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
            (2) CorrelationIdMiddleware
                          |
           +-----------------------------+
           | - Nếu không có X-Correlation-ID -> Tạo mới |
           | - Thêm CorrelationId vào LogContext      |
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
|                         |                          |                          |
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
            |      (Tìm kiếm, lọc theo CorrelationId, tạo Dashboard)     |
            +------------------------------------------------------+
```

## 👣 Giải thích các bước

1.  **Client gửi Request**: Người dùng hoặc một hệ thống khác gửi một HTTP request đến điểm vào của hệ thống (thường là API Gateway hoặc BFF). Request này có thể tùy chọn chứa header `X-Correlation-ID` nếu nó là một phần của một chuỗi giao dịch đã tồn tại.

2.  **CorrelationIdMiddleware**:
    *   Đây là middleware đầu tiên trong pipeline xử lý request.
    *   Nó kiểm tra sự tồn tại của header `X-Correlation-ID`.
    *   Nếu header **không tồn tại**, middleware sẽ tạo một giá trị `Guid` mới và gán làm `CorrelationId`.
    *   Middleware sử dụng `LogContext.PushProperty("CorrelationId", ...)` của Serilog. Thao tác này sẽ tự động đính kèm `CorrelationId` vào **tất cả** các bản ghi log được tạo ra trong phạm vi (scope) của request này.
    *   Request được chuyển tiếp đến các middleware và service tiếp theo, luôn mang theo header `X-Correlation-ID`.

3.  **Logging trong Microservice A**: Khi code trong Service A gọi `_logger.LogInformation(...)`, Serilog sẽ tự động lấy `CorrelationId` từ `LogContext` và thêm nó vào bản ghi log.

4.  **Logging trong Microservice B, C**: Tương tự, khi request được chuyển tiếp đến các service khác (ví dụ qua HTTP Client), header `X-Correlation-ID` phải được truyền theo. Middleware ở các service này sẽ lặp lại quy trình ở bước (2), đảm bảo `CorrelationId` được duy trì xuyên suốt.

5.  **Serilog Elasticsearch Sink**:
    *   Serilog không ghi log trực tiếp ra file văn bản. Thay vào đó, nó sử dụng một "Sink" đã được cấu hình.
    *   Elasticsearch Sink sẽ thu thập các bản ghi log, định dạng chúng thành JSON và gửi chúng đến Elasticsearch theo lô (batch) để tối ưu hiệu suất.

6.  **Gửi log có cấu trúc (JSON)**: Đây là một ví dụ về bản ghi log được gửi đến Elasticsearch. Lưu ý rằng nó là dữ liệu có cấu trúc, không phải là một chuỗi văn bản thuần túy.

    ```json
    {
      "@timestamp": "2025-11-23T10:00:00.123Z",
      "level": "Information",
      "messageTemplate": "Processing order for user {UserId} with total amount {Amount}",
      "message": "Processing order for user \"user-123\" with total amount 150.99",
      "fields": {
        "UserId": "user-123",
        "Amount": 150.99,
        "Application": "OrderService",
        "MachineName": "PROD-SERVER-01",
        "CorrelationId": "a1b2c3d4-e5f6-7890-1234-56789abcdef0"
      }
    }
    ```

7.  **Elasticsearch Cluster**: Elasticsearch nhận dữ liệu JSON, phân tích (parse) và index nó. Việc này giúp cho việc tìm kiếm sau này cực kỳ nhanh chóng.

8.  **Kibana**: Người dùng (Dev, QA, DevOps) mở Kibana, kết nối đến Elasticsearch và có thể thực hiện các truy vấn mạnh mẽ, ví dụ:
    *   `CorrelationId: "a1b2c3d4-e5f6-7890-1234-56789abcdef0"` để xem tất cả log của một request.
    *   `level: "Error" AND fields.Application: "PaymentService"` để tìm tất cả lỗi trong service thanh toán.
    *   Tạo biểu đồ để theo dõi số lượng lỗi theo thời gian.
