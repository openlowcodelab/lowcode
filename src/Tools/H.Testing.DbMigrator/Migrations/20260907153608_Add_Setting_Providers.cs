using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Testing.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_Setting_Providers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderKey",
                table: "Settings",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "Settings",
                type: "varchar(1)",
                unicode: false,
                maxLength: 1,
                nullable: false,
                defaultValueSql: "'G'");

            // 全局设置的 ProviderKey 统一存为空串（而非 NULL），以便被下方唯一索引覆盖并与代码查询一致
            migrationBuilder.Sql("UPDATE [Settings] SET [ProviderKey] = '' WHERE [ProviderKey] IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Key_ProviderName_ProviderKey",
                table: "Settings",
                columns: new[] { "Key", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[ProviderKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Settings_Key_ProviderName_ProviderKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ProviderKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "Settings");
        }
    }
}
