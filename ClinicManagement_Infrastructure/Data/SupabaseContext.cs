using System;
using System.Collections.Generic;
using ClinicManagement_Infrastructure.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_Infrastructure.Infrastructure.Data;

public partial class SupabaseContext : DbContext
{
    public SupabaseContext() { }

    public SupabaseContext(DbContextOptions<SupabaseContext> options)
        : base(options) { }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AuditLogEntry> AuditLogEntries { get; set; }

    public virtual DbSet<Bucket> Buckets { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<FlowState> FlowStates { get; set; }

    public virtual DbSet<Identity> Identities { get; set; }

    public virtual DbSet<ImportBill> ImportBills { get; set; }

    public virtual DbSet<ImportDetail> ImportDetails { get; set; }

    public virtual DbSet<Instance> Instances { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<MedicalStaff> MedicalStaffs { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<MfaAmrClaim> MfaAmrClaims { get; set; }

    public virtual DbSet<MfaChallenge> MfaChallenges { get; set; }

    public virtual DbSet<MfaFactor> MfaFactors { get; set; }

    public virtual DbSet<Migration> Migrations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OauthClient> OauthClients { get; set; }

    public virtual DbSet<OneTimeToken> OneTimeTokens { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientHistory> PatientHistories { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    public virtual DbSet<Queue> Queues { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<S3MultipartUpload> S3MultipartUploads { get; set; }

    public virtual DbSet<S3MultipartUploadsPart> S3MultipartUploadsParts { get; set; }

    public virtual DbSet<SamlProvider> SamlProviders { get; set; }

    public virtual DbSet<SamlRelayState> SamlRelayStates { get; set; }

    public virtual DbSet<SchemaMigration> SchemaMigrations { get; set; }

    public virtual DbSet<SchemaMigration1> SchemaMigrations1 { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<SsoDomain> SsoDomains { get; set; }

    public virtual DbSet<SsoProvider> SsoProviders { get; set; }

    public virtual DbSet<StaffSchedule> StaffSchedules { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<User1> Users1 { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql("Name=SupabaseConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
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

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_log_entries_pkey");

            entity.ToTable(
                "audit_log_entries",
                "auth",
                tb => tb.HasComment("Auth: Audit trail for user actions.")
            );

            entity.HasIndex(e => e.InstanceId, "audit_logs_instance_id_idx");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity
                .Property(e => e.IpAddress)
                .HasMaxLength(64)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("ip_address");
            entity.Property(e => e.Payload).HasColumnType("json").HasColumnName("payload");
        });

        modelBuilder.Entity<Bucket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("buckets_pkey");

            entity.ToTable("buckets", "storage");

            entity.HasIndex(e => e.Name, "bname").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AllowedMimeTypes).HasColumnName("allowed_mime_types");
            entity
                .Property(e => e.AvifAutodetection)
                .HasDefaultValue(false)
                .HasColumnName("avif_autodetection");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FileSizeLimit).HasColumnName("file_size_limit");
            entity.Property(e => e.Name).HasColumnName("name");
            entity
                .Property(e => e.Owner)
                .HasComment("Field is deprecated, use owner_id instead")
                .HasColumnName("owner");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Public).HasDefaultValue(false).HasColumnName("public");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
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

        modelBuilder.Entity<FlowState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("flow_state_pkey");

            entity.ToTable(
                "flow_state",
                "auth",
                tb => tb.HasComment("stores metadata for pkce logins")
            );

            entity.HasIndex(e => e.CreatedAt, "flow_state_created_at_idx").IsDescending();

            entity.HasIndex(e => e.AuthCode, "idx_auth_code");

            entity.HasIndex(
                e => new { e.UserId, e.AuthenticationMethod },
                "idx_user_id_auth_method"
            );

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.AuthCode).HasColumnName("auth_code");
            entity.Property(e => e.AuthCodeIssuedAt).HasColumnName("auth_code_issued_at");
            entity.Property(e => e.AuthenticationMethod).HasColumnName("authentication_method");
            entity.Property(e => e.CodeChallenge).HasColumnName("code_challenge");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ProviderAccessToken).HasColumnName("provider_access_token");
            entity.Property(e => e.ProviderRefreshToken).HasColumnName("provider_refresh_token");
            entity.Property(e => e.ProviderType).HasColumnName("provider_type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Identity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("identities_pkey");

            entity.ToTable(
                "identities",
                "auth",
                tb => tb.HasComment("Auth: Stores identities associated to a user.")
            );

            entity
                .HasIndex(e => e.Email, "identities_email_idx")
                .HasOperators(new[] { "text_pattern_ops" });

            entity
                .HasIndex(
                    e => new { e.ProviderId, e.Provider },
                    "identities_provider_id_provider_unique"
                )
                .IsUnique();

            entity.HasIndex(e => e.UserId, "identities_user_id_idx");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity
                .Property(e => e.Email)
                .HasComputedColumnSql("lower((identity_data ->> 'email'::text))", true)
                .HasComment(
                    "Auth: Email is a generated column that references the optional email property in the identity_data"
                )
                .HasColumnName("email");
            entity
                .Property(e => e.IdentityData)
                .HasColumnType("jsonb")
                .HasColumnName("identity_data");
            entity.Property(e => e.LastSignInAt).HasColumnName("last_sign_in_at");
            entity.Property(e => e.Provider).HasColumnName("provider");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Identities)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("identities_user_id_fkey");
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

        modelBuilder.Entity<Instance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("instances_pkey");

            entity.ToTable(
                "instances",
                "auth",
                tb => tb.HasComment("Auth: Manages users across multiple sites.")
            );

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.RawBaseConfig).HasColumnName("raw_base_config");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Uuid).HasColumnName("uuid");
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

        modelBuilder.Entity<MfaAmrClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("amr_id_pk");

            entity.ToTable(
                "mfa_amr_claims",
                "auth",
                tb =>
                    tb.HasComment(
                        "auth: stores authenticator method reference claims for multi factor authentication"
                    )
            );

            entity
                .HasIndex(
                    e => new { e.SessionId, e.AuthenticationMethod },
                    "mfa_amr_claims_session_id_authentication_method_pkey"
                )
                .IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.AuthenticationMethod).HasColumnName("authentication_method");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasOne(d => d.Session)
                .WithMany(p => p.MfaAmrClaims)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("mfa_amr_claims_session_id_fkey");
        });

        modelBuilder.Entity<MfaChallenge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mfa_challenges_pkey");

            entity.ToTable(
                "mfa_challenges",
                "auth",
                tb => tb.HasComment("auth: stores metadata about challenge requests made")
            );

            entity.HasIndex(e => e.CreatedAt, "mfa_challenge_created_at_idx").IsDescending();

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FactorId).HasColumnName("factor_id");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.OtpCode).HasColumnName("otp_code");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity
                .Property(e => e.WebAuthnSessionData)
                .HasColumnType("jsonb")
                .HasColumnName("web_authn_session_data");

            entity
                .HasOne(d => d.Factor)
                .WithMany(p => p.MfaChallenges)
                .HasForeignKey(d => d.FactorId)
                .HasConstraintName("mfa_challenges_auth_factor_id_fkey");
        });

        modelBuilder.Entity<MfaFactor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mfa_factors_pkey");

            entity.ToTable(
                "mfa_factors",
                "auth",
                tb => tb.HasComment("auth: stores metadata about factors")
            );

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "factor_id_created_at_idx");

            entity
                .HasIndex(e => e.LastChallengedAt, "mfa_factors_last_challenged_at_key")
                .IsUnique();

            entity
                .HasIndex(
                    e => new { e.FriendlyName, e.UserId },
                    "mfa_factors_user_friendly_name_unique"
                )
                .IsUnique()
                .HasFilter("(TRIM(BOTH FROM friendly_name) <> ''::text)");

            entity.HasIndex(e => e.UserId, "mfa_factors_user_id_idx");

            entity
                .HasIndex(e => new { e.UserId, e.Phone }, "unique_phone_factor_per_user")
                .IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FriendlyName).HasColumnName("friendly_name");
            entity.Property(e => e.LastChallengedAt).HasColumnName("last_challenged_at");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Secret).HasColumnName("secret");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WebAuthnAaguid).HasColumnName("web_authn_aaguid");
            entity
                .Property(e => e.WebAuthnCredential)
                .HasColumnType("jsonb")
                .HasColumnName("web_authn_credential");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.MfaFactors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("mfa_factors_user_id_fkey");
        });

        modelBuilder.Entity<Migration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("migrations_pkey");

            entity.ToTable("migrations", "storage");

            entity.HasIndex(e => e.Name, "migrations_name_key").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity
                .Property(e => e.ExecutedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("executed_at");
            entity.Property(e => e.Hash).HasMaxLength(40).HasColumnName("hash");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
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

        modelBuilder.Entity<OauthClient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("oauth_clients_pkey");

            entity.ToTable("oauth_clients", "auth");

            entity.HasIndex(e => e.ClientId, "oauth_clients_client_id_idx");

            entity.HasIndex(e => e.ClientId, "oauth_clients_client_id_key").IsUnique();

            entity.HasIndex(e => e.DeletedAt, "oauth_clients_deleted_at_idx");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientName).HasColumnName("client_name");
            entity.Property(e => e.ClientSecretHash).HasColumnName("client_secret_hash");
            entity.Property(e => e.ClientUri).HasColumnName("client_uri");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.GrantTypes).HasColumnName("grant_types");
            entity.Property(e => e.LogoUri).HasColumnName("logo_uri");
            entity.Property(e => e.RedirectUris).HasColumnName("redirect_uris");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<OneTimeToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("one_time_tokens_pkey");

            entity.ToTable("one_time_tokens", "auth");

            entity
                .HasIndex(e => e.RelatesTo, "one_time_tokens_relates_to_hash_idx")
                .HasMethod("hash");

            entity
                .HasIndex(e => e.TokenHash, "one_time_tokens_token_hash_hash_idx")
                .HasMethod("hash");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.RelatesTo).HasColumnName("relates_to");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.OneTimeTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("one_time_tokens_user_id_fkey");
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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

            entity.ToTable(
                "refresh_tokens",
                "auth",
                tb =>
                    tb.HasComment(
                        "Auth: Store of tokens used to refresh JWT tokens once they expire."
                    )
            );

            entity.HasIndex(e => e.InstanceId, "refresh_tokens_instance_id_idx");

            entity.HasIndex(
                e => new { e.InstanceId, e.UserId },
                "refresh_tokens_instance_id_user_id_idx"
            );

            entity.HasIndex(e => e.Parent, "refresh_tokens_parent_idx");

            entity.HasIndex(
                e => new { e.SessionId, e.Revoked },
                "refresh_tokens_session_id_revoked_idx"
            );

            entity.HasIndex(e => e.Token, "refresh_tokens_token_unique").IsUnique();

            entity.HasIndex(e => e.UpdatedAt, "refresh_tokens_updated_at_idx").IsDescending();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.Parent).HasMaxLength(255).HasColumnName("parent");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Token).HasMaxLength(255).HasColumnName("token");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasMaxLength(255).HasColumnName("user_id");

            entity
                .HasOne(d => d.Session)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("refresh_tokens_session_id_fkey");
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

        modelBuilder.Entity<S3MultipartUpload>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("s3_multipart_uploads_pkey");

            entity.ToTable("s3_multipart_uploads", "storage");

            entity
                .HasIndex(
                    e => new
                    {
                        e.BucketId,
                        e.Key,
                        e.CreatedAt,
                    },
                    "idx_multipart_uploads_list"
                )
                .UseCollation(new[] { null, "C", null });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BucketId).HasColumnName("bucket_id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity
                .Property(e => e.InProgressSize)
                .HasDefaultValue(0L)
                .HasColumnName("in_progress_size");
            entity.Property(e => e.Key).UseCollation("C").HasColumnName("key");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.UploadSignature).HasColumnName("upload_signature");
            entity
                .Property(e => e.UserMetadata)
                .HasColumnType("jsonb")
                .HasColumnName("user_metadata");
            entity.Property(e => e.Version).HasColumnName("version");

            entity
                .HasOne(d => d.Bucket)
                .WithMany(p => p.S3MultipartUploads)
                .HasForeignKey(d => d.BucketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("s3_multipart_uploads_bucket_id_fkey");
        });

        modelBuilder.Entity<S3MultipartUploadsPart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("s3_multipart_uploads_parts_pkey");

            entity.ToTable("s3_multipart_uploads_parts", "storage");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
            entity.Property(e => e.BucketId).HasColumnName("bucket_id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Etag).HasColumnName("etag");
            entity.Property(e => e.Key).UseCollation("C").HasColumnName("key");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.PartNumber).HasColumnName("part_number");
            entity.Property(e => e.Size).HasDefaultValue(0L).HasColumnName("size");
            entity.Property(e => e.UploadId).HasColumnName("upload_id");
            entity.Property(e => e.Version).HasColumnName("version");

            entity
                .HasOne(d => d.Bucket)
                .WithMany(p => p.S3MultipartUploadsParts)
                .HasForeignKey(d => d.BucketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("s3_multipart_uploads_parts_bucket_id_fkey");

            entity
                .HasOne(d => d.Upload)
                .WithMany(p => p.S3MultipartUploadsParts)
                .HasForeignKey(d => d.UploadId)
                .HasConstraintName("s3_multipart_uploads_parts_upload_id_fkey");
        });

        modelBuilder.Entity<SamlProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("saml_providers_pkey");

            entity.ToTable(
                "saml_providers",
                "auth",
                tb => tb.HasComment("Auth: Manages SAML Identity Provider connections.")
            );

            entity.HasIndex(e => e.EntityId, "saml_providers_entity_id_key").IsUnique();

            entity.HasIndex(e => e.SsoProviderId, "saml_providers_sso_provider_id_idx");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity
                .Property(e => e.AttributeMapping)
                .HasColumnType("jsonb")
                .HasColumnName("attribute_mapping");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.MetadataUrl).HasColumnName("metadata_url");
            entity.Property(e => e.MetadataXml).HasColumnName("metadata_xml");
            entity.Property(e => e.NameIdFormat).HasColumnName("name_id_format");
            entity.Property(e => e.SsoProviderId).HasColumnName("sso_provider_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasOne(d => d.SsoProvider)
                .WithMany(p => p.SamlProviders)
                .HasForeignKey(d => d.SsoProviderId)
                .HasConstraintName("saml_providers_sso_provider_id_fkey");
        });

        modelBuilder.Entity<SamlRelayState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("saml_relay_states_pkey");

            entity.ToTable(
                "saml_relay_states",
                "auth",
                tb =>
                    tb.HasComment(
                        "Auth: Contains SAML Relay State information for each Service Provider initiated login."
                    )
            );

            entity.HasIndex(e => e.CreatedAt, "saml_relay_states_created_at_idx").IsDescending();

            entity.HasIndex(e => e.ForEmail, "saml_relay_states_for_email_idx");

            entity.HasIndex(e => e.SsoProviderId, "saml_relay_states_sso_provider_id_idx");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FlowStateId).HasColumnName("flow_state_id");
            entity.Property(e => e.ForEmail).HasColumnName("for_email");
            entity.Property(e => e.RedirectTo).HasColumnName("redirect_to");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.SsoProviderId).HasColumnName("sso_provider_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasOne(d => d.FlowState)
                .WithMany(p => p.SamlRelayStates)
                .HasForeignKey(d => d.FlowStateId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("saml_relay_states_flow_state_id_fkey");

            entity
                .HasOne(d => d.SsoProvider)
                .WithMany(p => p.SamlRelayStates)
                .HasForeignKey(d => d.SsoProviderId)
                .HasConstraintName("saml_relay_states_sso_provider_id_fkey");
        });

        modelBuilder.Entity<SchemaMigration>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("schema_migrations_pkey");

            entity.ToTable(
                "schema_migrations",
                "auth",
                tb => tb.HasComment("Auth: Manages updates to the auth system.")
            );

            entity.Property(e => e.Version).HasMaxLength(255).HasColumnName("version");
        });

        modelBuilder.Entity<SchemaMigration1>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("schema_migrations_pkey");

            entity.ToTable("schema_migrations", "realtime");

            entity.Property(e => e.Version).ValueGeneratedNever().HasColumnName("version");
            entity
                .Property(e => e.InsertedAt)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("inserted_at");
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

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sessions_pkey");

            entity.ToTable(
                "sessions",
                "auth",
                tb => tb.HasComment("Auth: Stores session data associated to a user.")
            );

            entity.HasIndex(e => e.NotAfter, "sessions_not_after_idx").IsDescending();

            entity.HasIndex(e => e.UserId, "sessions_user_id_idx");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "user_id_created_at_idx");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FactorId).HasColumnName("factor_id");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity
                .Property(e => e.NotAfter)
                .HasComment(
                    "Auth: Not after is a nullable column that contains a timestamp after which the session should be regarded as expired."
                )
                .HasColumnName("not_after");
            entity
                .Property(e => e.RefreshedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refreshed_at");
            entity.Property(e => e.Tag).HasColumnName("tag");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("sessions_user_id_fkey");
        });

        modelBuilder.Entity<SsoDomain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sso_domains_pkey");

            entity.ToTable(
                "sso_domains",
                "auth",
                tb =>
                    tb.HasComment(
                        "Auth: Manages SSO email address domain mapping to an SSO Identity Provider."
                    )
            );

            entity.HasIndex(e => e.SsoProviderId, "sso_domains_sso_provider_id_idx");

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Domain).HasColumnName("domain");
            entity.Property(e => e.SsoProviderId).HasColumnName("sso_provider_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity
                .HasOne(d => d.SsoProvider)
                .WithMany(p => p.SsoDomains)
                .HasForeignKey(d => d.SsoProviderId)
                .HasConstraintName("sso_domains_sso_provider_id_fkey");
        });

        modelBuilder.Entity<SsoProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sso_providers_pkey");

            entity.ToTable(
                "sso_providers",
                "auth",
                tb =>
                    tb.HasComment(
                        "Auth: Manages SSO identity provider information; see saml_providers for SAML."
                    )
            );

            entity
                .HasIndex(e => e.ResourceId, "sso_providers_resource_id_pattern_idx")
                .HasOperators(new[] { "text_pattern_ops" });

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Disabled).HasColumnName("disabled");
            entity
                .Property(e => e.ResourceId)
                .HasComment(
                    "Auth: Uniquely identifies a SSO provider according to a user-chosen resource ID (case insensitive), useful in infrastructure as code."
                )
                .HasColumnName("resource_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<StaffSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("StaffSchedules_pkey");

            entity.Property(e => e.ScheduleId).UseIdentityAlwaysColumn();
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);

            entity
                .HasOne(d => d.Staff)
                .WithMany(p => p.StaffSchedules)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("StaffSchedules_StaffId_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_subscription");

            entity.ToTable("subscription", "realtime");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn().HasColumnName("id");
            entity.Property(e => e.Claims).HasColumnType("jsonb").HasColumnName("claims");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("timezone('utc'::text, now())")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
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
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable(
                "users",
                "auth",
                tb => tb.HasComment("Auth: Stores user login data within a secure schema.")
            );

            entity
                .HasIndex(e => e.ConfirmationToken, "confirmation_token_idx")
                .IsUnique()
                .HasFilter("((confirmation_token)::text !~ '^[0-9 ]*$'::text)");

            entity
                .HasIndex(e => e.EmailChangeTokenCurrent, "email_change_token_current_idx")
                .IsUnique()
                .HasFilter("((email_change_token_current)::text !~ '^[0-9 ]*$'::text)");

            entity
                .HasIndex(e => e.EmailChangeTokenNew, "email_change_token_new_idx")
                .IsUnique()
                .HasFilter("((email_change_token_new)::text !~ '^[0-9 ]*$'::text)");

            entity
                .HasIndex(e => e.ReauthenticationToken, "reauthentication_token_idx")
                .IsUnique()
                .HasFilter("((reauthentication_token)::text !~ '^[0-9 ]*$'::text)");

            entity
                .HasIndex(e => e.RecoveryToken, "recovery_token_idx")
                .IsUnique()
                .HasFilter("((recovery_token)::text !~ '^[0-9 ]*$'::text)");

            entity
                .HasIndex(e => e.Email, "users_email_partial_key")
                .IsUnique()
                .HasFilter("(is_sso_user = false)");

            entity.HasIndex(e => e.InstanceId, "users_instance_id_idx");

            entity.HasIndex(e => e.IsAnonymous, "users_is_anonymous_idx");

            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.Aud).HasMaxLength(255).HasColumnName("aud");
            entity.Property(e => e.BannedUntil).HasColumnName("banned_until");
            entity.Property(e => e.ConfirmationSentAt).HasColumnName("confirmation_sent_at");
            entity
                .Property(e => e.ConfirmationToken)
                .HasMaxLength(255)
                .HasColumnName("confirmation_token");
            entity
                .Property(e => e.ConfirmedAt)
                .HasComputedColumnSql("LEAST(email_confirmed_at, phone_confirmed_at)", true)
                .HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity.Property(e => e.EmailChange).HasMaxLength(255).HasColumnName("email_change");
            entity
                .Property(e => e.EmailChangeConfirmStatus)
                .HasDefaultValue((short)0)
                .HasColumnName("email_change_confirm_status");
            entity.Property(e => e.EmailChangeSentAt).HasColumnName("email_change_sent_at");
            entity
                .Property(e => e.EmailChangeTokenCurrent)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("email_change_token_current");
            entity
                .Property(e => e.EmailChangeTokenNew)
                .HasMaxLength(255)
                .HasColumnName("email_change_token_new");
            entity.Property(e => e.EmailConfirmedAt).HasColumnName("email_confirmed_at");
            entity
                .Property(e => e.EncryptedPassword)
                .HasMaxLength(255)
                .HasColumnName("encrypted_password");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.InvitedAt).HasColumnName("invited_at");
            entity
                .Property(e => e.IsAnonymous)
                .HasDefaultValue(false)
                .HasColumnName("is_anonymous");
            entity
                .Property(e => e.IsSsoUser)
                .HasDefaultValue(false)
                .HasComment(
                    "Auth: Set this column to true when the account comes from SSO. These accounts can have duplicate emails."
                )
                .HasColumnName("is_sso_user");
            entity.Property(e => e.IsSuperAdmin).HasColumnName("is_super_admin");
            entity.Property(e => e.LastSignInAt).HasColumnName("last_sign_in_at");
            entity
                .Property(e => e.Phone)
                .HasDefaultValueSql("NULL::character varying")
                .HasColumnName("phone");
            entity
                .Property(e => e.PhoneChange)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("phone_change");
            entity.Property(e => e.PhoneChangeSentAt).HasColumnName("phone_change_sent_at");
            entity
                .Property(e => e.PhoneChangeToken)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("phone_change_token");
            entity.Property(e => e.PhoneConfirmedAt).HasColumnName("phone_confirmed_at");
            entity
                .Property(e => e.RawAppMetaData)
                .HasColumnType("jsonb")
                .HasColumnName("raw_app_meta_data");
            entity
                .Property(e => e.RawUserMetaData)
                .HasColumnType("jsonb")
                .HasColumnName("raw_user_meta_data");
            entity
                .Property(e => e.ReauthenticationSentAt)
                .HasColumnName("reauthentication_sent_at");
            entity
                .Property(e => e.ReauthenticationToken)
                .HasMaxLength(255)
                .HasDefaultValueSql("''::character varying")
                .HasColumnName("reauthentication_token");
            entity.Property(e => e.RecoverySentAt).HasColumnName("recovery_sent_at");
            entity.Property(e => e.RecoveryToken).HasMaxLength(255).HasColumnName("recovery_token");
            entity.Property(e => e.Role).HasMaxLength(255).HasColumnName("role");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<User1>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.ToTable("Users");

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
        modelBuilder.HasSequence<int>("seq_schema_version", "graphql").IsCyclic();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
