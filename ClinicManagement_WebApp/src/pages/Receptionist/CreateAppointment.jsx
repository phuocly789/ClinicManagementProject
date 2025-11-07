// src/pages/CreateAppointment.jsx
import React, { useState } from "react";
import { Modal, Button, Form, Row, Col } from "react-bootstrap";
import { Plus } from "lucide-react";

const API_BASE_URL = "http://localhost:5066";

const CreateAppointment = ({ onToast }) => {
  const [show, setShow] = useState(false);
  const [form, setForm] = useState({
    patientName: "",
    staffId: "",
    appointmentDate: "",
    appointmentTime: "",
    notes: ""
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const res = await fetch(`${API_BASE_URL}/api/Appointment`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form)
      });
      if (!res.ok) {
        const err = await res.text();
        throw new Error(err || "Tạo thất bại");
      }
      onToast("success", "Tạo lịch hẹn thành công!");
      setShow(false);
      setForm({ patientName: "", staffId: "", appointmentDate: "", appointmentTime: "", notes: "" });
    } catch (err) {
      onToast("error", err.message);
    }
  };

  return (
    <>
      {/* NÚT TRONG SIDEBAR */}
      <Button
        variant="link"
        className="text-white d-flex align-items-center gap-2 p-0 mb-2"
        onClick={() => setShow(true)}
        style={{ textDecoration: "none" }}
      >
        <Plus size={18} /> Tạo Lịch Hẹn
      </Button>

      {/* MODAL */}
      <Modal show={show} onHide={() => setShow(false)} centered>
        <Modal.Header closeButton>
          <Modal.Title>Tạo Lịch Hẹn Mới</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Tên bệnh nhân</Form.Label>
                  <Form.Control
                    value={form.patientName}
                    onChange={(e) => setForm({ ...form, patientName: e.target.value })}
                    required
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Staff ID</Form.Label>
                  <Form.Control
                    value={form.staffId}
                    onChange={(e) => setForm({ ...form, staffId: e.target.value.replace(/\D/g, '') })}
                    required
                  />
                </Form.Group>
              </Col>
            </Row>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Ngày</Form.Label>
                  <Form.Control
                    type="date"
                    value={form.appointmentDate}
                    onChange={(e) => setForm({ ...form, appointmentDate: e.target.value })}
                    required
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Giờ</Form.Label>
                  <Form.Control
                    type="time"
                    value={form.appointmentTime}
                    onChange={(e) => setForm({ ...form, appointmentTime: e.target.value })}
                    required
                  />
                </Form.Group>
              </Col>
            </Row>
            <Form.Group className="mb-3">
              <Form.Label>Ghi chú</Form.Label>
              <Form.Control
                as="textarea"
                rows={2}
                value={form.notes}
                onChange={(e) => setForm({ ...form, notes: e.target.value })}
              />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShow(false)}>Hủy</Button>
            <Button variant="primary" type="submit">Tạo</Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </>
  );
};

export default CreateAppointment;