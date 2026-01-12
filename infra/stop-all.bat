@echo off
rem =================================================================
rem STOP ALL CODEBASE INFRASTRUCTURE SERVICES
rem =================================================================

echo Stopping CodeBase Infrastructure Services...
echo.

rem Change to infra directory
cd /d "%~dp0"

echo =================================================================
echo STOPPING SERVICES
echo =================================================================

rem Stop Keycloak
echo [1/3] Stopping Keycloak Auth Server...
cd auth
docker-compose --env-file ../.env -f keycloak.yml down
cd ..

rem Stop Redis
echo [2/3] Stopping Redis Cache...
cd cache
docker-compose --env-file ../.env -f redis.yml down
cd ..

rem Stop MySQL
echo [3/3] Stopping MySQL Database...
cd database
docker-compose --env-file ../.env -f mysql.yml down
cd ..

echo.
echo =================================================================
echo ALL SERVICES STOPPED SUCCESSFULLY!
echo =================================================================

echo.
echo Press any key to exit...
pause >nul