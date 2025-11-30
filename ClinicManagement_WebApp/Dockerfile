# Stage 1: Build
FROM node:22-alpine as build 
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# Lưu ý: Vite cần biến môi trường lúc Build.
# Bạn có thể set cứng ở đây hoặc truyền qua docker build args.
# Để đơn giản, docker-compose sẽ map port 5066 ra máy host, nên localhost vẫn đúng với browser của user.
ENV VITE_BACKEND_URL=http://localhost:5066/api

RUN npm run build

# Stage 2: Serve bằng Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
# Cấu hình để Nginx hỗ trợ React Router (SPA)
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]