import React from "react";
import { NavLink, Outlet } from "react-router-dom";
import "../../App.css";

const ReceptionistSidebar = () => {
  return (
    <div className="d-flex" style={{ Height: "100vh" }}>
      {/* Sidebar */}
      <div className="sidebar d-flex flex-column shadow-sm">
        <h2 className="sidebar-header text-center fw-bold mb-3">
          Phòng Khám XYZ
        </h2>

        <div className="user-info text-center border-bottom pb-3 mb-3">
          <p className="mb-0 opacity-75">Xin chào,</p>
          <strong>Lễ tân</strong>
        </div>

        <nav>
          <ul className="nav flex-column nav-list">
            <li>
              <NavLink to="/receptionist/appointment-management" className="nav-item">
                <i className="fa-solid fa-calendar-check"></i>
                Quản Lý Lịch Hẹn
              </NavLink>
            </li>
            <li>
              <NavLink to="/receptionist/appointment-create" className="nav-item">
                <i className="fa-solid fa-plus-circle"></i>
                Tạo Lịch Hẹn
              </NavLink>
            </li>
            <li>
              <NavLink to="/receptionist/appointment-update" className="nav-item">
                <i className="fa-solid fa-pen-to-square"></i>
                Cập Nhật Lịch Hẹn
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
      <div className="flex-grow-1 main-content">
        <Outlet />
      </div>
    </div>
  );
};

export default ReceptionistSidebar;
