import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Frontend http://localhost:5173 adresinde çalışır.
// "/api" ile başlayan istekler backend'e (http://localhost:5168) yönlendirilir.
// Böylece tarayıcı açısından ikisi aynı adreste görünür ve CORS sorunu olmaz.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5168',
    },
  },
})
