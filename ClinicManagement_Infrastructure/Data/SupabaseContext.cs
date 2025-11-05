using System;
using System.Collections.Generic;
using ClinicManagement_Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_Infrastructure.Data;

public partial class SupabaseContext : DbContext
{
    public SupabaseContext(DbContextOptions<SupabaseContext> options)
        : base(options) { }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<ImportBill> ImportBills { get; set; }

    public virtual DbSet<ImportDetail> ImportDetails { get; set; }

    public virtual DbSet<ImportReportView> ImportReportViews { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<MedicalStaff> MedicalStaffs { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientHistory> PatientHistories { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<Queue> Queues { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }

    public virtual DbSet<StaffSchedule> StaffSchedules { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum(
                "auth",
                "oauth_authorization_status",
                new[] { "pending", "approved", "denied", "expired" }
            )
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum(
                "auth",
                "one_time_token_type",
                new[]
                {
                    "confirmation_token",
                    "reauthentication_token",
                    "recovery_token",
                    "email_change_token_new",
                    "email_change_token_current",
                    "phone_change_token",
                }
            )
            .HasPostgresEnum(
                "realtime",
                "action",
                new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" }
            )
            .HasPostgresEnum(
                "realtime",
                "equality_op",
                new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" }
            )
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("Appointments_pkey");

            entity.HasIndex(
                e => new { e.PatientId, e.StaffId },
                "IX_Appointments_PatientId_StaffId"
            );

            entity.Property(e => e.AppointmentId).UseIdentityAlwaysColumn();
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Notes).HasColumnType("character varying");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.AppointmentCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Appointments_CreatedBy_fkey");

            entity
                .HasOne(d => d.Patient)
                .WithMany(p => p.AppointmentPatients)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Appointments_PatientId_fkey");

            entity
                .HasOne(d => d.Record)
                .WithMany(p => p.Appointments)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Appointments_RecordId_fkey");

            entity
                .HasOne(d => d.Room)
                .WithMany(p => p.Appointments)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Appointments_RoomId_fkey");

            entity
                .HasOne(d => d.Schedule)
                .WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Appointments_ScheduleId_fkey");

            entity
                .HasOne(d => d.Staff)
                .WithMany(p => p.AppointmentStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Appointments_StaffId_fkey");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.DiagnosisId).HasName("Diagnoses_pkey");

            entity.Property(e => e.DiagnosisId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Diagnosis1).HasColumnName("Diagnosis");
            entity
                .Property(e => e.DiagnosisDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity
                .HasOne(d => d.Appointment)
                .WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Diagnoses_AppointmentId_fkey");

            entity
                .HasOne(d => d.Record)
                .WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Diagnoses_RecordId_fkey");

            entity
                .HasOne(d => d.Staff)
                .WithMany(p => p.Diagnoses)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Diagnoses_StaffId_fkey");
        });

        modelBuilder.Entity<ImportBill>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("ImportBills_pkey");

            entity.Property(e => e.ImportId).UseIdentityAlwaysColumn();
            entity
                .Property(e => e.ImportDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.ImportBills)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ImportBills_CreatedBy_fkey");

            entity
                .HasOne(d => d.Supplier)
                .WithMany(p => p.ImportBills)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ImportBills_SupplierId_fkey");
        });

        modelBuilder.Entity<ImportDetail>(entity =>
        {
            entity.HasKey(e => e.ImportDetailId).HasName("ImportDetails_pkey");

            entity.Property(e => e.ImportDetailId).UseIdentityAlwaysColumn();
            entity.Property(e => e.ImportPrice).HasPrecision(18, 2);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);

            entity
                .HasOne(d => d.Import)
                .WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ImportDetails_ImportId_fkey");

            entity
                .HasOne(d => d.Medicine)
                .WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ImportDetails_MedicineId_fkey");
        });

        modelBuilder.Entity<ImportReportView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ImportReportView")
                .HasAnnotation("Npgsql:StorageParameter:security_invoker", "on");

            entity.Property(e => e.ImportDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ImportPrice).HasPrecision(18, 2);
            entity.Property(e => e.MedicineName).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("Invoices_pkey");

            entity.Property(e => e.InvoiceId).UseIdentityAlwaysColumn();
            entity
                .Property(e => e.InvoiceDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity
                .HasOne(d => d.Appointment)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Invoices_AppointmentId_fkey");

            entity
                .HasOne(d => d.Patient)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Invoices_PatientId_fkey");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoiceDetailId).HasName("InvoiceDetails_pkey");

            entity.Property(e => e.InvoiceDetailId).UseIdentityAlwaysColumn();
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

            entity
                .HasOne(d => d.Invoice)
                .WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("InvoiceDetails_InvoiceId_fkey");

            entity
                .HasOne(d => d.Medicine)
                .WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("InvoiceDetails_MedicineId_fkey");

            entity
                .HasOne(d => d.Service)
                .WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("InvoiceDetails_ServiceId_fkey");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("MedicalRecords_pkey");

            entity.HasIndex(e => e.PatientId, "IX_MedicalRecords_PatientId");

            entity.HasIndex(e => e.RecordNumber, "MedicalRecords_RecordNumber_key").IsUnique();

            entity.Property(e => e.RecordId).UseIdentityAlwaysColumn();
            entity.Property(e => e.IssuedDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.RecordNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("MedicalRecords_CreatedBy_fkey");

            entity
                .HasOne(d => d.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("MedicalRecords_PatientId_fkey");
        });

        modelBuilder.Entity<MedicalStaff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("MedicalStaff_pkey");

            entity.ToTable("MedicalStaff");

            entity.Property(e => e.StaffId).ValueGeneratedNever();
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.Specialty).HasMaxLength(100);
            entity.Property(e => e.StaffType).HasMaxLength(20);

            entity
                .HasOne(d => d.Staff)
                .WithOne(p => p.MedicalStaff)
                .HasForeignKey<MedicalStaff>(d => d.StaffId)
                .HasConstraintName("MedicalStaff_StaffId_fkey");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(e => e.MedicineId).HasName("Medicines_pkey");

            entity.Property(e => e.MedicineId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MedicineName).HasMaxLength(100);
            entity.Property(e => e.MedicineType).HasMaxLength(50);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("Notifications_pkey");

            entity.Property(e => e.NotificationId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Message).HasMaxLength(500);
            entity
                .Property(e => e.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Type).HasMaxLength(20);

            entity
                .HasOne(d => d.Appointment)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Notifications_AppointmentId_fkey");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Notifications_UserId_fkey");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("Patients_pkey");

            entity.Property(e => e.PatientId).ValueGeneratedNever();

            entity
                .HasOne(d => d.PatientNavigation)
                .WithOne(p => p.Patient)
                .HasForeignKey<Patient>(d => d.PatientId)
                .HasConstraintName("Patients_PatientId_fkey");
        });

        modelBuilder.Entity<PatientHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("PatientHistory")
                .HasAnnotation("Npgsql:StorageParameter:security_invoker", "on");

            entity.Property(e => e.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("Prescriptions_pkey");

            entity.Property(e => e.PrescriptionId).UseIdentityAlwaysColumn();
            entity
                .Property(e => e.PrescriptionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity
                .HasOne(d => d.Appointment)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Prescriptions_AppointmentId_fkey");

            entity
                .HasOne(d => d.Record)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Prescriptions_RecordId_fkey");

            entity
                .HasOne(d => d.Staff)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Prescriptions_StaffId_fkey");
        });

        modelBuilder.Entity<PrescriptionDetail>(entity =>
        {
            entity.HasKey(e => e.PrescriptionDetailId).HasName("PrescriptionDetails_pkey");

            entity.Property(e => e.PrescriptionDetailId).UseIdentityAlwaysColumn();
            entity.Property(e => e.DosageInstruction).HasMaxLength(200);

            entity
                .HasOne(d => d.Medicine)
                .WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("PrescriptionDetails_MedicineId_fkey");

            entity
                .HasOne(d => d.Prescription)
                .WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("PrescriptionDetails_PrescriptionId_fkey");
        });

        modelBuilder.Entity<Queue>(entity =>
        {
            entity.HasKey(e => e.QueueId).HasName("Queues_pkey");

            entity.Property(e => e.QueueId).UseIdentityAlwaysColumn();
            entity.Property(e => e.QueueDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.QueueTime).HasDefaultValueSql("CURRENT_TIME");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity
                .HasOne(d => d.Appointment)
                .WithMany(p => p.Queues)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Queues_AppointmentId_fkey");

            entity
                .HasOne(d => d.CreatedByNavigation)
                .WithMany(p => p.Queues)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Queues_CreatedBy_fkey");

            entity
                .HasOne(d => d.Patient)
                .WithMany(p => p.Queues)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Queues_PatientId_fkey");

            entity
                .HasOne(d => d.Record)
                .WithMany(p => p.Queues)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Queues_RecordId_fkey");

            entity
                .HasOne(d => d.Room)
                .WithMany(p => p.Queues)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Queues_RoomId_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Roles_pkey");

            entity.HasIndex(e => e.RoleName, "Roles_RoleName_key").IsUnique();

            entity.Property(e => e.RoleId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("Rooms_pkey");

            entity.Property(e => e.RoomId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoomName).HasMaxLength(50);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("Services_pkey");

            entity.Property(e => e.ServiceId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.ServiceName).HasMaxLength(100);
            entity.Property(e => e.ServiceType).HasMaxLength(50);
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.ServiceOrderId).HasName("ServiceOrders_pkey");

            entity.Property(e => e.ServiceOrderId).UseIdentityAlwaysColumn();
            entity
                .Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity
                .HasOne(d => d.Appointment)
                .WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ServiceOrders_AppointmentId_fkey");

            entity
                .HasOne(d => d.AssignedStaff)
                .WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ServiceOrders_AssignedStaffId_fkey");

            entity
                .HasOne(d => d.Service)
                .WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ServiceOrders_ServiceId_fkey");
        });

        modelBuilder.Entity<StaffSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("StaffSchedules_pkey");

            entity.Property(e => e.ScheduleId).UseIdentityAlwaysColumn();
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity
                .HasOne(d => d.Room)
                .WithMany(p => p.StaffSchedules)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("StaffSchedules_RoomId_fkey");

            entity
                .HasOne(d => d.Staff)
                .WithMany(p => p.StaffSchedules)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("StaffSchedules_StaffId_fkey");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("Suppliers_pkey");

            entity.Property(e => e.SupplierId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(15);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.HasIndex(e => e.Username, "Users_Username_key").IsUnique();

            entity.Property(e => e.UserId).UseIdentityAlwaysColumn();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MustChangePassword).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("UserRoles_pkey");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "IX_UserRoles_UserId_RoleId");

            entity
                .Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity
                .HasOne(d => d.Role)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("UserRoles_RoleId_fkey");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserRoles_UserId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
