#!/bin/bash
# =================================================================
# CLEAN ALL VOLUMES DATA - CẢNH BÁO: XÓA TẤT CẢ DỮ LIỆU!
# =================================================================

set -e

echo "================================================================="
echo "CẢNH BÁO: Script này sẽ XÓA TẤT CẢ dữ liệu trong volumes!"
echo "================================================================="
echo ""
echo "Điều này bao gồm:"
echo "- Tất cả database MySQL"
echo "- Tất cả cache Redis"
echo "- Tất cả cấu hình Keycloak và users"
echo ""
read -p "Bạn có chắc chắn muốn tiếp tục? (y/N): " confirm

if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
    echo "Hủy bỏ thao tác."
    exit 0
fi

# Change to script directory
cd "$(dirname "$0")"

echo ""
echo "Stopping all services first..."

# Stop all services
if [ -f .env ]; then
    docker-compose --env-file .env -f database/mysql.yml down 2>/dev/null || true
    docker-compose --env-file .env -f cache/redis.yml down 2>/dev/null || true
    docker-compose --env-file .env -f auth/keycloak.yml down 2>/dev/null || true
fi

echo ""
echo "Cleaning volumes..."

# Remove all volume data
if [ -d "volumes/mysql" ]; then
    echo "Cleaning MySQL data..."
    rm -rf "volumes/mysql"
    mkdir -p "volumes/mysql"
fi

if [ -d "volumes/redis" ]; then
    echo "Cleaning Redis data..."
    rm -rf "volumes/redis"
    mkdir -p "volumes/redis"
fi

if [ -d "volumes/keycloak" ]; then
    echo "Cleaning Keycloak data..."
    rm -rf "volumes/keycloak"
    mkdir -p "volumes/keycloak"
fi

echo ""
echo "================================================================="
echo "TẤT CẢ DỮ LIỆU VOLUMES ĐÃ ĐƯỢC XÓA!"
echo "================================================================="
echo ""
echo "Lần khởi động tiếp theo sẽ như lần đầu cài đặt."
echo "Chạy run-all.sh để khởi động lại các services."
echo ""
echo "Press Enter to exit..."
read
