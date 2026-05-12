using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace F1Net.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnomalyDriverMeanLapTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DriverMeanLapTime",
                table: "AnomalyFlags",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverMeanLapTime",
                table: "AnomalyFlags");
        }
    }
}
