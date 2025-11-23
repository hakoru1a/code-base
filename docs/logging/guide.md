# Logging Guide

## 📖 Table of Contents
1. [Giới thiệu](#-giới-thiệu-về-hệ-thống-logging)
2. [Các thành phần chính](#-các-thành-phần-chính)
3. [Cấu hình chi tiết](#-cấu-hình-chi-tiết)
4. [Best Practices](#-best-practices)
5. [Troubleshooting](#-troubleshooting)

---

## 🎯 Giới thiệu về Hệ thống Logging

Hệ thống logging của chúng ta được thiết kế để cung cấp một giải pháp ghi log tập trung, có cấu trúc và dễ dàng truy vấn. Mục tiêu là cho phép các developer và DevOps theo dõi luồng hoạt động của ứng dụng, chẩn đoán lỗi nhanh chóng và giám sát hiệu suất hệ thống một cách hiệu quả.

Chúng ta sử dụng một stack công nghệ mạnh mẽ và phổ biến trong ngành: **Serilog + Elasticsearch + Kibana**.

---

## 🧩 Các thành phần chính

### 1. Serilog
**Serilog** là một thư viện logging cho .NET với khả năng ghi log có cấu trúc (structured logging) mạnh mẽ. Thay vì ghi log dưới dạng văn bản thuần túy, Serilog ghi lại các sự kiện log dưới dạng dữ liệu có cấu trúc (thường là JSON), giúp việc lọc và truy vấn trở nên cực kỳ hiệu quả.

- **Website**: [https://serilog.net/](https://serilog.net/)

### 2. Elasticsearch
**Elasticsearch** là một công cụ tìm kiếm và phân tích phân tán. Trong stack của chúng ta, nó đóng vai trò là nơi lưu trữ tập trung tất cả các log từ mọi service. Khả năng tìm kiếm full-text và hiệu năng cao của nó cho phép chúng ta truy vấn hàng triệu bản ghi log trong vài giây.

- **Website**: [https://www.elastic.co/elasticsearch/](https://www.elastic.co/elasticsearch/)

### 3. Kibana
**Kibana** là một giao diện người dùng web cho Elasticsearch. Nó cho phép chúng ta trực quan hóa dữ liệu log, tạo các dashboard giám sát, và thực hiện các truy vấn phức tạp bằng một giao diện thân thiện. Đây là công cụ chính mà chúng ta sẽ sử dụng để xem và phân tích log.

- **Website**: [https://www.elastic.co/kibana/](https://www.elastic.co/kibana/)

### 4. Correlation ID
**Correlation ID** (còn gọi là Request ID hoặc Tracking ID) là một mã định danh duy nhất được gán cho mỗi request khi nó đi vào hệ thống. ID này sẽ được đính kèm vào **mọi bản ghi log** thuộc về request đó, trên tất cả các service mà request đi qua. Điều này cho phép chúng ta:
-   Dễ dàng truy vết toàn bộ luồng xử lý của một request, từ API Gateway cho đến các microservice bên trong.
-   Nhanh chóng lọc ra tất cả các log liên quan đến một giao dịch hoặc một luồng nghiệp vụ cụ thể.

---

## ⚙️ Cấu hình chi tiết

Cấu hình Serilog được thực hiện chủ yếu trong file `appsettings.json`.

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Enrich": [
    "FromLogContext",
    "WithMachineName",
    "WithProcessId",
    "WithThreadId"
  ],
  "WriteTo": [
    {
      "Name": "Console"
    },
    {
      "Name": "Elasticsearch",
      "Args": {
        "nodeUris": "http://localhost:9200", // URL của Elasticsearch
        "indexFormat": "your-app-logs-{0:yyyy.MM.dd}",
        "autoRegisterTemplate": true,
        "numberOfShards": 2,
        "numberOfReplicas": 1
      }
    }
  ],
  "Properties": {
    "Application": "YourAppName" // Tên ứng dụng để dễ dàng lọc log
  }
}
```

### Giải thích cấu hình:
-   **`MinimumLevel`**: Thiết lập cấp độ log tối thiểu. `Information` là lựa chọn tốt cho môi trường Production.
-   **`Enrich`**: Tự động thêm các thông tin hữu ích vào mỗi bản ghi log như `MachineName`, `ProcessId`, `ThreadId`. `FromLogContext` là mục quan trọng nhất, cho phép chúng ta thêm các thuộc tính động như `CorrelationId`.
-   **`WriteTo`**: Định nghĩa nơi ghi log ra (gọi là "Sinks"). Ở đây chúng ta ghi ra `Console` (hữu ích khi debug local) và `Elasticsearch`.
-   **`Args`**: Các tham số cho Elasticsearch Sink.
    -   `nodeUris`: Địa chỉ của Elasticsearch cluster.
    -   `indexFormat`: Định dạng tên của index trong Elasticsearch. Phân chia theo ngày giúp quản lý dữ liệu dễ dàng hơn.
-   **`Properties`**: Các thuộc tính tĩnh sẽ được thêm vào tất cả các bản ghi log, ví dụ như tên ứng dụng.

---

## 👍 Best Practices

### 1. Sử dụng Log Level hợp lý
-   **`Verbose`/`Debug`**: Chỉ dùng cho môi trường Development để gỡ lỗi chi tiết.
-   **`Information`**: Ghi lại các sự kiện quan trọng trong luồng hoạt động bình thường của ứng dụng (e.g., "User X created product Y", "Processing order Z").
-   **`Warning`**: Ghi lại các tình huống bất thường nhưng không gây lỗi cho hệ thống (e.g., "API response took longer than expected", "Failed login attempt").
-   **`Error`**: Ghi lại các lỗi đã được xử lý (try-catch) nhưng ảnh hưởng đến một phần của request (e.g., "Failed to send email notification"). Cần chứa thông tin chi tiết về lỗi (`Exception`).
-   **`Fatal`**: Ghi lại các lỗi nghiêm trọng khiến toàn bộ ứng dụng phải dừng lại.

### 2. Logging có cấu trúc (Structured Logging)
Luôn ưu tiên ghi log có cấu trúc để tận dụng sức mạnh của Elasticsearch.

**❌ KHÔNG NÊN:**
```csharp
_logger.LogInformation("Processing order for user " + userId + " with total amount " + amount);
```

**✅ NÊN LÀM:**
```csharp
_logger.LogInformation("Processing order for user {UserId} with total amount {Amount}", userId, amount);
```
Bằng cách này, `UserId` và `Amount` sẽ trở thành các trường riêng biệt trong Elasticsearch, giúp bạn có thể truy vấn, ví dụ: "tìm tất cả order có `Amount` > 1000000".

Để log một đối tượng phức tạp, hãy thêm ký tự `@` trước tên thuộc tính:
```csharp
_logger.LogInformation("Order received: {@OrderDetails}", orderObject);
```

### 3. KHÔNG log dữ liệu nhạy cảm
Tuyệt đối **KHÔNG** ghi log các thông tin như:
-   Mật khẩu
-   Số thẻ tín dụng
-   Access token, refresh token
-   Thông tin cá nhân nhạy cảm (PII)

Hãy kiểm tra và lọc các thông tin này trước khi ghi log.

### 4. Truy vấn trong Kibana với Correlation ID
Trong Kibana, để xem tất cả log của một request, bạn chỉ cần thực hiện truy vấn đơn giản trong thanh tìm kiếm:

```kql
CorrelationId: "your-correlation-id-value"
```

Bạn cũng có thể tạo các dashboard để theo dõi các lỗi theo `Application`, hoặc theo dõi thời gian xử lý request.

---

## 🔍 Troubleshooting

### Log không xuất hiện trong Kibana?
1.  **Kiểm tra kết nối**: Đảm bảo service của bạn có thể kết nối đến địa chỉ Elasticsearch đã cấu hình trong `appsettings.json`.
2.  **Kiểm tra Index Pattern**: Trong Kibana, vào `Stack Management > Index Patterns` và đảm bảo bạn đã tạo một pattern khớp với tên index của bạn (e.g., `your-app-logs-*`).
3.  **Kiểm tra Log Level**: Đảm bảo `MinimumLevel` trong cấu hình Serilog không cao hơn cấp độ của log bạn đang ghi. Ví dụ, nếu `MinimumLevel` là `Warning`, các log `Information` sẽ không được ghi.
4.  **Kiểm tra output của Serilog**: Serilog có `SelfLog` để chẩn đoán các vấn đề của chính nó. Bạn có thể bật nó lên khi khởi động ứng dụng để xem có lỗi gì khi gửi log đến Elasticsearch không.
    ```csharp
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    ```
