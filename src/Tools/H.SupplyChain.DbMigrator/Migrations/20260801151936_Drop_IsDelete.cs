using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.SupplyChain.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "SupplierSkuMappings");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "SupplierSkuMappings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupplierSkuMappings");

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
                table: "SupplierInterfaceMappings");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "SupplierInterfaceMappings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupplierInterfaceMappings");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductSkus");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "ApiInterfaces");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ApiInterfaces");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ApiInterfaces");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "SupplierSkuMappings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "SupplierSkuMappings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupplierSkuMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                table: "SupplierInterfaceMappings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "SupplierInterfaceMappings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupplierInterfaceMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "ProductSkus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ProductSkus",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductSkus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "ApiInterfaces",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ApiInterfaces",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ApiInterfaces",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
