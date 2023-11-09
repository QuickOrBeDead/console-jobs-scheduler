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
    port: 8080
  }
})
