@echo off
rem =================================================================
rem START KEYCLOAK AUTH SERVER
rem =================================================================

echo Starting Keycloak Auth Server...

rem Change to auth directory
cd /d "%~dp0auth"

rem Start Keycloak with environment file
docker-compose --env-file ../.env -f keycloak.yml up -d

if %errorlevel% equ 0 (
    echo Keycloak started successfully!
    echo Waiting for Keycloak to be ready...
    echo This may take a few minutes for first startup...
    timeout /t 30 >nul
    echo Keycloak Auth Server is running on port 8080
    echo Admin Console: http://localhost:8080/admin
) else (
    echo Failed to start Keycloak!
    exit /b 1
)

cd /d "%~dp0"