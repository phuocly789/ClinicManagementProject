// src/layouts/ReceptionistSidebar.jsx
import React, { useState, useEffect } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";

const ReceptionistSidebar = () => {
  const [isAppointmentOpen, setIsAppointmentOpen] = useState(true);
  const location = useLocation();

  // Tự động mở dropdown nếu đang ở trang con của lịch hẹn
  useEffect(() => {
    const appointmentPaths = [
      "/receptionist/appointment-management",
      "/receptionist/appointment-schedule",
      "/receptionist/appointment-create",
      "/receptionist/appointment-update",
    ];
    if (appointmentPaths.some(path => location.pathname.startsWith(path))) {
      setIsAppointmentOpen(true);
    }
  }, [location]);

  return (
    <div className="d-flex" style={{ minHeight: "100vh" }}>
      {/* Sidebar */}
      <div className="bg-white border-end shadow-sm d-flex flex-column" style={{ width: "280px" }}>

        {/* Header */}
        <div className="p-4 text-center border-bottom">
          <h4 className="fw-bold text-primary mb-0">Phòng Khám XYZ</h4>
        </div>

        {/* User Info */}
        <div className="text-center py-4 border-bottom">
          <div className="bg-primary text-white rounded-circle mx-auto d-flex align-items-center justify-content-center mb-2"
               style={{ width: "60px", height: "60px", fontSize: "1.8rem" }}>
            L
          </div>
          <p className="mb-1 text-muted small">Xin chào</p>
          <h6 className="fw-bold text-dark mb-0">Lễ tân</h6>
        </div>

        {/* Navigation */}
        <div className="flex-grow-1 overflow-auto py-3 px-2">
          <div className="list-group list-group-flush">

            {/* Dropdown: Quản lý lịch hẹn */}
            <div className="list-group-item border-0 px-3">
              <a
                href="#appointmentDropdown"
                className={`d-flex justify-content-between align-items-center text-decoration-none py-3 px-3 rounded-3 ${
                  isAppointmentOpen ? "bg-primary text-white" : "text-dark bg-light"
                }`}
                data-bs-toggle="collapse"
                onClick={(e) => {
                  e.preventDefault();
                  setIsAppointmentOpen(!isAppointmentOpen);
                }}
              >
                <span>
                  <i className="fas fa-calendar-check me-3"></i>
                  Quản Lý Lịch Hẹn
                </span>
                <i className={`fas fa-chevron-${isAppointmentOpen ? "down" : "right"}`}></i>
              </a>

              {/* Collapse Menu */}
              <div className={`collapse ${isAppointmentOpen ? "show" : ""}`} id="appointmentDropdown">
                <div className="pt-2">
                  {[
                    { to: "/receptionist/appointment-management", icon: "list-ul", text: "Danh sách lịch hẹn" },
                    { to: "/receptionist/appointment-schedule", icon: "calendar-week", text: "Lịch làm việc bác sĩ" },
                    { to: "/receptionist/appointment-create", icon: "plus-circle", text: "Tạo lịch hẹn mới" },
                  ].map((item, idx) => (
                    <NavLink
                      key={idx}
                      to={item.to}
                      className={({ isActive }) =>
                        `d-block text-decoration-none py-2 px-4 rounded-3 mb-1 text-start ${
                          isActive
                            ? "bg-primary text-white"
                            : "text-secondary hover-bg-light"
                        }`
                      }
                      end
                    >
                      <i className={`fas fa-${item.icon} me-3`}></i>
                      {item.text}
                    </NavLink>
                  ))}
                </div>
              </div>
            </div>

            {/* Hồ sơ bệnh án */}
            <NavLink
              to="/receptionist/create-medical-record"
              className={({ isActive }) =>
                `list-group-item border-0 text-decoration-none d-flex align-items-center py-3 px-3 rounded-3 mb-1 ${
                  isActive ? "bg-primary text-white" : "text-dark bg-light"
                }`
              }
            >
              <i className="fas fa-plus me-3"></i>
              Đăng Ký Hồ sơ bệnh án
            </NavLink>

            {/* Đăng ký bệnh nhân */}
            <NavLink
              to="/receptionist/create-patient"
              className={({ isActive }) =>
                `list-group-item border-0 text-decoration-none d-flex align-items-center py-3 px-3 rounded-3 mb-1 ${
                  isActive ? "bg-primary text-white" : "text-dark bg-light"
                }`
              }
            >
              <i className="fas fa-user-plus me-3"></i>
              Đăng Ký Bệnh Nhân
            </NavLink>
          </div>
        </div>

        {/* Logout */}
        <div className="border-top p-3">
          <NavLink
            to="/logout"
            className="d-flex align-items-center text-danger text-decoration-none py-3 px-3 rounded-3 bg-light hover-bg-danger hover-text-white transition"
          >
            <i className="fas fa-right-from-bracket me-3"></i>
            Đăng Xuất
          </NavLink>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-grow-1 bg-light">
        <Outlet />
      </div>
    </div>
  );
};

export default ReceptionistSidebar;