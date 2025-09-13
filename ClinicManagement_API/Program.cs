using ClinicManagement_Infrastructure.Infrastructure.Data;
<<<<<<< HEAD
=======
using ClinicManagement_Infrastructure.Repositories;
>>>>>>> phuoc
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// Đăng ký DbContext
builder.Services.AddDbContext<SupabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// DI Repository
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDiagnosisRepository, DiagnosisRepository >();
builder.Services.AddScoped<IImportBillRepository, ImportBillRepository>();
builder.Services.AddScoped<IImportDetailRepository, ImportDetailRepository>();
builder.Services.AddScoped<IInvoiceDetailRepository, InvoiceDetailRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IMedicalStaffRepository, MedicalStaffRepository>();
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPrescriptionDetailRepository, PrescriptionDetailRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IQueueRepository, QueueRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IStaffScheduleRepository, StaffScheduleRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

//Sử dụng map controller
builder.Services.AddControllers();
//Swagger cấu hình có điền Authentication
=======
// Đăng ký DbContext với connection string từ appsettings.json
builder.Services.AddDbContext<SupabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection"))
);

// Đăng ký repository services
builder.Services.AddRepositoryServices();

// Đăng ký các dịch vụ khác (nếu có)
builder.Services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
>>>>>>> phuoc
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
<<<<<<< HEAD
=======

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

>>>>>>> phuoc
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


//use middle ware controller
app.MapControllers();
//use swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
