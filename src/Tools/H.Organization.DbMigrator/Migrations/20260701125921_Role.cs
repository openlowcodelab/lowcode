using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Organization.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Organization_Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Organization_RoleMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Organization_Organizations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Organization_Members",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organization_Roles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organization_RoleMembers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organization_Organizations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organization_Members");
        }
    }
}
