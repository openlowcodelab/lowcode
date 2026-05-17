using H.SystemManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.SystemManagement.DbMigrator;

/// <summary>
/// 用于迁移数据库 (在MigratorDbContext所在项目的 Migrations 文件夹找迁移文件)
/// </summary>
public class MigratorDbContext : SystemManagementDbContext
{
    public MigratorDbContext(DbContextOptions<SystemManagementDbContext> options)
        : base(options)
    {

    }
}
