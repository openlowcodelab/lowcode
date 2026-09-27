using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovalId",
                table: "TaskExecutionStep",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutionStep_Kind_ApprovalState",
                table: "TaskExecutionStep",
                columns: new[] { "Kind", "ApprovalState" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskExecutionStep_Kind_ApprovalState",
                table: "TaskExecutionStep");

            migrationBuilder.DropColumn(
                name: "ApprovalId",
                table: "TaskExecutionStep");
        }
    }
}
