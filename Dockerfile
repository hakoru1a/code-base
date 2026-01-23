# syntax=docker/dockerfile:1
# ---
# Stage 1: Base – Node + pnpm
# ---
FROM node:22-alpine AS base
RUN corepack enable && corepack prepare pnpm@9 --activate
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
WORKDIR /app

# ---
# Stage 2: Dependencies – chỉ cài deps, tận dụng cache layer
# ---
FROM base AS deps
COPY package.json pnpm-lock.yaml* ./
RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    if pnpm install --frozen-lockfile; then \
        echo "Frozen lockfile install successful"; \
    else \
        echo "Frozen lockfile failed, installing without frozen lockfile"; \
        pnpm install --no-frozen-lockfile; \
    fi

# ---
# Stage 3: Builder – build app
# ---
FROM base AS builder
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build-time env (override qua docker build --build-arg)
ARG VITE_APP_API_URL
ARG VITE_APP_FRONTEND_URL
ENV VITE_APP_API_URL=${VITE_APP_API_URL}
ENV VITE_APP_FRONTEND_URL=${VITE_APP_FRONTEND_URL}

RUN pnpm build

# ---
# Stage 4: Runner – serve static bằng nginx
# ---
FROM nginx:alpine AS runner
RUN apk add --no-cache curl gettext

# Xóa config mặc định
RUN rm -rf /usr/share/nginx/html/* /etc/nginx/conf.d/default.conf

# Copy static files
COPY --from=builder /app/dist /usr/share/nginx/html

# Copy nginx template và entrypoint script
COPY nginx/nginx.conf /etc/nginx/templates/nginx.conf.template
COPY nginx/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh && \
    # Fix line endings (CRLF -> LF) for entrypoint script
    sed -i 's/\r$//' /usr/local/bin/entrypoint.sh && \
    # Verify file exists and is executable
    ls -la /usr/local/bin/entrypoint.sh

# Runtime environment variables (sẽ được set từ docker-compose)
ENV VITE_APP_API_URL=""
ENV VITE_APP_FRONTEND_URL=""

EXPOSE 80

# Use sh to run the entrypoint script (more compatible)
CMD ["/bin/sh", "/usr/local/bin/entrypoint.sh"]
