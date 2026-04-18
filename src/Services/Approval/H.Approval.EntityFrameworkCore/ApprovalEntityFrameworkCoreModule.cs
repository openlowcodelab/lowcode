using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using H.Approval.Application.Contracts;

namespace H.Approval.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule),
    typeof(ApprovalApplicationContractsModule)
)]
public class ApprovalEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ApprovalDbContext>(options =>
        {
            // 添加默认仓储
            options.AddDefaultRepositories(includeAllEntities: true);
        });
        
        // TODO: 配置 Elsa EF Core 持久化
        // context.Services.AddElsa(elsa => elsa
        //     .UseEntityFrameworkPersistence(ef => ef.UseSqlServer())
        // );
    }
}
