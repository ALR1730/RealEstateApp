import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5196',
        changeOrigin: true,
        secure: false,
      },
      '/hubs': {
        target: 'http://localhost:5196',
        ws: true,
        changeOrigin: true,
      },
      '/images': {
        target: 'http://localhost:5196',
        changeOrigin: true,
      }
    }
  },
  // @ts-ignore
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/tests/setup.ts',
  }
});
