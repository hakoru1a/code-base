# Hướng dẫn Kiểm tra Log Serilog trong Elasticsearch

Hướng dẫn chi tiết cách xem và tìm kiếm logs từ Serilog trong Elasticsearch/Kibana.

## 🚀 Cách 1: Sử dụng Kibana (Khuyến nghị)

### Bước 1: Truy cập Kibana

1. Mở trình duyệt và truy cập: **http://localhost:5601**
2. Đăng nhập với:
   - **Username**: `elastic`
   - **Password**: `elastic123`

### Bước 2: Tạo Index Pattern

Index pattern giúp Kibana biết cách đọc dữ liệu từ Elasticsearch.

1. Vào **Management** → **Stack Management** → **Index Patterns**
2. Click **Create index pattern**
3. Nhập pattern: `ch-logs-*` (khớp với format: `ch-logs-{applicationName}-{environmentName}-{yyyy-MM}`)
4. Click **Next step**
5. Chọn time field: `@timestamp` hoặc `timestamp`
6. Click **Create index pattern**

### Bước 3: Xem Logs trong Discover

1. Vào **Analytics** → **Discover**
2. Chọn index pattern vừa tạo từ dropdown ở góc trên bên trái
3. Bạn sẽ thấy tất cả logs từ các services

### Bước 4: Tìm kiếm và Lọc Logs

#### Tìm theo Correlation ID
```
CorrelationId: "abc-123-xyz"
```

#### Tìm theo Application/Service
```
Application: "api-gateway"
```
hoặc
```
Application: "generate-api"
```

#### Tìm theo Log Level
```
Level: "Error"
```
hoặc
```
Level: "Warning"
```

#### Tìm theo Message
```
Message: "user login"
```

#### Kết hợp nhiều điều kiện
```
Level: "Error" AND Application: "api-gateway"
```

#### Tìm theo thời gian
- Sử dụng time picker ở góc trên bên phải để chọn khoảng thời gian
- Có thể chọn: Last 15 minutes, Last 1 hour, Last 24 hours, hoặc Custom range

### Bước 5: Xem chi tiết một Log Entry

Click vào một log entry trong danh sách để xem đầy đủ thông tin:
- Message
- Level
- Timestamp
- Application
- MachineName
- CorrelationId
- Exception (nếu có)
- Các thuộc tính khác

---

## 🔍 Cách 2: Sử dụng Elasticsearch API (Command Line)

### Kiểm tra Elasticsearch đang chạy

```bash
curl -u elastic:elastic123 http://localhost:9200
```

### Xem danh sách các Index

```bash
curl -u elastic:elastic123 http://localhost:9200/_cat/indices?v
```

Kết quả sẽ hiển thị các index như:
- `ch-logs-api-gateway-development-2024-12`
- `ch-logs-generate-api-development-2024-12`
- `ch-logs-auth-api-development-2024-12`

### Xem số lượng documents trong một index

```bash
curl -u elastic:elastic123 http://localhost:9200/_cat/indices/ch-logs-*?v
```

### Tìm kiếm logs trong Elasticsearch

#### Tìm tất cả logs
```bash
curl -u elastic:elastic123 "http://localhost:9200/ch-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "match_all": {}
  },
  "size": 10
}
'
```

#### Tìm logs theo Correlation ID
```bash
curl -u elastic:elastic123 "http://localhost:9200/ch-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "match": {
      "CorrelationId": "abc-123-xyz"
    }
  }
}
'
```

#### Tìm logs theo Application
```bash
curl -u elastic:elastic123 "http://localhost:9200/ch-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "match": {
      "Application": "api-gateway"
    }
  }
}
'
```

#### Tìm logs Error
```bash
curl -u elastic:elastic123 "http://localhost:9200/ch-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "match": {
      "Level": "Error"
    }
  },
  "sort": [
    {
      "@timestamp": {
        "order": "desc"
      }
    }
  ]
}
'
```

#### Tìm logs trong khoảng thời gian
```bash
curl -u elastic:elastic123 "http://localhost:9200/ch-logs-*/_search?pretty" -H 'Content-Type: application/json' -d'
{
  "query": {
    "range": {
      "@timestamp": {
        "gte": "2024-12-01T00:00:00",
        "lte": "2024-12-31T23:59:59"
      }
    }
  }
}
'
```

---

## 📊 Cách 3: Tạo Dashboard trong Kibana

### Tạo Dashboard để theo dõi logs

1. Vào **Analytics** → **Dashboard**
2. Click **Create dashboard**
3. Click **Create visualization** để thêm các biểu đồ:
   - **Logs theo Level**: Pie chart với field `Level`
   - **Logs theo Application**: Bar chart với field `Application`
   - **Logs theo thời gian**: Line chart với time field
   - **Top Errors**: Data table với filter `Level: Error`

### Lưu Dashboard

Sau khi tạo xong, click **Save** để lưu dashboard và có thể truy cập lại sau.

---

## 🛠️ Troubleshooting

### Logs không xuất hiện trong Kibana?

1. **Kiểm tra Elasticsearch có nhận được logs:**
   ```bash
   curl -u elastic:elastic123 http://localhost:9200/_cat/indices?v
   ```
   Nếu không thấy index `ch-logs-*`, có thể:
   - Application chưa được cấu hình đúng
   - Elasticsearch URI không đúng trong `appsettings.json`
   - Application chưa ghi log nào

2. **Kiểm tra cấu hình trong `appsettings.json`:**
   ```json
   {
     "ElasticConfiguration": {
       "Uri": "http://localhost:9200",
       "Username": "elastic",
       "Password": "elastic123"
     }
   }
   ```

3. **Kiểm tra logs của application:**
   - Xem console output khi chạy application
   - Kiểm tra file log trong thư mục `logs/` (nếu có)
   - Tìm các lỗi kết nối Elasticsearch

4. **Kiểm tra Index Pattern trong Kibana:**
   - Vào **Management** → **Stack Management** → **Index Patterns**
   - Đảm bảo pattern `ch-logs-*` đã được tạo
   - Kiểm tra time field đã được chọn đúng

5. **Kiểm tra Elasticsearch và Kibana đang chạy:**
   ```bash
   docker ps | grep elasticsearch
   docker ps | grep kibana
   ```

### Không thể kết nối đến Elasticsearch?

1. Kiểm tra Elasticsearch có đang chạy:
   ```bash
   curl http://localhost:9200
   ```

2. Kiểm tra firewall/network settings

3. Kiểm tra credentials (username/password)

---

## 📝 Format của Index Name

Dựa vào code trong `SeriLogger.cs`, format index name là:
```
ch-logs-{applicationName}-{environmentName}-{yyyy-MM}
```

Ví dụ:
- `ch-logs-api-gateway-development-2024-12`
- `ch-logs-generate-api-production-2024-12`
- `ch-logs-auth-api-development-2024-12`

---

## 💡 Tips

1. **Sử dụng KQL (Kibana Query Language)** trong Kibana để tìm kiếm mạnh mẽ hơn:
   ```
   Level: "Error" AND Application: "api-gateway" AND @timestamp >= now()-1h
   ```

2. **Lưu các search queries** để sử dụng lại sau:
   - Sau khi tìm kiếm, click **Save** để lưu query

3. **Export logs** nếu cần:
   - Trong Discover, click **Share** → **CSV Reports** hoặc **JSON**

4. **Tạo Alerts** để nhận thông báo khi có lỗi:
   - Vào **Management** → **Stack Management** → **Rules and Connectors**

---

## 🔗 Tài liệu tham khảo

- [Kibana Query Language (KQL)](https://www.elastic.co/guide/en/kibana/current/kuery-query.html)
- [Elasticsearch Query DSL](https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html)
- [Serilog Elasticsearch Sink](https://github.com/serilog/serilog-sinks-elasticsearch)

