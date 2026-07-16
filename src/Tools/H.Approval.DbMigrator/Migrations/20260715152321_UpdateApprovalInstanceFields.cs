using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Approval.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApprovalInstanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApprovalTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentNodeId",
                table: "ApprovalInstances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApprovalInstances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ApprovalInstances",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApprovalDefinitions",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalTasks");

            migrationBuilder.DropColumn(
                name: "CurrentNodeId",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ApprovalInstances");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApprovalDefinitions");
        }
    }
}
