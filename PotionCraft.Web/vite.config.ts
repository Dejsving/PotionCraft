import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Прокси на HTTPS-бэкенд: cookie сессии остаётся same-origin с фронтендом.
      '/api': {
        target: 'https://localhost:7088',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
