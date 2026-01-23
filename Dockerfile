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
    pnpm install --frozen-lockfile

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
RUN apk add --no-cache curl

# Xóa config mặc định, dùng config custom
RUN rm -rf /usr/share/nginx/html/* /etc/nginx/conf.d/default.conf

COPY --from=builder /app/dist /usr/share/nginx/html

# SPA fallback: mọi route -> index.html
RUN echo 'server { \
    listen 80; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
    location /health { return 200; add_header Content-Type text/plain; } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
