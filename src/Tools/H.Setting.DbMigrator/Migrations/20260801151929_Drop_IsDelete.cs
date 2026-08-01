using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Setting.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppSettingValues");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppSettingValues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppSettingValues");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "AppSettingDefinitions");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AppSettingDefinitions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppSettingDefinitions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppSettingValues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppSettingValues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppSettingValues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "AppSettingDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AppSettingDefinitions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppSettingDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
