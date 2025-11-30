#!/bin/sh

# 1. Tạo các thư mục con bắt buộc nếu chưa có
mkdir -p /var/www/storage/framework/cache/data
mkdir -p /var/www/storage/framework/sessions
mkdir -p /var/www/storage/framework/views
mkdir -p /var/www/storage/logs

# 2. Xóa cache cũ có thể gây lỗi permission (nếu có)
rm -rf /var/www/storage/framework/cache/data/*

# 3. Cấp quyền 777 (Full quyền) cho toàn bộ thư mục storage
# Vì chạy trên Windows mount vào, 777 là giải pháp ổn định nhất cho dev
chmod -R 777 /var/www/storage
chmod -R 777 /var/www/bootstrap/cache

# 4. Clear cache của Laravel để đảm bảo config mới nhận
php artisan optimize:clear

# 5. Chạy lệnh chính
exec "$@"
