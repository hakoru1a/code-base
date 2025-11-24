# Docker Infrastructure Setup

Thư mục này chứa các file cấu hình Docker Compose để chạy toàn bộ stack của ứng dụng.

## Cấu trúc thư mục

```
infra/
├── docker-compose.yml          # Entry point - tổng hợp tất cả services
├── .env                        # File biến môi trường (cần tạo)
├── database/
│   ├── mysql.yml               # MySQL service definition (reference)
│   └── postgresql.yml          # PostgreSQL service definition (reference)
├── cache/
│   └── redis.yml               # Redis cache service definition (reference)
├── message-bus/
│   └── rabbitmq.yml            # RabbitMQ message bus definition (reference)
├── auth/
│   └── keycloak.yml            # Keycloak authentication definition (reference)
├── monitoring/
│   └── elastic-search.yml      # Elasticsearch + Kibana definition (reference)
└── services/
    ├── api-gateway.yml         # API Gateway service definition (reference)
    ├── base-api.yml            # Base API service definition (reference)
    └── generate-api.yml        # Generate API service definition (reference)
```

**Lưu ý**: Các file YML trong các thư mục con là định nghĩa tham khảo. Tất cả services đã được tổng hợp trong `docker-compose.yml`.

## Cách sử dụng

### 1. Tạo file .env

Tạo file `.env` trong thư mục `infra/` với nội dung như sau (xem mục Environment Variables bên dưới):

```bash
# Tạo file .env và điền các giá trị
nano .env
```

Hoặc copy từ file mẫu (nếu có).

### 2. Chạy toàn bộ stack

```bash
docker-compose --env-file .env up -d
```

### 3. Các lệnh thường dùng

```bash
# Xem logs
docker-compose --env-file .env logs -f [service-name]

# Xem logs tất cả services
docker-compose --env-file .env logs -f

# Dừng tất cả services
docker-compose --env-file .env down

# Dừng và xóa volumes
docker-compose --env-file .env down -v

# Khởi động lại service
docker-compose --env-file .env restart [service-name]

# Xem trạng thái services
docker-compose --env-file .env ps

# Rebuild và khởi động lại
docker-compose --env-file .env up -d --build

# Chỉ khởi động infrastructure services (không có apps)
docker-compose --env-file .env up -d mysql redis rabbitmq keycloak elasticsearch kibana
```

## Environment Variables (.env)

Tạo file `.env` với các biến sau:

```bash
# Application Environment
ASPNETCORE_ENVIRONMENT=Development

# MySQL Configuration
MYSQL_ROOT_PASSWORD=root_password_123
MYSQL_DATABASE=CodeBase
MYSQL_USER=codebase_user
MYSQL_PASSWORD=codebase_password_123
MYSQL_PORT=3306

# PostgreSQL Configuration
POSTGRES_DB=CodeBase
POSTGRES_USER=codebase_user
POSTGRES_PASSWORD=codebase_password_123
POSTGRES_PORT=5432

# Database Provider (MySQL or PostgreSQL)
DATABASE_PROVIDER=MySQL
DATABASE_CONNECTION_STRING=Server=mysql;Database=CodeBase;Uid=codebase_user;Pwd=codebase_password_123;

# Redis Configuration
REDIS_PASSWORD=redis_password_123
REDIS_PORT=6379
REDIS_CONNECTION_STRING=redis:6379,password=redis_password_123

# RabbitMQ Configuration
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=admin_password_123
RABBITMQ_VHOST=/
RABBITMQ_PORT=5672
RABBITMQ_MANAGEMENT_PORT=15672
RABBITMQ_HOST=rabbitmq

# Keycloak Configuration
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin_password_123
KEYCLOAK_DB=keycloak
KEYCLOAK_DB_USER=keycloak_user
KEYCLOAK_DB_PASSWORD=keycloak_password_123
KEYCLOAK_PORT=8080
KEYCLOAK_LOG_LEVEL=INFO
KEYCLOAK_AUTHORITY=http://localhost:8080/realms/master
KEYCLOAK_CLIENT_ID=webapp
KEYCLOAK_CLIENT_SECRET=your_client_secret_here
KEYCLOAK_METADATA_ADDRESS=http://localhost:8080/realms/master/.well-known/openid-configuration

# Elasticsearch Configuration
ELASTIC_PASSWORD=elastic123
ELASTICSEARCH_PORT=9200
ELASTICSEARCH_TRANSPORT_PORT=9300
ELASTICSEARCH_URI=http://elasticsearch:9200
KIBANA_PORT=5601

# Application Services Ports
API_GATEWAY_PORT=5000
API_GATEWAY_HTTPS_PORT=5001
BASE_API_PORT=5002
BASE_API_HTTPS_PORT=5003
GENERATE_API_PORT=5004
GENERATE_API_HTTPS_PORT=5005
```

## Services và Ports

| Service | Port | Description |
|---------|------|-------------|
| MySQL | 3306 | Database |
| PostgreSQL | 5432 | Database |
| Redis | 6379 | Cache |
| RabbitMQ | 5672 | Message Queue |
| RabbitMQ Management | 15672 | Web UI |
| Keycloak | 8080 | Authentication |
| Elasticsearch | 9200 | Search Engine |
| Kibana | 5601 | Visualization |
| API Gateway | 5000, 5001 | Gateway HTTP/HTTPS |
| Base API | 5002, 5003 | Base Service HTTP/HTTPS |
| Generate API | 5004, 5005 | Generate Service HTTP/HTTPS |

## Truy cập các services

- **RabbitMQ Management**: http://localhost:15672 (user/password từ .env)
- **Keycloak**: http://localhost:8080 (admin/admin_password từ .env)
- **Kibana**: http://localhost:5601 (elastic/elastic123)
- **Elasticsearch**: http://localhost:9200 (elastic/elastic123)
- **API Gateway**: http://localhost:5000

## Yêu cầu

- Docker Desktop hoặc Docker Engine
- Docker Compose v2.0+
- Đủ RAM (khuyến nghị tối thiểu 8GB)
- Đủ disk space cho volumes

## Lưu ý

1. **Đổi tất cả các mật khẩu mặc định** trong file `.env` trước khi deploy production
2. Volumes sẽ được tạo tự động và lưu trữ dữ liệu persistent
3. Health checks được cấu hình cho tất cả services để đảm bảo dependencies khởi động đúng thứ tự
4. Network `codebase_network` được tạo tự động để các services có thể giao tiếp với nhau
5. File `.env` chứa các mật khẩu nhạy cảm - **KHÔNG** commit vào git repository
