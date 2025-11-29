# Hệ thống Quản lý Phòng khám Dịch vụ

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![React](https://img.shields.io/badge/React-61DAFB?style=for-the-badge&logo=react&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)

Dự án xây dựng một hệ thống phần mềm dựa trên web nhằm tối ưu hóa và quản lý toàn diện quy trình hoạt động của một phòng khám dịch vụ.

## 🎯 Mục tiêu dự án

Xây dựng một hệ thống phần mềm web cho phép phòng khám hoạt động hiệu quả hơn bằng cách quản lý toàn bộ quy trình khám bệnh:

> Từ tiếp nhận bệnh nhân → đặt lịch → chờ khám  
> Đến khám bệnh, chỉ định xét nghiệm → trả kết quả → kê toa  
> Quản lý thuốc, quản lý dịch vụ, bác sĩ, thống kê doanh thu

---

## 👥 Người thực hiện

- Lý Minh Phước  
- Nguyễn Đắc Nhân Tâm

---

## 📋 Yêu cầu kỹ thuật

| Thành phần      | Công nghệ gợi ý                  |
|-----------------|----------------------------------|
| **Backend**     | ASP.NET Core Web API             |
| **Frontend**    | Blazor / React / Angular / Vue / JS Thuần (dự án này dùng **React + Vite**) |
| **Database**    | **PostgreSQL**                   |
| **Auth**        | JWT (JSON Web Tokens)            |
| **Triển khai**  | Localhost hoặc Cloud (Azure, Docker tùy chọn) |

---

## ⭐ Chức năng bắt buộc

Hệ thống phân quyền chặt chẽ cho 4 vai trò:

### 📌 Bệnh nhân
- Đăng ký, đăng nhập
- Đặt lịch khám (chọn ngày, giờ, bác sĩ)
- Xem lịch sử khám bệnh, đơn thuốc
- Xem kết quả xét nghiệm

### 📌 Lễ tân
- Tạo lịch khám trực tiếp cho bệnh nhân
- Quản lý danh sách bệnh nhân chờ khám
- Quản lý, sắp xếp lịch làm việc bác sĩ

### 📌 Bác sĩ
- Xem danh sách bệnh nhân trong ngày
- Ghi nhận triệu chứng, chẩn đoán
- Chỉ định xét nghiệm
- Kê đơn thuốc điện tử

### 📌 Admin
- Quản lý tất cả người dùng
- Quản lý danh mục dịch vụ, thuốc (tồn kho, giá)
- Xem thống kê lượt khám & doanh thu

---

## 🔄 Quy trình nghiệp vụ

1. **Tiếp nhận & Đặt lịch**: Bệnh nhân đặt online hoặc lễ tân tạo trực tiếp → trạng thái: `Đã đặt` → `Đang chờ` → `Đã khám`
2. **Khám bệnh**: Bác sĩ ghi nhận thông tin, chỉ định xét nghiệm nếu cần
3. **Xét nghiệm**: Kỹ thuật viên nhập kết quả
4. **Kê toa**: Bác sĩ xem kết quả → kê đơn → hệ thống trừ tồn kho thuốc
5. **Thanh toán**: Tự động tính phí khám + dịch vụ + thuốc**, xuất hóa đơn

---

## 🔐 Bảo mật & Xác thực

- Mật khẩu được mã hóa bằng **Bcrypt**
- Xác thực API bằng **JWT**
- Phân quyền theo vai trò (RBAC)
- Input validation + Rate limiting

---

## 💡 Tính năng nâng cao (Tùy chọn triển khai)

- Gửi SMS/Email nhắc lịch khám
- Chat nội bộ giữa nhân viên
- Quản lý đa chi nhánh
- Tạo QR code tra cứu hồ sơ nhanh
- Báo cáo, biểu đồ doanh thu chi tiết

---

## 🚀 Hướng dẫn cài đặt & chạy dự án

### Yêu cầu
- .NET 8 SDK
- Node.js ≥ 18
- PostgreSQL ≥ 13
- Git

### Các bước thực hiện

```bash
# 1. Clone dự án
git clone https://github.com/username/phongkham-dichvu.git
cd phongkham-dichvu

# 2. Cấu hình Database
# Tạo database trong PostgreSQL
createdb phongkham_db

# Cập nhật connection string trong file:
# Backend/appsettings.Development.json
# Thay YourPassword bằng mật khẩu PostgreSQL của bạn
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=phongkham_db;Username=postgres;Password=YourPassword"
}

# 3. Chạy migrations để tạo bảng
cd Backend
dotnet ef migrations add InitialCreate
dotnet ef database update

# 4. Chạy Backend (mở terminal thứ nhất)
dotnet watch run
# API sẽ chạy tại: https://localhost:7001

# 5. Chạy Frontend (mở terminal thứ hai)
cd ../Frontend
npm install
npm run dev
# Frontend sẽ chạy tại: http://localhost:5173  
  
### Tài khoản đăng nhập mẫu (đã được seed sẵn trong database)

| Vai trò       | Email                       | Mật khẩu |
|---------------|-----------------------------|----------|
| Admin         | admin@phongkham.com         | 123456   |
| Bác sĩ        | bs.khang@phongkham.com      | 123456   |
| Lễ tân        | lt.hang@phongkham.com       | 123456   |
| Bệnh nhân     | bn.an@gmail.com             | 123456   |

> Lưu ý: Tất cả mật khẩu đều đã được mã hóa bằng Bcrypt trong database.  
> Bạn có thể đăng nhập ngay sau khi chạy xong dự án mà không cần đăng ký thêm.
