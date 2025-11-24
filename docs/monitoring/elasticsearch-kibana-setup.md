# Elasticsearch & Kibana Setup Guide

Hướng dẫn cấu hình và khởi động Elasticsearch và Kibana cho hệ thống logging.

## 📋 Thông tin cấu hình

- **Elasticsearch**: http://localhost:9200
  - **Username**: `elastic`
  - **Password**: `elastic123`
- **Kibana**: http://localhost:5601
  - **Username**: `elastic`
  - **Password**: `elastic123`

## 🚀 Khởi động với Docker Compose

### 1. Tạo network (nếu chưa có)

```bash
docker network create codebase_network
```

### 2. Tạo file `.env` (nếu chưa có)

Tạo file `.env` trong thư mục `infra/` với nội dung:

```env
ELASTIC_PASSWORD=elastic123
ELASTICSEARCH_TRANSPORT_PORT=9300
```

### 3. Khởi động Elasticsearch và Kibana

```bash
cd infra/monitoring
docker-compose -f elastic-search.yml up -d
```

### 4. Kiểm tra trạng thái

**Elasticsearch:**
```bash
curl http://localhost:9200
```

Hoặc với authentication:
```bash
curl -u elastic:elastic123 http://localhost:9200
```

**Kibana:**
Mở trình duyệt và truy cập: http://localhost:5601

## ⚙️ Cấu hình trong ứng dụng

### Cấu hình trong `appsettings.json`

Tất cả các API services cần có cấu hình sau:

```json
{
  "ElasticConfiguration": {
    "Uri": "http://localhost:9200",
    "Username": "elastic",
    "Password": "elastic123"
  }
}
```

### Các services đã được cấu hình

- ✅ `ApiGateway/appsettings.json`
- ✅ `Auth.API/appsettings.json`
- ✅ `Generate.API/appsettings.json`

## 📊 Sử dụng Kibana

### 1. Truy cập Kibana

Mở trình duyệt: http://localhost:5601

Đăng nhập với:
- **Username**: `elastic`
- **Password**: `elastic123`

### 2. Tạo Index Pattern

1. Vào **Management** → **Stack Management** → **Index Patterns**
2. Click **Create index pattern**
3. Nhập pattern: `ch-logs-*` (hoặc pattern tương ứng với index format của bạn)
4. Click **Next step**
5. Chọn time field: `@timestamp`
6. Click **Create index pattern**

### 3. Xem logs trong Discover

1. Vào **Analytics** → **Discover**
2. Chọn index pattern vừa tạo
3. Bạn sẽ thấy tất cả logs từ các services

### 4. Tạo Dashboard (tùy chọn)

1. Vào **Analytics** → **Dashboard**
2. Click **Create dashboard**
3. Thêm các visualizations để theo dõi:
   - Log levels (Error, Warning, Information)
   - Logs theo service
   - Logs theo thời gian
   - Correlation IDs

## 🔍 Tìm kiếm logs

### Tìm logs theo Correlation ID

```
correlationId: "abc-123-xyz"
```

### Tìm logs theo service

```
Application: "api-gateway"
```

### Tìm logs theo level

```
Level: "Error"
```

### Tìm logs theo thời gian

Sử dụng time picker ở góc trên bên phải để chọn khoảng thời gian.

## 🛠️ Troubleshooting

### Elasticsearch không khởi động

1. Kiểm tra logs:
```bash
docker logs codebase_elasticsearch
```

2. Kiểm tra memory:
```bash
# Elasticsearch cần ít nhất 512MB RAM
# Kiểm tra trong docker-compose.yml: ES_JAVA_OPTS=-Xms512m -Xmx512m
```

3. Kiểm tra ports:
```bash
# Đảm bảo ports 9200 và 5601 không bị chiếm
netstat -an | grep 9200
netstat -an | grep 5601
```

### Kibana không kết nối được Elasticsearch

1. Kiểm tra network:
```bash
docker network inspect codebase_network
```

2. Kiểm tra environment variables trong Kibana:
```bash
docker exec codebase_kibana env | grep ELASTICSEARCH
```

3. Kiểm tra logs:
```bash
docker logs codebase_kibana
```

### Logs không xuất hiện trong Kibana

1. Kiểm tra cấu hình ElasticConfiguration trong `appsettings.json`
2. Kiểm tra logs của application để xem có lỗi kết nối Elasticsearch không
3. Kiểm tra index pattern trong Kibana có đúng format không
4. Kiểm tra Elasticsearch có nhận được logs:
```bash
curl -u elastic:elastic123 http://localhost:9200/_cat/indices
```

## 📝 Lưu ý

1. **Security**: Đổi password mặc định `elastic123` trong production nếu cần
2. **Memory**: Elasticsearch cần ít nhất 512MB RAM, khuyến nghị 2GB+
3. **Storage**: Dữ liệu được lưu trong Docker volume `elasticsearch_data`
4. **Network**: Đảm bảo network `codebase_network` đã được tạo
5. **Version**: Elasticsearch và Kibana phải cùng version (hiện tại: 8.11.0)

## 🔗 Tài liệu tham khảo

- [Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Kibana Documentation](https://www.elastic.co/guide/en/kibana/current/index.html)
- [Serilog Elasticsearch Sink](https://github.com/serilog/serilog-sinks-elasticsearch)

