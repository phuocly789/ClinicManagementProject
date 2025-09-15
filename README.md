# Hệ thống Quản lý Phòng khám Dịch vụ

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)

Dự án xây dựng một hệ thống phần mềm dựa trên web nhằm tối ưu hóa và quản lý toàn diện quy trình hoạt động của một phòng khám dịch vụ.

## 🎯 Mục tiêu dự án

Xây dựng một hệ thống phần mềm web cho phép phòng khám hoạt động hiệu quả hơn bằng cách quản lý toàn bộ quy trình khám bệnh:
> * Từ tiếp nhận bệnh nhân → đặt lịch → chờ khám
> * Đến khám bệnh, chỉ định xét nghiệm → trả kết quả → kê toa
> * Quản lý thuốc, quản lý dịch vụ, bác sĩ, thống kê doanh thu

---

## 👥 Người thực hiện
* Lý Minh Phước
* Nguyễn Đắc Nhân Tâm

---

## 📋 Yêu cầu kỹ thuật

| Thành phần | Công nghệ gợi ý |
| :--- | :--- |
| **Backend** | ASP.NET Core Web API |
| **Frontend** | Blazor / React / Angular / Vue / JS Thuần |
| **Database** | **PostgreSQL** |
| **Auth** | JWT (JSON Web Tokens) |
| **Triển khai** | Localhost hoặc Cloud (Azure, Docker tùy chọn) |

---

## ⭐ Chức năng bắt buộc

Hệ thống được phân quyền chặt chẽ cho 4 vai trò người dùng chính:

#### 📌 Bệnh nhân
* Đăng ký, đăng nhập vào hệ thống.
* Đặt lịch khám (chọn ngày, giờ, bác sĩ).
* Xem lại lịch sử khám bệnh, đơn thuốc đã được kê.
* Xem kết quả các dịch vụ xét nghiệm.

#### 📌 Lễ tân
* Tạo lịch khám cho bệnh nhân đến trực tiếp tại phòng khám.
* Quản lý danh sách bệnh nhân đang chờ khám.
* Quản lý và sắp xếp lịch làm việc của bác sĩ.

#### 📌 Bác sĩ
* Xem danh sách bệnh nhân được chỉ định khám trong ngày.
* Ghi nhận chẩn đoán, triệu chứng và các thông tin liên quan.
* Chỉ định các dịch vụ xét nghiệm cần thiết.
* Kê toa thuốc điện tử cho bệnh nhân.

#### 📌 Admin
* Quản lý danh sách tất cả người dùng (bệnh nhân, nhân viên).
* Quản lý danh mục các dịch vụ của phòng khám (khám, xét nghiệm…).
* Quản lý danh mục thuốc (tên, đơn vị, giá, tồn kho).
* Xem thống kê tổng số lượt khám và doanh thu.

---

## 🔄 Quy trình nghiệp vụ mô phỏng

1.  **Tiếp nhận & Đặt lịch khám:** Bệnh nhân có thể đặt lịch online hoặc lễ tân tạo lịch trực tiếp. Mỗi lịch khám sẽ có trạng thái: `Đã đặt` / `Đang chờ` / `Đã khám`.
2.  **Khám bệnh:** Bác sĩ xem thông tin bệnh nhân, ghi nhận triệu chứng, chẩn đoán và có thể chỉ định dịch vụ xét nghiệm.
3.  **Xét nghiệm:** Kỹ thuật viên cập nhật kết quả xét nghiệm lên hệ thống.
4.  **Kê toa & Đơn thuốc:** Sau khi có kết quả, bác sĩ xem và đưa ra kết luận, kê đơn thuốc và ghi chú hướng dẫn điều trị. Hệ thống quản lý kho thuốc: tồn kho, số lượng đã cấp, đơn giá.
5.  **Thanh toán:** Hệ thống tự động tính tổng chi phí bao gồm phí khám, dịch vụ và thuốc. Hỗ trợ quản lý hoá đơn, in và tra cứu lịch sử thanh toán.

---

## 🔐 Bảo mật & Xác thực
* **Mã hoá mật khẩu:** Sử dụng thuật toán **Bcrypt** để mã hóa mật khẩu người dùng, đảm bảo an toàn cho dữ liệu.
* **Xác thực API:** Sử dụng **JWT (JSON Web Token)** để xác thực đăng nhập và bảo vệ các API endpoint.
* **Phân quyền chặt chẽ:** Xây dựng hệ thống phân quyền theo vai trò (Role-Based Access Control) để đảm bảo người dùng chỉ có thể truy cập các chức năng được phép.
* **Validation:** Kiểm tra dữ liệu đầu vào (Input Validation) và giới hạn truy cập API (Rate Limiting) để chống lại các tấn công phổ biến.

---

## 💡 Tính năng nâng cao (Tùy chọn)
* Tích hợp gửi SMS hoặc email để nhắc lịch khám cho bệnh nhân.
* Xây dựng module chat nội bộ giữa các nhân viên (lễ tân – bác sĩ).
* Mở rộng hệ thống để quản lý nhiều chi nhánh phòng khám.
* Tạo mã QR code để bệnh nhân có thể tra cứu nhanh hồ sơ sức khỏe.
* Xây dựng các báo cáo, biểu đồ doanh thu chi tiết theo ngày/tháng/năm.

---



## 📝 Giấy phép
Dự án này được cấp phép theo Giấy phép MIT.
