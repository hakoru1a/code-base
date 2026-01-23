#!/bin/sh
set -e

# Debug: Check if script is running
echo "Entrypoint script started at $(date)"

# Default values
API_URL="${VITE_APP_API_URL:-http://20.195.15.250:5238}"
FRONTEND_URL="${VITE_APP_FRONTEND_URL:-http://localhost:3000}"

# Build returnUrl
RETURN_URL="${FRONTEND_URL}/auth/callback"

# URL encode using sed (simple approach for common characters)
# This handles most common cases: : / ? # [ ] @ ! $ & ' ( ) * + , ; =
ENCODED_RETURN_URL=$(echo "$RETURN_URL" | sed \
    -e 's/:/%3A/g' \
    -e 's|/|%2F|g' \
    -e 's/?/%3F/g' \
    -e 's/#/%23/g' \
    -e 's/\[/%5B/g' \
    -e 's/\]/%5D/g' \
    -e 's/@/%40/g' \
    -e 's/!/%21/g' \
    -e 's/\$/%24/g' \
    -e 's/&/%26/g' \
    -e "s/'/%27/g" \
    -e 's/(/%28/g' \
    -e 's/)/%29/g' \
    -e 's/\*/%2A/g' \
    -e 's/+/%2B/g' \
    -e 's/,/%2C/g' \
    -e 's/;/%3B/g' \
    -e 's/=/%3D/g')

# Build redirect URL
REDIRECT_URL="${API_URL}/auth/login?returnUrl=${ENCODED_RETURN_URL}"

# Export for envsubst
export REDIRECT_URL

# Generate nginx config from template
envsubst '${REDIRECT_URL}' < /etc/nginx/templates/nginx.conf.template > /etc/nginx/conf.d/default.conf

# Test nginx configuration
nginx -t

# Start nginx
exec nginx -g "daemon off;"
