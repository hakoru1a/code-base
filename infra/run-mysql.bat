@echo off
rem =================================================================
rem START MYSQL DATABASE
rem =================================================================

echo Starting MySQL Database...

rem Change to database directory
cd /d "%~dp0database"

rem Start MySQL with environment file
docker-compose --env-file ../.env -f mysql.yml up -d

if %errorlevel% equ 0 (
    echo MySQL started successfully!
    echo Waiting for MySQL to be ready...
    timeout /t 10 >nul
    echo MySQL Database is running on port 4306
) else (
    echo Failed to start MySQL!
    exit /b 1
)

cd /d "%~dp0"