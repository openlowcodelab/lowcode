using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Approval.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTenantAndAddWorkflowFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalTasks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalDefinitions");

            migrationBuilder.AddColumn<string>(
                name: "NodeId",
                table: "ApprovalTasks",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VariablesJson",
                table: "ApprovalInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTasks_NodeId",
                table: "ApprovalTasks",
                column: "NodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApprovalTasks_NodeId",
                table: "ApprovalTasks");

            migrationBuilder.DropColumn(
                name: "NodeId",
                table: "ApprovalTasks");

            migrationBuilder.DropColumn(
                name: "VariablesJson",
                table: "ApprovalInstances");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApprovalTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApprovalInstances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApprovalDefinitions",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
