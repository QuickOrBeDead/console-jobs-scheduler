using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qrtz_job_run_email",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_run_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    message_to = table.Column<string>(type: "text", nullable: false),
                    message_cc = table.Column<string>(type: "text", nullable: false),
                    message_bcc = table.Column<string>(type: "text", nullable: false),
                    is_sent = table.Column<bool>(type: "boolean", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qrtz_job_run_email", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qrtz_job_run_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_run_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    is_error = table.Column<bool>(type: "boolean", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qrtz_job_run_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qrtz_packages",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    author = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    file_name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    arguments = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qrtz_packages", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "qrtz_job_run_attachment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    job_run_id = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FileContent = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    create_time = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qrtz_job_run_attachment", x => x.id);
                    table.ForeignKey(
                        name: "FK_qrtz_job_run_attachment_qrtz_job_run_email_email_id",
                        column: x => x.email_id,
                        principalTable: "qrtz_job_run_email",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_qrtz_job_run_attachment_job_run_id",
                table: "qrtz_job_run_attachment",
                column: "job_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_qrtz_job_run_attachment_email_id",
                table: "qrtz_job_run_attachment",
                column: "email_id");

            migrationBuilder.CreateIndex(
                name: "idx_qrtz_job_run_email_job_run_id",
                table: "qrtz_job_run_email",
                column: "job_run_id");

            migrationBuilder.CreateIndex(
                name: "idx_qrtz_job_run_log_job_run_id",
                table: "qrtz_job_run_log",
                column: "job_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qrtz_job_run_attachment");

            migrationBuilder.DropTable(
                name: "qrtz_job_run_log");

            migrationBuilder.DropTable(
                name: "qrtz_packages");

            migrationBuilder.DropTable(
                name: "qrtz_job_run_email");
        }
    }
}
