using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.Organization.DbMigrator;

/// <summary>
/// 用于迁移数据库 (在MigratorDbContext所在项目的 Migrations 文件夹找迁移文件)
/// </summary>
public class MigratorDbContext : OrganizationDbContext
{
    public MigratorDbContext(DbContextOptions<OrganizationDbContext> options)
        : base(options)
    {

    }
}
