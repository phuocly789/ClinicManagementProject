<?php

namespace App\Http\Controllers\API\Receptionist;

use App\Exceptions\AppErrors;
use App\Http\Controllers\Controller;
use App\Http\Services\ReceptionistService;
use App\Models\Appointment;
use App\Models\MedicalRecord;
use App\Models\Notification;
use App\Models\Patient;
use App\Models\Queue;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Validator;

class ReceptionController extends Controller
{
    protected $receptionistService;
    public function __construct(ReceptionistService $receptionistService)
    {
        $this->receptionistService = $receptionistService;
    }
    public function completeReception(Request $request)
    {

        $request->validate([
            'patient' => 'nullable|array',
            'patient.FullName' => 'required_with:patient|string|max:100',
            'patient.Phone' => 'required_with:patient|string|max:15',
            'appointment.StaffId' => 'required|integer',
            'appointment.RoomId' => 'required|integer',
            'appointment.AppointmentDate' => 'required|date',
            'appointment.AppointmentTime' => 'required|date_format:H:i',
            'receptionType' => 'required|string|in:online,direct',
        ]);

        try {
            DB::beginTransaction();

            $createdBy = auth()->id();
            $today = now('Asia/Ho_Chi_Minh')->toDateString();

            // 1: CHECK CHỖ TRỐNG & LOCK KHUNG GIỜ
            // Đếm số lượng appointment đang active trong khung giờ này
            // lockForUpdate() sẽ ngăn các request khác chen ngang khi đang đếm
            $currentCount = Appointment::where('AppointmentDate', $request->appointment['AppointmentDate'])
                ->where('AppointmentTime', $request->appointment['AppointmentTime'])
                ->whereIn('Status', ['Ordered', 'Waiting', 'InProgress'])
                ->count();

            if ($currentCount >= 10) {
                throw new \Exception('Khung giờ này vừa đầy. Vui lòng chọn giờ khác.');
            }

            // 2: XỬ LÝ BỆNH NHÂN
            $patientId = $request->existingPatientId;

            // Nếu chưa có ID nhưng có thông tin form -> Tạo mới hoặc tìm theo SĐT
            if (!$patientId && $request->has('patient')) {
                $phone = $request->patient['Phone'];

                // Check xem SĐT đã tồn tại chưa để tránh tạo trùng User
                $existingUser = User::where('Phone', $phone)->first();

                if ($existingUser) {
                    $patientId = $existingUser->UserId;
                    // Có thể update thông tin user tại đây nếu cần
                } else {
                    // Tạo User & Patient mới
                    $user = User::create([
                        'Username' => $phone,
                        'PasswordHash' => bcrypt($phone),
                        'FullName' => $request->patient['FullName'],
                        'Phone' => $phone,
                        'Email' => $request->patient['Email'] ?? null,
                        'Gender' => $request->patient['Gender'] ?? null,
                        'Address' => $request->patient['Address'] ?? null,
                        'DateOfBirth' => $request->patient['DateOfBirth'] ?? null,
                        'MustChangePassword' => true,
                        'IsActive' => true
                    ]);

                    Patient::create([
                        'PatientId' => $user->UserId,
                        'MedicalHistory' => $request->patient['MedicalHistory'] ?? null
                    ]);
                    $patientId = $user->UserId;
                }
            }

            if (!$patientId) throw new \Exception('Thiếu thông tin bệnh nhân');

            // 3: XỬ LÝ HỒ SƠ BỆNH ÁN
            $record = MedicalRecord::firstOrCreate(
                ['PatientId' => $patientId, 'Status' => 'Active'],
                [
                    'RecordNumber' => 'MR-' . date('YmdHis') . '-' . $patientId,
                    'IssuedDate' => $today,
                    'Notes' => 'Hồ sơ được tạo khi tiếp nhận',
                    'CreatedBy' => $createdBy,
                ]
            );

            // 4: XỬ LÝ APPOINTMENT
            $appointment = null;

            // TRƯỜNG HỢP A: TIẾP NHẬN TỪ ONLINE
            if ($request->receptionType === 'online' && $request->has('original_appointment_id')) {

                // Tìm và KHÓA dòng appointment cũ
                $appointment = Appointment::where('AppointmentId', $request->original_appointment_id)
                    ->lockForUpdate()
                    ->first();

                if (!$appointment) {
                    throw new \Exception('Lịch hẹn gốc không tồn tại.');
                }

                // CHỐT CHẶN TRÙNG LẶP: Nếu status không phải "Ordered" nghĩa là đã có người khác xử lý rồi
                if ($appointment->Status !== 'Ordered') {
                    throw new \Exception('Lịch hẹn này đã được nhân viên khác tiếp nhận rồi!');
                }

                // Cập nhật thông tin mới vào lịch hẹn cũ
                $appointment->update([
                    'StaffId' => $request->appointment['StaffId'],
                    'RoomId' => $request->appointment['RoomId'],
                    'RecordId' => $record->RecordId,
                    'Notes' => $request->appointment['Notes'] ?? $appointment->Notes,
                    'Status' => 'Waiting',
                    'ServiceType' => $request->appointment['ServiceType'] ?? 'Khám bệnh'
                ]);
            }
            // TRƯỜNG HỢP B: TIẾP NHẬN TRỰC TIẾP (Tạo mới)
            else {
                // Check duplicate: Bệnh nhân này đã có lịch active hôm nay chưa?
                $duplicateCheck = Appointment::where('PatientId', $patientId)
                    ->where('AppointmentDate', $today)
                    ->whereIn('Status', ['Ordered', 'Waiting', 'InProgress'])
                    ->exists();

                if ($duplicateCheck) {
                    throw new \Exception('Bệnh nhân này đã có lịch hẹn/đang khám trong ngày hôm nay.');
                }

                // Tạo appointment mới
                $appointment = Appointment::create([
                    'PatientId' => $patientId,
                    'StaffId' => $request->appointment['StaffId'],
                    'RoomId' => $request->appointment['RoomId'],
                    'RecordId' => $record->RecordId,
                    'AppointmentDate' => $request->appointment['AppointmentDate'],
                    'AppointmentTime' => $request->appointment['AppointmentTime'],
                    'Status' => 'Waiting',
                    'CreatedBy' => $createdBy,
                    'Notes' => $request->appointment['Notes'] ?? null,
                    'ServiceType' => $request->appointment['ServiceType'] ?? 'Khám bệnh'
                ]);
            }

            // 5: TẠO QUEUE
            $lastQueue = Queue::where('QueueDate', $today)
                ->where('RoomId', $request->appointment['RoomId'])
                ->orderBy('QueueNumber', 'desc')
                ->lockForUpdate()
                ->first();

            // phòng chưa có ai → số 1
            $newQueueNumber = $lastQueue ? $lastQueue->QueueNumber + 1 : 1;

            $queue = Queue::create([
                'PatientId' => $patientId,
                'AppointmentId' => $appointment->AppointmentId,
                'RecordId' => $record->RecordId,
                'RoomId' => $request->appointment['RoomId'],
                'QueueNumber' => $newQueueNumber,
                'QueueDate' => $today,
                'QueueTime' => $request->appointment['AppointmentTime'],
                'Status' => 'Waiting',
                'CreatedBy' => $createdBy
            ]);


            DB::commit();

            // Load lại info để trả về
            $patientInfo = User::find($patientId);

            return response()->json([
                'success' => true,
                'data' => [
                    'queue' => $queue,
                    'patient' => $patientInfo,
                    'queueNumber' => $newQueueNumber
                ],
                'message' => 'Tiếp nhận thành công. Số phiếu: ' . $newQueueNumber
            ], 200);
        } catch (\Exception $e) {
            DB::rollBack();
            return response()->json([
                'success' => false,
                'message' => $e->getMessage(),
                'error' => $e->getMessage()
            ], 400);
        }
    }

}
