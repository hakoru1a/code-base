import { defineConfig, loadEnv, type UserConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

export default ({ mode }: UserConfig) => {
  process.env = { ...process.env, ...loadEnv(mode || '', process.cwd()) };

  return defineConfig({
    plugins: [react(), tsconfigPaths()],
    server: {
      host: process.env.VITE_HOST,
      port: parseInt(process.env.VITE_PORT || '3000')
    }
  });
};

