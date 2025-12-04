using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sensore_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddImportJobTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalFiles = table.Column<int>(type: "int", nullable: false),
                    ProcessedFiles = table.Column<int>(type: "int", nullable: false),
                    TotalMaps = table.Column<int>(type: "int", nullable: false),
                    ProcessedMaps = table.Column<int>(type: "int", nullable: false),
                    CurrentFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedFilesList = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobs");
        }
    }
}
