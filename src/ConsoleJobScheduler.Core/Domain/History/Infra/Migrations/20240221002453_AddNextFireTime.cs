using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleJobScheduler.Core.Domain.History.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddNextFireTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "next_fire_time",
                table: "qrtz_job_history",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_fire_time",
                table: "qrtz_job_history");
        }
    }
}
