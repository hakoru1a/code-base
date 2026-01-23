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
# Stage 4: Production – serve with nginx
# ---
FROM nginx:alpine AS runner
RUN apk add --no-cache curl gettext

# Remove default nginx config
RUN rm -rf /usr/share/nginx/html/* /etc/nginx/conf.d/default.conf

# Copy built static files
COPY --from=builder /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
