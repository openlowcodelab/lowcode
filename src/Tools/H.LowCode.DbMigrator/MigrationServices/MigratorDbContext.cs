using H.LowCode.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DbMigrator;

/// <summary>
/// 用于迁移数据库
/// 说明: 直接使用 LowCodeDbContext 会在 LowCodeDbContext 所在项目(即"H.LowCode.EntityFrameworkCore")中找 Migrations 文件夹下的迁移文件
///       由于迁移文件是在"H.LowCode.DbMigrator"项目中生成的, 所以在 "H.LowCode.DbMigrator" 中重新定义一个 DbContext
/// </summary>
public class MigratorDbContext : LowCodeDbContext
{
    public MigratorDbContext(DbContextOptions<LowCodeDbContext> options,
        EntityTypeManager entityTypeManager) : base(options, entityTypeManager)
    {
    }
}
