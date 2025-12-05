using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sensore_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertCommentsWithFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackText",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedbackProvidedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedbackUserId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AlertId",
                table: "Comments",
                column: "AlertId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Alerts_AlertId",
                table: "Comments",
                column: "AlertId",
                principalTable: "Alerts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Alerts_AlertId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AlertId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AlertId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FeedbackText",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FeedbackProvidedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FeedbackUserId",
                table: "Comments");
        }
    }
}
