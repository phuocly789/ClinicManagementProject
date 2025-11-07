// src/pages/UpdateAppointment.jsx
import React from "react";
import { Button } from "react-bootstrap";
import { Edit } from "lucide-react";
import { useNavigate } from "react-router-dom";

const UpdateAppointment = () => {
  const navigate = useNavigate();

  return (
    <Button
      variant="link"
      className="text-white d-flex align-items-center gap-2 p-0"
      onClick={() => navigate("/appointment-list")}
      style={{ textDecoration: "none" }}
    >
      <Edit size={18} /> Cập Nhật Lịch Hẹn
    </Button>
  );
};

export default UpdateAppointment;