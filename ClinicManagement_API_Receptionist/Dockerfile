FROM php:8.3-fpm

# 1. Cài đặt thư viện hệ thống
RUN apt-get update && apt-get install -y \
    git \
    curl \
    libpng-dev \
    libonig-dev \
    libxml2-dev \
    libzip-dev \
    zip \
    unzip \
    libpq-dev

# 2. Cài đặt PHP extensions
RUN docker-php-ext-install pdo_pgsql mbstring exif pcntl bcmath gd zip

# 3. Cài đặt Composer
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www

# 4. Copy source code
COPY . .

# 5. Cài đặt dependencies
RUN composer install --no-interaction --optimize-autoloader --no-dev

# 6. Phân quyền ban đầu (để build image không lỗi)
RUN chown -R www-data:www-data /var/www/storage /var/www/bootstrap/cache
RUN chmod -R 775 /var/www/storage /var/www/bootstrap/cache

# 7. Setup Entrypoint Script
COPY docker/entrypoint.sh /usr/local/bin/entrypoint.sh

# --- QUAN TRỌNG: Fix lỗi dòng lệnh Windows (CRLF -> LF) ---
RUN sed -i 's/\r$//' /usr/local/bin/entrypoint.sh

# Cấp quyền thực thi cho script
RUN chmod +x /usr/local/bin/entrypoint.sh

# 8. Chạy
ENTRYPOINT ["entrypoint.sh"]
CMD ["php-fpm"]
