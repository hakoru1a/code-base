# Volumes Directory

Thư mục này chứa tất cả dữ liệu persistent của các services:

## Cấu trúc thư mục

```
volumes/
├── mysql/          # MySQL database data
├── redis/          # Redis cache data  
├── keycloak/       # Keycloak PostgreSQL database data
└── README.md       # File này
```

## Lưu ý quan trọng

⚠️ **KHÔNG XÓA** các thư mục này khi containers đang chạy!

- Dữ liệu trong các thư mục này sẽ được giữ lại ngay cả khi containers bị xóa
- Để reset hoàn toàn dữ liệu, tắt tất cả containers trước khi xóa nội dung các thư mục
- Backup dữ liệu quan trọng trước khi thực hiện các thao tác maintenance

## Backup & Restore

### Backup
```bash
# Tạo backup của toàn bộ volumes
xcopy volumes volumes_backup /E /I

# Hoặc backup từng service
xcopy volumes\mysql mysql_backup /E /I
```

### Restore  
```bash
# Stop all services trước
cd infra
stop-all.bat

# Restore data
xcopy mysql_backup volumes\mysql /E /I

# Start services lại
run-all.bat
```