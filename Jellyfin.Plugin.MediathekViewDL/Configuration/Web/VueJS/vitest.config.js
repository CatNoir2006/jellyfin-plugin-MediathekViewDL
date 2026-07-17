import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [vue()],
    test: {
        environment: 'happy-dom',
        globals: true,
        include: ['src/**/*.{test,spec}.{js,mjs}'],
        exclude: ['node_modules', 'dist'],
        coverage: {
            provider: 'v8',
            reporter: ['text', 'lcov', 'cobertura', 'json', 'json-summary'],
            reportsDirectory: './coverage',
            include: ['src/**/*.{js,vue}'],
            exclude: [
                'node_modules',
                'dist',
                '**/*.test.{js,mjs}',
                '**/*.spec.{js,mjs}',
                'src/main.js'
            ]
        }
    }
})
