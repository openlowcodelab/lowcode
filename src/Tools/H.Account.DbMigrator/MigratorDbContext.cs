using H.Account.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.Account.DbMigrator;

/// <summary>
/// 用于迁移数据库 (在MigratorDbContext所在项目的 Migrations 文件夹找迁移文件)
/// </summary>
public class MigratorDbContext : AccountDbContext
{
    public MigratorDbContext(DbContextOptions<AccountDbContext> options)
        : base(options)
    {

    }
}
