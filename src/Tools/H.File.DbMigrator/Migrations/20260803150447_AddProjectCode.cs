using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.File.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "FileProjects",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // 为已有项目回填编号：从 BucketName 中剥离租户前缀得出原始编号
            migrationBuilder.Sql(@"
UPDATE FileProjects
SET Code = LOWER(LEFT(REPLACE(RIGHT(BucketName, LEN(BucketName) - CHARINDEX('-', BucketName)), '-', ''), 20))
WHERE Code = '';");

            // 无法推导出合法编号时，用 Id 的前 10 位字符生成小写字母编号
            migrationBuilder.Sql(@"
UPDATE FileProjects
SET Code = LOWER(TRANSLATE(LEFT(CONVERT(varchar(36), Id), 10), '0123456789', 'abcdefghij'))
WHERE LEN(Code) < 3;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "FileProjects");
        }
    }
}
