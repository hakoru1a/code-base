#!/bin/bash
# =================================================================
# CODEBASE INFRASTRUCTURE STARTUP SCRIPT
# =================================================================

set -e

echo "Starting CodeBase Infrastructure Services..."
echo ""

# Change to script directory
cd "$(dirname "$0")"

# Load environment variables
echo "Loading environment configuration..."
if [ -f .env ]; then
    echo "Environment file found!"
else
    echo "Error: .env file not found!"
    exit 1
fi

# Create network if not exists
echo "Creating Docker network..."
docker network create codebase_network 2>/dev/null || echo "Network already exists"

# Create volumes directories if not exist
echo "Creating volumes directories..."
mkdir -p volumes/mysql
mkdir -p volumes/redis
mkdir -p volumes/keycloak

echo ""
echo "================================================================="
echo "STARTING SERVICES"
echo "================================================================="

Start MySQL
echo "[1/3] Starting MySQL Database..."
docker-compose --env-file .env -f database/mysql.yml up -d
if [ $? -ne 0 ]; then
    echo "Error starting MySQL!"
    exit 1
fi

# Start Redis
echo "[2/3] Starting Redis Cache..."
docker-compose --env-file .env -f cache/redis.yml up -d
if [ $? -ne 0 ]; then
    echo "Error starting Redis!"
    exit 1
fi

# Start Keycloak
echo "[3/3] Starting Keycloak Auth Server..."
docker-compose --env-file .env -f auth/keycloak.yml up -d
if [ $? -ne 0 ]; then
    echo "Error starting Keycloak!"
    exit 1
fi

echo ""
echo "================================================================="
echo "ALL SERVICES STARTED SUCCESSFULLY!"
echo "================================================================="
echo ""
echo "Access URLs:"
echo "- Keycloak Admin Console: http://localhost:8080/admin"
echo "  Username: admin"
echo "  Password: admin123"
echo ""
echo "- MySQL Database: localhost:4306"
echo "  Username: root"
echo "  Password: 123@56789"
echo "  Database: generate"
echo ""
echo "- Redis Cache: localhost:6379"
echo "  Password: redis123"
echo ""
echo "================================================================="
echo ""
echo "Press Enter to exit..."
read
