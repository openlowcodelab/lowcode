using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.File.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "FileProjects");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "FileProjects");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FileProjects");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "FileFolders");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "FileFolders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FileFolders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "FileProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "FileProjects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FileProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "FileFolders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "FileFolders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FileFolders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
