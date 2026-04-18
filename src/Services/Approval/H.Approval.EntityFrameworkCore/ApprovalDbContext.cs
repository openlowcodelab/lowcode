using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace H.Approval.EntityFrameworkCore;

public class ApprovalDbContext : AbpDbContext<ApprovalDbContext>
{
    public ApprovalDbContext(DbContextOptions<ApprovalDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // 配置自定义业务表
        // Elsa 相关表由 Elsa.EntityFrameworkCore 自动配置
    }
}
