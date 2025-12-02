<?php

use App\Http\Controllers\API\Service\AdminServiceController;
use App\Events\AppointmentUpdated;
use App\Http\Controllers\API\Receptionist\AppointmentRecepController;
use App\Http\Controllers\API\Receptionist\RoomController;
use App\Http\Controllers\API\ReportRevenueController;
use App\Http\Controllers\API\ScheduleController;
use Dba\Connection;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use App\Http\Controllers\API\MedicinesController;
use App\Http\Controllers\API\UserController;
use App\Http\Controllers\API\ImportBillController;
use App\Http\Controllers\API\SuppliersController;

use App\Http\Controllers\API\AuthController;
use App\Http\Controllers\API\DashboardController;
use App\Http\Controllers\API\Doctor\AISuggestionController;
use App\Http\Controllers\API\Doctor\AppointmentsController;
use App\Http\Controllers\API\Doctor\DiagnosisSuggestionController;
use App\Http\Controllers\API\Doctor\DoctorMedicineSearchController;
use App\Http\Controllers\API\Doctor\ServiceController;
use App\Http\Controllers\API\Doctor\DoctorExaminationsController;
use App\Http\Controllers\API\Doctor\PatientsController;
use App\Http\Controllers\API\PatientController;
use App\Http\Controllers\API\Payment\InvoiceController;
use App\Http\Controllers\API\Payment\PaymentController;
use App\Http\Controllers\API\PrescriptionAnalyticsController;
//----------------------------------------------Hết-------------------------------
use App\Http\Controllers\API\User\AdminUserController;
use App\Http\Controllers\API\Print\InvoicePrintController;
use App\Http\Controllers\API\Receptionist\MedicalStaffController;
use App\Http\Controllers\API\Receptionist\PatientByRecepController;
use App\Http\Controllers\API\Receptionist\QueueController;
use App\Http\Controllers\API\Receptionist\ReceptionController;
use App\Http\Controllers\API\RevenueForecastController;
use App\Http\Controllers\API\Technician\TestResultsController;
use App\Http\Controllers\TestWebSocketController;
use App\Http\Controllers\API\SearchController;

Route::get('/rooms', [RoomController::class, 'getAllRooms']);

Route::options('/{any}', function () {
    return response()->json([], 200);
})->where('any', '.*');

//Receptionist Routes
Route::prefix('receptionist')->group(function () {
    //lịch hẹn
    Route::get('/appointments/today', [AppointmentRecepController::class, 'GetAppointmentToday']);
    Route::get('/appointments/count-by-timeslot', [AppointmentRecepController::class, 'getAppointmentCountByTimeSlot']);
    Route::get('/appointments/counts-by-timeslots', [AppointmentRecepController::class, 'getAppointmentCountsByTimeSlots']);
    //hàng chờ
    Route::get('/queue', [QueueController::class, 'GetQueueByDate']);
    Route::get('/queue/{room_id}', [QueueController::class, 'GetQueueByRoomAndDate']);
    Route::post('/queueNoDirect', [QueueController::class, 'CreateQueue']);
    Route::post('/queueDirect', [QueueController::class, 'CreateDicrectAppointment']);
    Route::put('/queue/{queueId}/status', [QueueController::class, 'UpdateQueueStatus']);
    Route::delete('/queue/{queueId}', [QueueController::class, 'DeleteQueue']);
    //Rooms
    Route::get('/rooms', [RoomController::class, 'getAllRooms']);
    //Tiếp nhận patient
    Route::get('/searchPatient', [PatientByRecepController::class, 'searchPatients']); // Giữ cũ
    Route::get('/patients', [PatientByRecepController::class, 'getPatient']);
    // Thêm route này vào receptionist routes
    Route::get('/patients/{patientId}', [PatientByRecepController::class, 'getPatientDetails']);
    // Medical staff routes
    Route::get('/medical-staff/schedules', [MedicalStaffController::class, 'getDoctorsWithSchedules']);
    Route::get('/medical-staff/room/{roomId}', [MedicalStaffController::class, 'getDoctorsByRoom']);

    // Complete reception
    Route::post('/complete', [ReceptionController::class, 'completeReception']);

    // Online appointments
    Route::get('/appointments/online', [AppointmentRecepController::class, 'getOnlineAppointments']);
});
