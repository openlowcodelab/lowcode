using H.Approval.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.LowCode.DbMigrator;

/// <summary>
/// 用于迁移数据库 (在MigratorDbContext所在项目的 Migrations 文件夹找迁移文件)
/// </summary>
public class MigratorDbContext : ApprovalDbContext
{
    public MigratorDbContext(DbContextOptions<ApprovalDbContext> options)
        : base(options)
    {

    }
}
