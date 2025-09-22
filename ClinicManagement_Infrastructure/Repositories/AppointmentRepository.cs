using ClinicManagement_Infrastructure.Infrastructure.Data;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

public interface IAppointmentRepository : IRepository<Appointment>
{
    // Add custom methods for Appointment here if needed
    Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        int staffId,
        DateOnly date
    );
}

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(SupabaseContext context)
        : base(context) { }

    public async Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        int staffId,
        DateOnly date
    )
    {
        return await _context
            .Appointments.Where(a => a.StaffId == staffId && a.AppointmentDate == date)
            .Join(
                _context.Users1,
                a => a.PatientId,
                u => u.UserId,
                (a, u) => new { Appointment = a, User = u }
            )
            .Join(
                _context.MedicalRecords,
                au => au.Appointment.RecordId,
                mr => mr.RecordId,
                (au, mr) =>
                    new
                    {
                        au.Appointment,
                        au.User,
                        MedicalRecord = mr,
                    }
            )
            .GroupJoin(
                _context.Diagnoses,
                aum => aum.Appointment.AppointmentId,
                d => d.AppointmentId,
                (aum, diagnoses) =>
                    new
                    {
                        aum.Appointment,
                        aum.User,
                        aum.MedicalRecord,
                        Diagnoses = diagnoses,
                    }
            )
            .SelectMany(
                aum => aum.Diagnoses.DefaultIfEmpty(),
                (aum, d) =>
                    new AppointmentMyScheduleDto
                    {
                        AppointmentId = aum.Appointment.AppointmentId,
                        PatientId = aum.Appointment.PatientId,
                        PatientName = aum.User.FullName,
                        AppointmentDate = aum.Appointment.AppointmentDate,
                        AppointmentTime = aum.Appointment.AppointmentTime,
                        Status = aum.Appointment.Status,
                        RecordId = aum.MedicalRecord.RecordId,
                        Notes = d != null ? d.Notes : null, // Lấy notes từ Diagnoses, null nếu không có
                    }
            )
            .OrderBy(dto => dto.AppointmentTime)
            .AsNoTracking()
            .ToListAsync();
    }
}

public class AppointmentMyScheduleDto
{
    public int AppointmentId { get; set; }
    public int? PatientId { get; set; }
    public string PatientName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string Status { get; set; }
    public int RecordId { get; set; }
    public string? Notes { get; set; }
}
