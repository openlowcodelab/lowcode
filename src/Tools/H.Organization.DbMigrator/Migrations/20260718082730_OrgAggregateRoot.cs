using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Organization.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class OrgAggregateRoot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Organization_Roles",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Organization_Roles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Organization_RoleMembers",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Organization_RoleMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Organization_Organizations",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Organization_Organizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Organization_Members",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Organization_Members",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Organization_Invites",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "Organization_Invites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Organization_Roles");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Organization_Roles");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Organization_RoleMembers");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Organization_RoleMembers");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Organization_Organizations");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Organization_Organizations");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Organization_Members");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Organization_Members");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Organization_Invites");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "Organization_Invites");
        }
    }
}
