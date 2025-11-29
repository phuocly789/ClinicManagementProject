
import { BrowserRouter, Routes, Route } from "react-router-dom";
import "./App.css";

import { path } from "./utils/constant";
import LoginPage from "./pages/auth/Login/Login";
import AdminMedicine from "./pages/Admin/AdminMedicine.jsx";
import AdminInventory from "./pages/Admin/AdminInventory.jsx";
import DoctorDashboard from "./pages/Doctors/DoctorDashboard.jsx";
import AdminDashboard from "./pages/Admin/AdminDashboard.jsx";
import Register from "./pages/auth/Register/Register.jsx";
import AdminRevenueReport from "./pages/Admin/AdminRevenueReport.jsx";
import AdminScheduleManagement from "./pages/Admin/AdminScheduleManagement.jsx";
import AdminUserManagement from "./pages/Admin/AdminUserManagement.jsx";
import AdminSuppliers from "./pages/Admin/AdminSuppliers";
import VerifyEmailPage from "./pages/auth/VerifyEmail/EmailVerification.jsx";
import PatientProfile from "./pages/Patient/PatientProfile.jsx";
import PatientLayout from "./Components/Patient/PatientLayout.jsx";
import AdminSidebar from "./Components/Sidebar/AdminSidebar.jsx";
import DoctorSidebar from "./Components/Sidebar/DoctorSidebar.jsx";
import Home from "./pages/Home.jsx";
import TechnicianDashboard from "./pages/Technician/TechnicianDashboard.jsx";
import PrivateRoute from "./Components/PrivateRoute.jsx";
import Logout from "./pages/auth/Logout.jsx";
import AdminService from "./pages/Admin/AdminService.jsx";
import AdminSupplier from "./pages/Admin/AdminSupplier.jsx";
import DoctorSchedule from "./pages/Doctors/DoctorSchedule.jsx";
import ReceptionistSidebar from "./Components/Sidebar/ReceptionistSidebar.jsx";
import AppointmentDashboard from "./pages/Receptionist/AppointmentDashboard.jsx";
import CreateAppointment from "./pages/Receptionist/CreateAppointment.jsx";
import UpdateAppointment from "./pages/Receptionist/UpdateAppointment.jsx";
import ReceptionistScheduleManagement from "./pages/Receptionist/ReceptionistScheduleManagement.jsx";
import AppointmentManagement from "./pages/Receptionist/AppointmentManagement.jsx";
import CreatePatient from "./pages/Receptionist/CreatePatient.jsx";
import CreateMedicalRecord from "./pages/Receptionist/CreateMedicalRecord.jsx";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Home */}
        <Route path={path.HOME} element={<Home />} />
        {/* Admin */}
        <Route element={<PrivateRoute allowedRoles={['Admin']} />}>
          <Route path={path.ADMIN.ROOT} element={<AdminSidebar />} >
            <Route path={path.ADMIN.DASHBOARD} element={<AdminDashboard />} />
            <Route path={path.ADMIN.USER.MANAGEMENT} element={<AdminUserManagement />} />
            <Route path={path.ADMIN.REVENUE_REPORT} element={<AdminRevenueReport />} />
            <Route path={path.ADMIN.SCHEDULE.MANAGEMENT} element={<AdminScheduleManagement />} />
            <Route path={path.ADMIN.SERVICE.MANAGEMENT} element={<AdminService />} />
            <Route path={path.ADMIN.SUPPLIERS.MANAGEMENT} element={<AdminSupplier />} />
            <Route path={path.ADMIN.MEDICINE.MANAGEMENT} element={<AdminMedicine />} />
            <Route path={path.ADMIN.INVENTORY} element={<AdminInventory />} />
            <Route path={path.ADMIN.SUPPLIERS.MANAGEMENT} element={<AdminSuppliers />} />
            <Route path={path.ADMIN.MEDICINE.MANAGEMENT} element={<AdminMedicine />} />
          </Route>
        </Route>
        {/* Receptionist */}
        <Route element={<PrivateRoute allowedRoles={['Receptionist']} />}>
          <Route path={path.RECEPTIONIST.ROOT} element={<ReceptionistSidebar />}>
            <Route path={path.RECEPTIONIST.APPOINTMENT.MANAGEMENT} element={<AppointmentManagement />} />
            <Route path={path.RECEPTIONIST.APPOINTMENT.CREATE} element={<CreateAppointment />} />
            <Route path={path.RECEPTIONIST.APPOINTMENT.UPDATE} element={<UpdateAppointment />} />
            <Route path={path.RECEPTIONIST.APPOINTMENT.SCHEDULE} element={<ReceptionistScheduleManagement />} />
            <Route path={path.RECEPTIONIST.MEDICALRECORD.CREATE} element={<CreateMedicalRecord />} />
            <Route path={path.RECEPTIONIST.USER.CREATE} element={<CreatePatient />} />
          </Route>
        </Route>
        {/* Doctor */}
        <Route element={<PrivateRoute allowedRoles={['Doctor']} />}>
          <Route path={path.DOCTOR.ROOT} element={<DoctorSidebar />} >
            <Route path={path.DOCTOR.TODAYAPPOINTMENT} element={<DoctorDashboard />} />
            <Route path={path.DOCTOR.SCHEDULE} element={<DoctorSchedule />} />

          </Route>
        </Route>
        {/* Technician */}
        <Route element={<PrivateRoute allowedRoles={['Technician']} />}>
        </Route>
        {/* Patient */}
        <Route element={<PrivateRoute allowedRoles={['Patient']} />}>
          <Route path={path.PATIENT.ROOT} element={<PatientLayout />}>
            <Route
              path={path.PATIENT.PROFILE.MANAGEMENT}
              element={<PatientProfile />}
            />
            {/* <Route path={path.PATIENT.BOOKING} element={<PatientBooking />} />
          <Route path={path.PATIENT.HISTORY} element={<PatientHistory />} /> */}
          </Route>
        </Route>
        <Route path={path.LOGIN} element={<LoginPage />} />{" "}
        <Route path={path.REGISTER} element={<Register />} />{" "}
        <Route path={path.LOGOUT} element={<Logout />} />{" "}
        <Route
          path={path.VERIFICATION_EMAIL}
          element={<VerifyEmailPage />}
        />
        {/* Trang mặc định */}
        <Route path="/technician" element={<TechnicianDashboard />} />
      </Routes>
    </BrowserRouter >
  );
}

export default App;
