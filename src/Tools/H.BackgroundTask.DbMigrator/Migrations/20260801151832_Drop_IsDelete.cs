using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.BackgroundTask.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "BackgroundJobs");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "BackgroundJobs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BackgroundJobs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "BackgroundJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "BackgroundJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BackgroundJobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
