import { defineConfig, loadEnv, type UserConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

// Plugin để redirect root path đến Keycloak
const keycloakRedirectPlugin = () => ({
  name: 'keycloak-redirect',
  configureServer(server: any) {
    server.middlewares.use('/', (req: any, res: any, next: any) => {
      // Chỉ redirect root path và không phải callback/assets
      if (req.url === '/' && req.method === 'GET') {
        const apiUrl = process.env.VITE_APP_API_URL || 'http://20.195.15.250:5238';
        const frontendUrl = process.env.VITE_APP_FRONTEND_URL || `http://${req.headers.host}`;
        const returnUrl = `${frontendUrl}/auth/callback`;
        const redirectUrl = `${apiUrl}/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;

        res.writeHead(302, { Location: redirectUrl });
        res.end();
        return;
      }
      next();
    });
  }
});

export default ({ mode }: UserConfig) => {
  process.env = { ...process.env, ...loadEnv(mode || '', process.cwd()) };

  return defineConfig({
    plugins: [
      react(),
      tsconfigPaths(),
      keycloakRedirectPlugin()
    ],
    server: {
      host: process.env.VITE_HOST,
      port: parseInt(process.env.VITE_PORT || '3000')
    }
  });
};

