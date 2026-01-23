import { defineConfig, loadEnv, type UserConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

export default ({ mode }: UserConfig) => {
  const env = loadEnv(mode || '', process.cwd());
  process.env = { ...process.env, ...env };

  const apiUrl = env.VITE_APP_API_URL || 'http://20.195.15.250:5238';
  const frontendUrl = env.VITE_APP_FRONTEND_URL || 'http://localhost:3000';

  return defineConfig({
    plugins: [
      react(),
      tsconfigPaths(),
      // Custom plugin to handle root redirect
      {
        name: 'root-redirect',
        configureServer(server) {
          server.middlewares.use('/', (req, res, next) => {
            // Only redirect exact root path requests
            if (req.url === '/' && req.method === 'GET') {
              const returnUrl = encodeURIComponent(`${frontendUrl}/auth/callback`);
              const redirectUrl = `${apiUrl}/auth/login?returnUrl=${returnUrl}`;
              
              res.writeHead(302, {
                'Location': redirectUrl,
                'Cache-Control': 'no-cache'
              });
              res.end();
              return;
            }
            next();
          });
        },
        configurePreviewServer(server) {
          server.middlewares.use('/', (req, res, next) => {
            // Only redirect exact root path requests
            if (req.url === '/' && req.method === 'GET') {
              const returnUrl = encodeURIComponent(`${frontendUrl}/auth/callback`);
              const redirectUrl = `${apiUrl}/auth/login?returnUrl=${returnUrl}`;
              
              res.writeHead(302, {
                'Location': redirectUrl,
                'Cache-Control': 'no-cache'
              });
              res.end();
              return;
            }
            next();
          });
        }
      }
    ],
    server: {
      host: process.env.VITE_HOST || '0.0.0.0',
      port: parseInt(process.env.VITE_PORT || '3000'),
      // Handle SPA routing - fallback to index.html for all routes
      historyApiFallback: true,
    },
    preview: {
      host: '0.0.0.0',
      port: parseInt(process.env.VITE_PORT || '3000'),
      // Handle SPA routing in preview mode as well
      historyApiFallback: true,
    },
    build: {
      // Optimize build for production
      rollupOptions: {
        output: {
          manualChunks: {
            vendor: ['react', 'react-dom'],
            router: ['react-router-dom'],
          },
        },
      },
    },
  });
};

