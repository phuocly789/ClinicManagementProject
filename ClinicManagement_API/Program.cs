using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using ClinicManagement_Infrastructure.Data;
using ClinicManagement_Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với connection string từ appsettings.json
builder.Services.AddDbContext<SupabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection"))
);

// Đăng ký repository services
builder.Services.AddRepositoryServices();

// Đăng ký các dịch vụ khác (nếu có)
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPatinetService, PatinetService>();
builder.Services.AddScoped<IReceptionistService, ReceptionistService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<ISuplierService, SuplierService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

// Thêm dịch vụ controller
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // THÊM DÒNG NÀY VÀO
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//jwt
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // 🔥 Thêm hỗ trợ Authorization header tất cả api
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập token vào ô bên dưới theo định dạng: Bearer {token}",
        }
    );

    // 🔥 Định nghĩa yêu cầu sử dụng Authorization trên từng api
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

//add service cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        builder =>
            builder
                .WithOrigins(
                    "http://localhost:5098",
                    "https://localhost:5001",
                    "https://localhost:5066"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
    );
});

//cài đặt jwt
//Service jwt
//Thêm middleware authentication
var privateKey = builder.Configuration["jwt:Serect-Key"];
var Issuer = builder.Configuration["jwt:Issuer"];
var Audience = builder.Configuration["jwt:Audience"];

// Thêm dịch vụ Authentication vào ứng dụng, sử dụng JWT Bearer làm phương thức xác thực
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Thiết lập các tham số xác thực token
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            // Kiểm tra và xác nhận Issuer (nguồn phát hành token)
            ValidateIssuer = true,
            ValidIssuer = Issuer, // Biến `Issuer` chứa giá trị của Issuer hợp lệ
            // Kiểm tra và xác nhận Audience (đối tượng nhận token)
            ValidateAudience = true,
            ValidAudience = Audience, // Biến `Audience` chứa giá trị của Audience hợp lệ
            // Kiểm tra và xác nhận khóa bí mật được sử dụng để ký token
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)),
            // Sử dụng khóa bí mật (`privateKey`) để tạo SymmetricSecurityKey nhằm xác thực chữ ký của token
            // Giảm độ trễ (skew time) của token xuống 0, đảm bảo token hết hạn chính xác
            ClockSkew = TimeSpan.Zero,
            // Xác định claim chứa vai trò của user (để phân quyền)
            RoleClaimType = ClaimTypes.Role,
            // Xác định claim chứa tên của user
            NameClaimType = ClaimTypes.Name,
            // Kiểm tra thời gian hết hạn của token, không cho phép sử dụng token hết hạn
            ValidateLifetime = true,
        };
    });

// Thêm dịch vụ Authorization để hỗ trợ phân quyền người dùng
builder.Services.AddAuthorization();

//add jwt service
builder.Services.AddScoped<JwtAuthService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//use middle ware controller
app.MapControllers();

//use swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
