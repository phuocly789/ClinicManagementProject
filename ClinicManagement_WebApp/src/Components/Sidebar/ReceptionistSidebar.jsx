import React, { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import "../../App.css";
import authService from "../../services/authService";
import { path } from "../../utils/constant";

const ReceptionistSidebar = () => {
  const [fullName, setFullName] = useState("");

  useEffect(() => {
    const user = authService.getFullNameFromToken();
    setFullName(user);
  })
  return (
    <div className="d-flex" style={{ Height: "100vh" }}>
      {/* Sidebar */}
      <div className="sidebar d-flex flex-column shadow-sm">
        <img src="/logo1.png" alt="logo" className="sidebar-logo" />
        <h2 className="sidebar-header text-center fw-bold mb-3">
          Phòng Khám VitaCare
        </h2>

        <div className="user-info text-center border-bottom pb-3 mb-3">
          <p className="mb-0 opacity-75">Xin chào,</p>
          <strong>{fullName}</strong>
        </div>

        <nav>
          <ul className="nav flex-column nav-list">
            <li>
              <NavLink to={path.RECEPTIONIST.DASHBOARD} className="nav-item">
                <i className="fa-solid fa-list-check"></i>
                Hàng Đợi Khám
              </NavLink>
            </li>

            <li>
              <NavLink to={path.RECEPTIONIST.PATIENT_MANAGEMENT} className="nav-item">
                <i className="fa-solid fa-user-check"></i>
                Tiếp Nhận Bệnh Nhân
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
      <div className="flex-grow-1  ">
        <Outlet />
      </div>
    </div>
  );
};

export default ReceptionistSidebar;
