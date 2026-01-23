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
# Stage 3: Production – serve with Vite preview
# ---
FROM base AS runner
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build-time env (override qua docker build --build-arg)
ARG VITE_APP_API_URL
ARG VITE_APP_FRONTEND_URL
ENV VITE_APP_API_URL=${VITE_APP_API_URL}
ENV VITE_APP_FRONTEND_URL=${VITE_APP_FRONTEND_URL}

# Build the application
RUN pnpm build

# Install curl for health checks
RUN apk add --no-cache curl

# Runtime environment variables (sẽ được set từ docker-compose)
ENV VITE_HOST=0.0.0.0
ENV VITE_PORT=3000

EXPOSE 3000

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:3000/dashboard || exit 1

# Use Vite preview to serve the built application
CMD ["pnpm", "preview"]
