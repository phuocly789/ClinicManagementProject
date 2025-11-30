import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'fs';

export default defineConfig({
    plugins: [react()],
    server: {
        port: 5173,
        host: '0.0.0.0',
        // https: {
        //     key: fs.readFileSync('./125.212.218.44-key.pem'),
        //     cert: fs.readFileSync('./125.212.218.44.pem')
        // },
        watch: {
            usePolling: true // Để hot reload trong Docker
        }
    }
});
