using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Testing.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Rename_TestCaseId_To_CaseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TestCaseId",
                table: "CaseExecutionRecord",
                newName: "CaseId");

            migrationBuilder.RenameIndex(
                name: "IX_CaseExecutionRecord_TestCaseId",
                table: "CaseExecutionRecord",
                newName: "IX_CaseExecutionRecord_CaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CaseId",
                table: "CaseExecutionRecord",
                newName: "TestCaseId");

            migrationBuilder.RenameIndex(
                name: "IX_CaseExecutionRecord_CaseId",
                table: "CaseExecutionRecord",
                newName: "IX_CaseExecutionRecord_TestCaseId");
        }
    }
}
