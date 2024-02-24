using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleJobScheduler.Core.Domain.History.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qrtz_job_history",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    sched_name = table.Column<string>(type: "text", nullable: false),
                    instance_name = table.Column<string>(type: "text", nullable: false),
                    package_name = table.Column<string>(type: "text", nullable: false),
                    job_name = table.Column<string>(type: "text", nullable: false),
                    job_group = table.Column<string>(type: "text", nullable: false),
                    trigger_name = table.Column<string>(type: "text", nullable: false),
                    trigger_group = table.Column<string>(type: "text", nullable: false),
                    sched_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fired_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_signal_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    run_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    has_error = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    error_details = table.Column<string>(type: "text", nullable: true),
                    completed = table.Column<bool>(type: "boolean", nullable: false),
                    vetoed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_qrtz_job_history", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qrtz_job_history");
        }
    }
}
