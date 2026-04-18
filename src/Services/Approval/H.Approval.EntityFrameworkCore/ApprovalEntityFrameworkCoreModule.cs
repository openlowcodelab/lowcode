using H.Approval.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.Approval.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule)
)]
public class ApprovalEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册自定义仓储
        context.Services.AddTransient<IApprovalDefinitionRepository, ApprovalDefinitionRepository>();
        context.Services.AddTransient<IApprovalInstanceRepository, ApprovalInstanceRepository>();
        context.Services.AddTransient<IApprovalTaskRepository, ApprovalTaskRepository>();

        // 配置 Approval DbContext
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("ApprovalDb");
        context.Services.AddDbContext<ApprovalDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // TODO: 配置 Elsa EF Core 持久化
        // context.Services.AddElsa(elsa => elsa
        //     .UseEntityFrameworkPersistence(ef => ef.UseSqlServer())
        // );


    }
}
