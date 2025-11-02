import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import "../../App.css";

const AdminSidebar = () => {
  return (
    <div className="d-flex" style={{ minHeight: "100vh" }}>
      {/* Sidebar */}
      <div className="sidebar d-flex flex-column shadow-sm">
        <h2 className="sidebar-header text-center fw-bold mb-3">
          Phòng Khám XYZ
        </h2>

        <div className="user-info text-center border-bottom pb-3 mb-3">
          <p className="mb-0 opacity-75">Xin chào,</p>
          <strong>Admin</strong>
        </div>

        <nav>
          <ul className="nav flex-column nav-list">
            <li>
              <NavLink to="/doctor/today-appointment" className="nav-item">
                <i className="fa-solid fa-chart-line"></i>
                Lịch Khám Hôm Nay
              </NavLink>
            </li>
            <li>
              <NavLink to="/doctor/schedule" className="nav-item">
                <i className="fa-solid fa-calendar-days"></i>
                Lịch Làm Việc
              </NavLink>
            </li>
            <li>
              <NavLink to="/doctor/patient-history" className="nav-item">
                <i className="fa-solid fa-history"></i>
                Lịch Sử Bệnh Nhân
              </NavLink>
            </li>
            
            <li className="logout border-top mt-auto pt-3">
              <NavLink to="/logout" className="nav-item">
                <i className="fa-solid fa-right-from-bracket"></i>
                Đăng Xuất
              </NavLink>
            </li>
          </ul>
        </nav>
      </div>

      {/* Nội dung trang con */}
      <div className="flex-grow-1 ">
        <Outlet />
      </div>
    </div>
  );
};

export default AdminSidebar;
