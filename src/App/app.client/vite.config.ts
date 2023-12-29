import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue({
    template: {
      compilerOptions: {
        whitespace: 'preserve'
      }
    }
  })],
  server: {
    port: 8080,
    proxy: {
        '^/api': {
            target: 'http://localhost:5000/',
            secure: false
        },
        '^/jobRunConsoleHub': {
            target: 'http://localhost:5000/',
            secure: false,
            ws: true
        }
    },
  }
})
