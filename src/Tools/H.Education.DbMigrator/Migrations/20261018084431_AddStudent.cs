using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Education.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "PracticeSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "AnswerSheets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    GuardianPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_StudentId",
                table: "PracticeSessions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSheets_StudentId",
                table: "AnswerSheets",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Grade",
                table: "Students",
                column: "Grade");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Name",
                table: "Students",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNo",
                table: "Students",
                column: "StudentNo",
                unique: true,
                filter: "[StudentNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_PracticeSessions_StudentId",
                table: "PracticeSessions");

            migrationBuilder.DropIndex(
                name: "IX_AnswerSheets_StudentId",
                table: "AnswerSheets");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "AnswerSheets");
        }
    }
}
