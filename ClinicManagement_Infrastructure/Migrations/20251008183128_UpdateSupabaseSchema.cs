using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicManagement_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.EnsureSchema(
                name: "storage");

            migrationBuilder.EnsureSchema(
                name: "realtime");

            migrationBuilder.EnsureSchema(
                name: "graphql");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:auth.aal_level", "aal1,aal2,aal3")
                .Annotation("Npgsql:Enum:auth.code_challenge_method", "s256,plain")
                .Annotation("Npgsql:Enum:auth.factor_status", "unverified,verified")
                .Annotation("Npgsql:Enum:auth.factor_type", "totp,webauthn,phone")
                .Annotation("Npgsql:Enum:auth.oauth_registration_type", "dynamic,manual")
                .Annotation("Npgsql:Enum:auth.one_time_token_type", "confirmation_token,reauthentication_token,recovery_token,email_change_token_new,email_change_token_current,phone_change_token")
                .Annotation("Npgsql:Enum:realtime.action", "INSERT,UPDATE,DELETE,TRUNCATE,ERROR")
                .Annotation("Npgsql:Enum:realtime.equality_op", "eq,neq,lt,lte,gt,gte,in")
                .Annotation("Npgsql:PostgresExtension:extensions.pg_stat_statements", ",,")
                .Annotation("Npgsql:PostgresExtension:extensions.pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:extensions.uuid-ossp", ",,")
                .Annotation("Npgsql:PostgresExtension:graphql.pg_graphql", ",,")
                .Annotation("Npgsql:PostgresExtension:vault.supabase_vault", ",,");

            migrationBuilder.CreateSequence<int>(
                name: "seq_schema_version",
                schema: "graphql",
                cyclic: true);

            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payload = table.Column<string>(type: "json", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_log_entries_pkey", x => x.id);
                },
                comment: "Auth: Audit trail for user actions.");

            migrationBuilder.CreateTable(
                name: "buckets",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    owner = table.Column<Guid>(type: "uuid", nullable: true, comment: "Field is deprecated, use owner_id instead"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    @public = table.Column<bool>(name: "public", type: "boolean", nullable: true, defaultValue: false),
                    avif_autodetection = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    file_size_limit = table.Column<long>(type: "bigint", nullable: true),
                    allowed_mime_types = table.Column<List<string>>(type: "text[]", nullable: true),
                    owner_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("buckets_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flow_state",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    auth_code = table.Column<string>(type: "text", nullable: false),
                    code_challenge = table.Column<string>(type: "text", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    provider_access_token = table.Column<string>(type: "text", nullable: true),
                    provider_refresh_token = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    authentication_method = table.Column<string>(type: "text", nullable: false),
                    auth_code_issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("flow_state_pkey", x => x.id);
                },
                comment: "stores metadata for pkce logins");

            migrationBuilder.CreateTable(
                name: "instances",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_base_config = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("instances_pkey", x => x.id);
                },
                comment: "Auth: Manages users across multiple sites.");

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    MedicineId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    MedicineName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MedicineType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Medicines_pkey", x => x.MedicineId);
                });

            migrationBuilder.CreateTable(
                name: "migrations",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    executed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("migrations_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oauth_clients",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: false),
                    client_secret_hash = table.Column<string>(type: "text", nullable: false),
                    redirect_uris = table.Column<string>(type: "text", nullable: false),
                    grant_types = table.Column<string>(type: "text", nullable: false),
                    client_name = table.Column<string>(type: "text", nullable: true),
                    client_uri = table.Column<string>(type: "text", nullable: true),
                    logo_uri = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("oauth_clients_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Roles_pkey", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    RoomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Rooms_pkey", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "schema_migrations",
                schema: "auth",
                columns: table => new
                {
                    version = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("schema_migrations_pkey", x => x.version);
                },
                comment: "Auth: Manages updates to the auth system.");

            migrationBuilder.CreateTable(
                name: "schema_migrations",
                schema: "realtime",
                columns: table => new
                {
                    version = table.Column<long>(type: "bigint", nullable: false),
                    inserted_at = table.Column<DateTime>(type: "timestamp(0) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("schema_migrations_pkey", x => x.version);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Services_pkey", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "sso_providers",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<string>(type: "text", nullable: true, comment: "Auth: Uniquely identifies a SSO provider according to a user-chosen resource ID (case insensitive), useful in infrastructure as code."),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    disabled = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sso_providers_pkey", x => x.id);
                },
                comment: "Auth: Manages SSO identity provider information; see saml_providers for SAML.");

            migrationBuilder.CreateTable(
                name: "subscription",
                schema: "realtime",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claims = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "timezone('utc'::text, now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    SupplierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Suppliers_pkey", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aud = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    encrypted_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmation_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    confirmation_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    recovery_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    recovery_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email_change_token_new = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_change = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email_change_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sign_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    raw_app_meta_data = table.Column<string>(type: "jsonb", nullable: true),
                    raw_user_meta_data = table.Column<string>(type: "jsonb", nullable: true),
                    is_super_admin = table.Column<bool>(type: "boolean", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true, defaultValueSql: "NULL::character varying"),
                    phone_confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    phone_change = table.Column<string>(type: "text", nullable: true, defaultValueSql: "''::character varying"),
                    phone_change_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "''::character varying"),
                    phone_change_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, computedColumnSql: "LEAST(email_confirmed_at, phone_confirmed_at)", stored: true),
                    email_change_token_current = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "''::character varying"),
                    email_change_confirm_status = table.Column<short>(type: "smallint", nullable: true, defaultValue: (short)0),
                    banned_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reauthentication_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, defaultValueSql: "''::character varying"),
                    reauthentication_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_sso_user = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Auth: Set this column to true when the account comes from SSO. These accounts can have duplicate emails."),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_anonymous = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                },
                comment: "Auth: Stores user login data within a secure schema.");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Users_pkey", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Object",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Owner = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    PathTokens = table.Column<List<string>>(type: "text[]", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true),
                    OwnerId = table.Column<string>(type: "text", nullable: true),
                    UserMetadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Object", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Object_buckets_BucketId",
                        column: x => x.BucketId,
                        principalSchema: "storage",
                        principalTable: "buckets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "s3_multipart_uploads",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    in_progress_size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    upload_signature = table.Column<string>(type: "text", nullable: false),
                    bucket_id = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false, collation: "C"),
                    version = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("s3_multipart_uploads_pkey", x => x.id);
                    table.ForeignKey(
                        name: "s3_multipart_uploads_bucket_id_fkey",
                        column: x => x.bucket_id,
                        principalSchema: "storage",
                        principalTable: "buckets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "saml_providers",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sso_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<string>(type: "text", nullable: false),
                    metadata_xml = table.Column<string>(type: "text", nullable: false),
                    metadata_url = table.Column<string>(type: "text", nullable: true),
                    attribute_mapping = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name_id_format = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("saml_providers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "saml_providers_sso_provider_id_fkey",
                        column: x => x.sso_provider_id,
                        principalSchema: "auth",
                        principalTable: "sso_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Manages SAML Identity Provider connections.");

            migrationBuilder.CreateTable(
                name: "saml_relay_states",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sso_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<string>(type: "text", nullable: false),
                    for_email = table.Column<string>(type: "text", nullable: true),
                    redirect_to = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    flow_state_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("saml_relay_states_pkey", x => x.id);
                    table.ForeignKey(
                        name: "saml_relay_states_flow_state_id_fkey",
                        column: x => x.flow_state_id,
                        principalSchema: "auth",
                        principalTable: "flow_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "saml_relay_states_sso_provider_id_fkey",
                        column: x => x.sso_provider_id,
                        principalSchema: "auth",
                        principalTable: "sso_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Contains SAML Relay State information for each Service Provider initiated login.");

            migrationBuilder.CreateTable(
                name: "sso_domains",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sso_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    domain = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sso_domains_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sso_domains_sso_provider_id_fkey",
                        column: x => x.sso_provider_id,
                        principalSchema: "auth",
                        principalTable: "sso_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Manages SSO email address domain mapping to an SSO Identity Provider.");

            migrationBuilder.CreateTable(
                name: "identities",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_data = table.Column<string>(type: "jsonb", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: false),
                    last_sign_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower((identity_data ->> 'email'::text))", stored: true, comment: "Auth: Email is a generated column that references the optional email property in the identity_data")
                },
                constraints: table =>
                {
                    table.PrimaryKey("identities_pkey", x => x.id);
                    table.ForeignKey(
                        name: "identities_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Stores identities associated to a user.");

            migrationBuilder.CreateTable(
                name: "mfa_factors",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    secret = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    last_challenged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    web_authn_credential = table.Column<string>(type: "jsonb", nullable: true),
                    web_authn_aaguid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mfa_factors_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mfa_factors_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "auth: stores metadata about factors");

            migrationBuilder.CreateTable(
                name: "one_time_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    relates_to = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("one_time_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "one_time_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    factor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    not_after = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Auth: Not after is a nullable column that contains a timestamp after which the session should be regarded as expired."),
                    refreshed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    tag = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sessions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "sessions_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Stores session data associated to a user.");

            migrationBuilder.CreateTable(
                name: "ImportBills",
                columns: table => new
                {
                    ImportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    ImportDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ImportBills_pkey", x => x.ImportId);
                    table.ForeignKey(
                        name: "ImportBills_CreatedBy_fkey",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ImportBills_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MedicalStaff",
                columns: table => new
                {
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    StaffType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Specialty = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MedicalStaff_pkey", x => x.StaffId);
                    table.ForeignKey(
                        name: "MedicalStaff_StaffId_fkey",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    MedicalHistory = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Patients_pkey", x => x.PatientId);
                    table.ForeignKey(
                        name: "Patients_PatientId_fkey",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("StaffSchedules_pkey", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "StaffSchedules_StaffId_fkey",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserRoles_pkey", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "UserRoles_RoleId_fkey",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "UserRoles_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "s3_multipart_uploads_parts",
                schema: "storage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    upload_id = table.Column<string>(type: "text", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    part_number = table.Column<int>(type: "integer", nullable: false),
                    bucket_id = table.Column<string>(type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false, collation: "C"),
                    etag = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("s3_multipart_uploads_parts_pkey", x => x.id);
                    table.ForeignKey(
                        name: "s3_multipart_uploads_parts_bucket_id_fkey",
                        column: x => x.bucket_id,
                        principalSchema: "storage",
                        principalTable: "buckets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "s3_multipart_uploads_parts_upload_id_fkey",
                        column: x => x.upload_id,
                        principalSchema: "storage",
                        principalTable: "s3_multipart_uploads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mfa_challenges",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: false),
                    otp_code = table.Column<string>(type: "text", nullable: true),
                    web_authn_session_data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mfa_challenges_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mfa_challenges_auth_factor_id_fkey",
                        column: x => x.factor_id,
                        principalSchema: "auth",
                        principalTable: "mfa_factors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "auth: stores metadata about challenge requests made");

            migrationBuilder.CreateTable(
                name: "mfa_amr_claims",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    authentication_method = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("amr_id_pk", x => x.id);
                    table.ForeignKey(
                        name: "mfa_amr_claims_session_id_fkey",
                        column: x => x.session_id,
                        principalSchema: "auth",
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "auth: stores authenticator method reference claims for multi factor authentication");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    revoked = table.Column<bool>(type: "boolean", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    parent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "refresh_tokens_session_id_fkey",
                        column: x => x.session_id,
                        principalSchema: "auth",
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth: Store of tokens used to refresh JWT tokens once they expire.");

            migrationBuilder.CreateTable(
                name: "ImportDetails",
                columns: table => new
                {
                    ImportDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ImportId = table.Column<int>(type: "integer", nullable: true),
                    MedicineId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ImportPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ImportDetails_pkey", x => x.ImportDetailId);
                    table.ForeignKey(
                        name: "ImportDetails_ImportId_fkey",
                        column: x => x.ImportId,
                        principalTable: "ImportBills",
                        principalColumn: "ImportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ImportDetails_MedicineId_fkey",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: true),
                    RecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MedicalRecords_pkey", x => x.RecordId);
                    table.ForeignKey(
                        name: "MedicalRecords_CreatedBy_fkey",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "MedicalRecords_PatientId_fkey",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: true),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    ScheduleId = table.Column<int>(type: "integer", nullable: true),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    AppointmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AppointmentTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Appointments_pkey", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "Appointments_CreatedBy_fkey",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "Appointments_PatientId_fkey",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Appointments_RecordId_fkey",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Appointments_ScheduleId_fkey",
                        column: x => x.ScheduleId,
                        principalTable: "StaffSchedules",
                        principalColumn: "ScheduleId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "Appointments_StaffId_fkey",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Diagnoses",
                columns: table => new
                {
                    DiagnosisId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    Symptoms = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DiagnosisDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("Diagnoses_pkey", x => x.DiagnosisId);
                    table.ForeignKey(
                        name: "Diagnoses_AppointmentId_fkey",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Diagnoses_RecordId_fkey",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Diagnoses_StaffId_fkey",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    PatientId = table.Column<int>(type: "integer", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Invoices_pkey", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "Invoices_AppointmentId_fkey",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Invoices_PatientId_fkey",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Notifications_pkey", x => x.NotificationId);
                    table.ForeignKey(
                        name: "Notifications_AppointmentId_fkey",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Notifications_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    PrescriptionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Instructions = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Prescriptions_pkey", x => x.PrescriptionId);
                    table.ForeignKey(
                        name: "Prescriptions_AppointmentId_fkey",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Prescriptions_RecordId_fkey",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Prescriptions_StaffId_fkey",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Queues",
                columns: table => new
                {
                    QueueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: true),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    QueueNumber = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: true),
                    QueueDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    QueueTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false, defaultValueSql: "CURRENT_TIME"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Queues_pkey", x => x.QueueId);
                    table.ForeignKey(
                        name: "Queues_AppointmentId_fkey",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Queues_CreatedBy_fkey",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "Queues_PatientId_fkey",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Queues_RecordId_fkey",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Queues_RoomId_fkey",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOrders",
                columns: table => new
                {
                    ServiceOrderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    ServiceId = table.Column<int>(type: "integer", nullable: true),
                    AssignedStaffId = table.Column<int>(type: "integer", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ServiceOrders_pkey", x => x.ServiceOrderId);
                    table.ForeignKey(
                        name: "ServiceOrders_AppointmentId_fkey",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ServiceOrders_AssignedStaffId_fkey",
                        column: x => x.AssignedStaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ServiceOrders_ServiceId_fkey",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    InvoiceDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true),
                    ServiceId = table.Column<int>(type: "integer", nullable: true),
                    MedicineId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("InvoiceDetails_pkey", x => x.InvoiceDetailId);
                    table.ForeignKey(
                        name: "InvoiceDetails_InvoiceId_fkey",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "InvoiceDetails_MedicineId_fkey",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "InvoiceDetails_ServiceId_fkey",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionDetails",
                columns: table => new
                {
                    PrescriptionDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: true),
                    MedicineId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DosageInstruction = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrescriptionDetails_pkey", x => x.PrescriptionDetailId);
                    table.ForeignKey(
                        name: "PrescriptionDetails_MedicineId_fkey",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "MedicineId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "PrescriptionDetails_PrescriptionId_fkey",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedBy",
                table: "Appointments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_StaffId",
                table: "Appointments",
                columns: new[] { "PatientId", "StaffId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_RecordId",
                table: "Appointments",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ScheduleId",
                table: "Appointments",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffId",
                table: "Appointments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "audit_logs_instance_id_idx",
                schema: "auth",
                table: "audit_log_entries",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "bname",
                schema: "storage",
                table: "buckets",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_AppointmentId",
                table: "Diagnoses",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_RecordId",
                table: "Diagnoses",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_StaffId",
                table: "Diagnoses",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "flow_state_created_at_idx",
                schema: "auth",
                table: "flow_state",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_auth_code",
                schema: "auth",
                table: "flow_state",
                column: "auth_code");

            migrationBuilder.CreateIndex(
                name: "idx_user_id_auth_method",
                schema: "auth",
                table: "flow_state",
                columns: new[] { "user_id", "authentication_method" });

            migrationBuilder.CreateIndex(
                name: "identities_email_idx",
                schema: "auth",
                table: "identities",
                column: "email")
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "identities_provider_id_provider_unique",
                schema: "auth",
                table: "identities",
                columns: new[] { "provider_id", "provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "identities_user_id_idx",
                schema: "auth",
                table: "identities",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBills_CreatedBy",
                table: "ImportBills",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBills_SupplierId",
                table: "ImportBills",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportDetails_ImportId",
                table: "ImportDetails",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportDetails_MedicineId",
                table: "ImportDetails",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_InvoiceId",
                table: "InvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_MedicineId",
                table: "InvoiceDetails",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_ServiceId",
                table: "InvoiceDetails",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId",
                table: "Invoices",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_CreatedBy",
                table: "MedicalRecords",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "MedicalRecords_RecordNumber_key",
                table: "MedicalRecords",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "mfa_amr_claims_session_id_authentication_method_pkey",
                schema: "auth",
                table: "mfa_amr_claims",
                columns: new[] { "session_id", "authentication_method" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mfa_challenges_factor_id",
                schema: "auth",
                table: "mfa_challenges",
                column: "factor_id");

            migrationBuilder.CreateIndex(
                name: "mfa_challenge_created_at_idx",
                schema: "auth",
                table: "mfa_challenges",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "factor_id_created_at_idx",
                schema: "auth",
                table: "mfa_factors",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "mfa_factors_last_challenged_at_key",
                schema: "auth",
                table: "mfa_factors",
                column: "last_challenged_at",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "mfa_factors_user_friendly_name_unique",
                schema: "auth",
                table: "mfa_factors",
                columns: new[] { "friendly_name", "user_id" },
                unique: true,
                filter: "(TRIM(BOTH FROM friendly_name) <> ''::text)");

            migrationBuilder.CreateIndex(
                name: "mfa_factors_user_id_idx",
                schema: "auth",
                table: "mfa_factors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "unique_phone_factor_per_user",
                schema: "auth",
                table: "mfa_factors",
                columns: new[] { "user_id", "phone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "migrations_name_key",
                schema: "storage",
                table: "migrations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentId",
                table: "Notifications",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "oauth_clients_client_id_idx",
                schema: "auth",
                table: "oauth_clients",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "oauth_clients_client_id_key",
                schema: "auth",
                table: "oauth_clients",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "oauth_clients_deleted_at_idx",
                schema: "auth",
                table: "oauth_clients",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_Object_BucketId",
                table: "Object",
                column: "BucketId");

            migrationBuilder.CreateIndex(
                name: "IX_one_time_tokens_user_id",
                schema: "auth",
                table: "one_time_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "one_time_tokens_relates_to_hash_idx",
                schema: "auth",
                table: "one_time_tokens",
                column: "relates_to")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "one_time_tokens_token_hash_hash_idx",
                schema: "auth",
                table: "one_time_tokens",
                column: "token_hash")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionDetails_MedicineId",
                table: "PrescriptionDetails",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionDetails_PrescriptionId",
                table: "PrescriptionDetails",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_RecordId",
                table: "Prescriptions",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_StaffId",
                table: "Prescriptions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_AppointmentId",
                table: "Queues",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_CreatedBy",
                table: "Queues",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_PatientId",
                table: "Queues",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_RecordId",
                table: "Queues",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Queues_RoomId",
                table: "Queues",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_instance_id_idx",
                schema: "auth",
                table: "refresh_tokens",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_instance_id_user_id_idx",
                schema: "auth",
                table: "refresh_tokens",
                columns: new[] { "instance_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_parent_idx",
                schema: "auth",
                table: "refresh_tokens",
                column: "parent");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_session_id_revoked_idx",
                schema: "auth",
                table: "refresh_tokens",
                columns: new[] { "session_id", "revoked" });

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_token_unique",
                schema: "auth",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_updated_at_idx",
                schema: "auth",
                table: "refresh_tokens",
                column: "updated_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "Roles_RoleName_key",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_multipart_uploads_list",
                schema: "storage",
                table: "s3_multipart_uploads",
                columns: new[] { "bucket_id", "key", "created_at" })
                .Annotation("Relational:Collation", new[] { null, "C", null });

            migrationBuilder.CreateIndex(
                name: "IX_s3_multipart_uploads_parts_bucket_id",
                schema: "storage",
                table: "s3_multipart_uploads_parts",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "IX_s3_multipart_uploads_parts_upload_id",
                schema: "storage",
                table: "s3_multipart_uploads_parts",
                column: "upload_id");

            migrationBuilder.CreateIndex(
                name: "saml_providers_entity_id_key",
                schema: "auth",
                table: "saml_providers",
                column: "entity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "saml_providers_sso_provider_id_idx",
                schema: "auth",
                table: "saml_providers",
                column: "sso_provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_saml_relay_states_flow_state_id",
                schema: "auth",
                table: "saml_relay_states",
                column: "flow_state_id");

            migrationBuilder.CreateIndex(
                name: "saml_relay_states_created_at_idx",
                schema: "auth",
                table: "saml_relay_states",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "saml_relay_states_for_email_idx",
                schema: "auth",
                table: "saml_relay_states",
                column: "for_email");

            migrationBuilder.CreateIndex(
                name: "saml_relay_states_sso_provider_id_idx",
                schema: "auth",
                table: "saml_relay_states",
                column: "sso_provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_AppointmentId",
                table: "ServiceOrders",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_AssignedStaffId",
                table: "ServiceOrders",
                column: "AssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_ServiceId",
                table: "ServiceOrders",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "sessions_not_after_idx",
                schema: "auth",
                table: "sessions",
                column: "not_after",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "sessions_user_id_idx",
                schema: "auth",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_id_created_at_idx",
                schema: "auth",
                table: "sessions",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "sso_domains_sso_provider_id_idx",
                schema: "auth",
                table: "sso_domains",
                column: "sso_provider_id");

            migrationBuilder.CreateIndex(
                name: "sso_providers_resource_id_pattern_idx",
                schema: "auth",
                table: "sso_providers",
                column: "resource_id")
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffSchedules_StaffId",
                table: "StaffSchedules",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "confirmation_token_idx",
                schema: "auth",
                table: "users",
                column: "confirmation_token",
                unique: true,
                filter: "((confirmation_token)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "email_change_token_current_idx",
                schema: "auth",
                table: "users",
                column: "email_change_token_current",
                unique: true,
                filter: "((email_change_token_current)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "email_change_token_new_idx",
                schema: "auth",
                table: "users",
                column: "email_change_token_new",
                unique: true,
                filter: "((email_change_token_new)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "reauthentication_token_idx",
                schema: "auth",
                table: "users",
                column: "reauthentication_token",
                unique: true,
                filter: "((reauthentication_token)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "recovery_token_idx",
                schema: "auth",
                table: "users",
                column: "recovery_token",
                unique: true,
                filter: "((recovery_token)::text !~ '^[0-9 ]*$'::text)");

            migrationBuilder.CreateIndex(
                name: "users_email_partial_key",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true,
                filter: "(is_sso_user = false)");

            migrationBuilder.CreateIndex(
                name: "users_instance_id_idx",
                schema: "auth",
                table: "users",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "users_is_anonymous_idx",
                schema: "auth",
                table: "users",
                column: "is_anonymous");

            migrationBuilder.CreateIndex(
                name: "users_phone_key",
                schema: "auth",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Users_Email_key",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Users_Username_key",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_entries",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Diagnoses");

            migrationBuilder.DropTable(
                name: "identities",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ImportDetails");

            migrationBuilder.DropTable(
                name: "instances",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "MedicalStaff");

            migrationBuilder.DropTable(
                name: "mfa_amr_claims",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "mfa_challenges",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "migrations",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "oauth_clients",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Object");

            migrationBuilder.DropTable(
                name: "one_time_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "PrescriptionDetails");

            migrationBuilder.DropTable(
                name: "Queues");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "s3_multipart_uploads_parts",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "saml_providers",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "saml_relay_states",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "schema_migrations",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "schema_migrations",
                schema: "realtime");

            migrationBuilder.DropTable(
                name: "ServiceOrders");

            migrationBuilder.DropTable(
                name: "sso_domains",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "subscription",
                schema: "realtime");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ImportBills");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "mfa_factors",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "s3_multipart_uploads",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "flow_state",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "sso_providers",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "buckets",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "StaffSchedules");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropSequence(
                name: "seq_schema_version",
                schema: "graphql");
        }
    }
}
