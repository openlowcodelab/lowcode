using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Order.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "RouteRules");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "RouteRules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RouteRules");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "OrderExtensions");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "OrderExtensions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrderExtensions");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "DispatchLogs");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "DispatchLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DispatchLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Suppliers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "RouteRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "RouteRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RouteRules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "OrderExtensions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "OrderExtensions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrderExtensions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "DispatchLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "DispatchLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DispatchLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
