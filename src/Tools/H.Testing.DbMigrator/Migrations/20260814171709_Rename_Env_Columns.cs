using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Testing.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Rename_Env_Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnvironmentSnapshotJson",
                table: "CaseExecutionRecord",
                newName: "EnvSnapshotJson");

            migrationBuilder.RenameColumn(
                name: "EnvironmentName",
                table: "CaseExecutionRecord",
                newName: "EnvName");

            migrationBuilder.RenameColumn(
                name: "EnvironmentId",
                table: "CaseExecutionRecord",
                newName: "EnvId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnvSnapshotJson",
                table: "CaseExecutionRecord",
                newName: "EnvironmentSnapshotJson");

            migrationBuilder.RenameColumn(
                name: "EnvName",
                table: "CaseExecutionRecord",
                newName: "EnvironmentName");

            migrationBuilder.RenameColumn(
                name: "EnvId",
                table: "CaseExecutionRecord",
                newName: "EnvironmentId");
        }
    }
}
