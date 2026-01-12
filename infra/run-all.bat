@echo off
rem =================================================================
rem CODEBASE INFRASTRUCTURE STARTUP SCRIPT
rem =================================================================

echo Starting CodeBase Infrastructure Services...
echo.

rem Change to infra directory
cd /d "%~dp0"

rem Load environment variables
echo Loading environment configuration...
if exist .env (
    echo Environment file found!
) else (
    echo Error: .env file not found!
    pause
    exit /b 1
)

# Create network if not exists
echo Creating Docker network...
docker network create codebase_network 2>nul || echo Network already exists

rem Create volumes directories if not exist
echo Creating volumes directories...
if not exist "volumes" mkdir volumes
if not exist "volumes\mysql" mkdir volumes\mysql
if not exist "volumes\redis" mkdir volumes\redis
if not exist "volumes\keycloak" mkdir volumes\keycloak

echo.
echo =================================================================
echo STARTING SERVICES
echo =================================================================

rem Start MySQL
echo [1/3] Starting MySQL Database...
call run-mysql.bat
if %errorlevel% neq 0 (
    echo Error starting MySQL!
    pause
    exit /b 1
)

rem Start Redis
echo [2/3] Starting Redis Cache...
call run-redis.bat
if %errorlevel% neq 0 (
    echo Error starting Redis!
    pause
    exit /b 1
)

rem Start Keycloak
echo [3/3] Starting Keycloak Auth Server...
call run-keycloak.bat
if %errorlevel% neq 0 (
    echo Error starting Keycloak!
    pause
    exit /b 1
)

echo.
echo =================================================================
echo ALL SERVICES STARTED SUCCESSFULLY!
echo =================================================================
echo.
echo Access URLs:
echo - Keycloak Admin Console: http://localhost:8080/admin
echo   Username: admin
echo   Password: admin123
echo.
echo - MySQL Database: localhost:4306
echo   Username: root
echo   Password: 123@56789
echo   Database: generate
echo.
echo - Redis Cache: localhost:6379
echo   Password: redis123
echo.
echo =================================================================

echo.
echo Press any key to exit...
pause >nul