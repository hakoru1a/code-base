@echo off
rem =================================================================
rem CHECK STATUS OF ALL CODEBASE INFRASTRUCTURE SERVICES
rem =================================================================

echo Checking CodeBase Infrastructure Services Status...
echo.

rem Change to infra directory
cd /d "%~dp0"

echo =================================================================
echo SERVICES STATUS
echo =================================================================

echo.
echo Docker Network:
docker network ls | findstr codebase_network

echo.
echo Running Containers:
echo ------------------
docker ps --filter "name=codebase_" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo Available Services:
echo ------------------
echo - Keycloak: http://localhost:8080/admin (admin/admin123)
echo - MySQL: localhost:4306 (root/123@56789)
echo - Redis: localhost:6379 (password: redis123)

echo.
echo =================================================================

echo.
echo Press any key to exit...
pause >nul