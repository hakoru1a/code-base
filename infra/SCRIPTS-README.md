# CodeBase Infrastructure Scripts

Các file batch để quản lý infrastructure services của CodeBase project.

## Yêu cầu

- Docker Desktop phải được cài đặt và chạy
- File `.env` phải tồn tại trong thư mục `infra/`

## Cách sử dụng

### Khởi động tất cả services
```bash
run-all.bat
```
Script này sẽ:
1. Tạo Docker network
2. Khởi động MySQL Database
3. Khởi động Redis Cache  
4. Khởi động Keycloak Auth Server

### Khởi động từng service riêng lẻ
```bash
run-mysql.bat    # Chỉ MySQL
run-redis.bat    # Chỉ Redis
run-keycloak.bat # Chỉ Keycloak
```

### Tắt tất cả services
```bash
stop-all.bat
```

### Dọn dẹp volumes (XÓA TẤT CẢ DỮ LIỆU)
```bash
clean-volumes.bat
```
⚠️ **CẢNH BÁO**: Script này sẽ xóa toàn bộ dữ liệu database, cache và cấu hình!

### Kiểm tra trạng thái
```bash
status.bat
```

## Thông tin truy cập

Sau khi chạy thành công, các service sẽ có thể truy cập qua:

### Keycloak Admin Console
- URL: http://localhost:8080/admin
- Username: `admin`
- Password: `admin123`

### MySQL Database
- Host: `localhost`
- Port: `4306`
- Username: `root`
- Password: `123@56789`
- Database: `generate`

### Redis Cache
- Host: `localhost`
- Port: `6379`
- Password: `redis123`

## Troubleshooting

1. **Lỗi port đã được sử dụng**: Kiểm tra xem có service nào đang chạy trên các port 8080, 4306, 6379
2. **Docker network error**: Chạy `docker network create codebase_network` thủ công
3. **Permission denied**: Chạy Command Prompt/PowerShell với quyền Administrator

## Lưu ý

- Lần đầu chạy Keycloak có thể mất 2-3 phút để khởi động hoàn toàn
- Tất cả dữ liệu được lưu trữ trong thư mục `volumes/` với bind mounts
- Để backup dữ liệu: copy toàn bộ thư mục `volumes/`
- Để reset data: chạy `clean-volumes.bat` hoặc xóa nội dung thư mục `volumes/`

## Cấu trúc Volumes

```
infra/
├── volumes/
│   ├── mysql/          # MySQL database data
│   ├── redis/          # Redis cache data
│   ├── keycloak/       # Keycloak PostgreSQL data
│   └── README.md       # Hướng dẫn volumes
└── ...
```