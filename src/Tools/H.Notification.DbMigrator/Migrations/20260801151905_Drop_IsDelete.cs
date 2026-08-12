using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Notification.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Drop_IsDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NotificationTemplates");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "NotificationContacts");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "NotificationContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NotificationContacts");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "NotificationContactGroups");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "NotificationContactGroups");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NotificationContactGroups");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "NotificationChannels");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "NotificationChannels");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NotificationChannels");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "NotificationCategories");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "NotificationCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NotificationCategories");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "NotificationBusinesses");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "NotificationBusinesses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NotificationBusinesses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "NotificationTemplates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "NotificationTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NotificationTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "NotificationContacts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "NotificationContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NotificationContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "NotificationContactGroups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "NotificationContactGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NotificationContactGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "NotificationChannels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "NotificationChannels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NotificationChannels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "NotificationCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "NotificationCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NotificationCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "NotificationBusinesses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "NotificationBusinesses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NotificationBusinesses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
