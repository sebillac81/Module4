import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

// La API corre en el puerto fijo de src/RoastingManager.Api/Properties/launchSettings.json.
const apiUrl = process.env.API_URL ?? 'http://localhost:5080';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': apiUrl,
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./tests/setup.ts'],
  },
});
