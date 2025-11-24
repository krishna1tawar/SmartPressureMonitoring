using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sensore_Project.Migrations
{
    /// <inheritdoc />
    public partial class FixPressureType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnomalyScore",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "IsAnomalous",
                table: "SensorData");

            migrationBuilder.AlterColumn<float>(
                name: "Pressure",
                table: "SensorData",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Pressure",
                table: "SensorData",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<double>(
                name: "AnomalyScore",
                table: "SensorData",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnomalous",
                table: "SensorData",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
