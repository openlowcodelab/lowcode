using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Assistant.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Agent");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Agent");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Agent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Skill",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Skill",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Skill",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Agent",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Agent",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Agent",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
