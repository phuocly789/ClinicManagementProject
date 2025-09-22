public interface IAppointmentService
{
    Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        int staffId,
        DateOnly? date = null
    );
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<List<AppointmentMyScheduleDto>> GetAppointmentsByStaffIdAnddDateAsync(
        int staffId,
        DateOnly? date = null
    )
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Now);

        return await _appointmentRepository.GetAppointmentsByStaffIdAnddDateAsync(
            staffId,
            selectedDate
        );
    }
}
