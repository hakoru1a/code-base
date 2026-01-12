@echo off
rem =================================================================
rem CLEAN ALL VOLUMES DATA - CẢNH BÁO: XÓA TẤT CẢ DỮ LIỆU!
rem =================================================================

echo =================================================================
echo CẢNH BÁO: Script này sẽ XÓA TẤT CẢ dữ liệu trong volumes!
echo =================================================================
echo.
echo Điều này bao gồm:
echo - Tất cả database MySQL
echo - Tất cả cache Redis  
echo - Tất cả cấu hình Keycloak và users
echo.
echo Bạn có chắc chắn muốn tiếp tục? (y/N)
set /p confirm=

if /i "%confirm%" neq "y" (
    echo Hủy bỏ thao tác.
    pause
    exit /b 0
)

echo.
echo Stopping all services first...
call stop-all.bat

echo.
echo Cleaning volumes...

rem Remove all volume data
if exist "volumes\mysql" (
    echo Cleaning MySQL data...
    rmdir /s /q "volumes\mysql" 2>nul
    mkdir "volumes\mysql"
)

if exist "volumes\redis" (
    echo Cleaning Redis data...
    rmdir /s /q "volumes\redis" 2>nul
    mkdir "volumes\redis"
)

if exist "volumes\keycloak" (
    echo Cleaning Keycloak data...
    rmdir /s /q "volumes\keycloak" 2>nul
    mkdir "volumes\keycloak"
)

echo.
echo =================================================================
echo TẤT CẢ DỮ LIỆU VOLUMES ĐÃ ĐƯỢC XÓA!
echo =================================================================
echo.
echo Lần khởi động tiếp theo sẽ như lần đầu cài đặt.
echo Chạy run-all.bat để khởi động lại các services.
echo.

pause