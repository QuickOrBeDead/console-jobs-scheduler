using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleJobScheduler.Core.Domain.History.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddCronExpression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cron_expression",
                table: "qrtz_job_history",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cron_expression",
                table: "qrtz_job_history");
        }
    }
}
