@echo off
rem =================================================================
rem START REDIS CACHE
rem =================================================================

echo Starting Redis Cache...

rem Change to cache directory
cd /d "%~dp0cache"

rem Start Redis with environment file
docker-compose --env-file ../.env -f redis.yml up -d

if %errorlevel% equ 0 (
    echo Redis started successfully!
    echo Waiting for Redis to be ready...
    timeout /t 5 >nul
    echo Redis Cache is running on port 6379
) else (
    echo Failed to start Redis!
    exit /b 1
)

cd /d "%~dp0"