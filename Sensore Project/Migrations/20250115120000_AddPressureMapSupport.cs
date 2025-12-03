using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sensore_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddPressureMapSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to SensorData
            migrationBuilder.AddColumn<string>(
                name: "PressureMapJson",
                table: "SensorData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresClinicianReview",
                table: "SensorData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetricsJson",
                table: "SensorData",
                type: "nvarchar(max)",
                nullable: true);

            // Add new columns to Alerts
            migrationBuilder.AddColumn<int>(
                name: "PressureMapId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlertType",
                table: "Alerts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PressureAnomaly");

            migrationBuilder.AddColumn<string>(
                name: "ClusterInfoJson",
                table: "Alerts",
                type: "nvarchar(max)",
                nullable: true);

            // Add new columns to RiskPredictions
            migrationBuilder.AddColumn<int>(
                name: "PressureMapId",
                table: "RiskPredictions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnalysisType",
                table: "RiskPredictions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SingleValue");

            migrationBuilder.AddColumn<string>(
                name: "MapMetricsJson",
                table: "RiskPredictions",
                type: "nvarchar(max)",
                nullable: true);

            // Add indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_SensorData_RequiresClinicianReview_Timestamp",
                table: "SensorData",
                columns: new[] { "RequiresClinicianReview", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertType_Timestamp",
                table: "Alerts",
                columns: new[] { "AlertType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskPredictions_AnalysisType_Timestamp",
                table: "RiskPredictions",
                columns: new[] { "AnalysisType", "Timestamp" });

            // Add foreign key constraints for referential integrity
            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_SensorData_PressureMapId",
                table: "Alerts",
                column: "PressureMapId",
                principalTable: "SensorData",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskPredictions_SensorData_PressureMapId",
                table: "RiskPredictions",
                column: "PressureMapId",
                principalTable: "SensorData",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_SensorData_PressureMapId",
                table: "Alerts");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskPredictions_SensorData_PressureMapId",
                table: "RiskPredictions");

            // Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_SensorData_RequiresClinicianReview_Timestamp",
                table: "SensorData");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_AlertType_Timestamp",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_RiskPredictions_AnalysisType_Timestamp",
                table: "RiskPredictions");

            // Remove columns from RiskPredictions
            migrationBuilder.DropColumn(
                name: "PressureMapId",
                table: "RiskPredictions");

            migrationBuilder.DropColumn(
                name: "AnalysisType",
                table: "RiskPredictions");

            migrationBuilder.DropColumn(
                name: "MapMetricsJson",
                table: "RiskPredictions");

            // Remove columns from Alerts
            migrationBuilder.DropColumn(
                name: "PressureMapId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AlertType",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ClusterInfoJson",
                table: "Alerts");

            // Remove columns from SensorData
            migrationBuilder.DropColumn(
                name: "PressureMapJson",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "RequiresClinicianReview",
                table: "SensorData");

            migrationBuilder.DropColumn(
                name: "MetricsJson",
                table: "SensorData");
        }
    }
}


