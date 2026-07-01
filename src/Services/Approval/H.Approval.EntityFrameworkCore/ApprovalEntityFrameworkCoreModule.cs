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
        // 注册 ApprovalDbContext
        context.Services.AddDbContext<ApprovalDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("ApprovalDb");
            options.UseSqlServer(connectionString);
        });

        // 注册自定义仓储
        context.Services.AddTransient<IApprovalDefinitionRepository, ApprovalDefinitionRepository>();
        context.Services.AddTransient<IApprovalInstanceRepository, ApprovalInstanceRepository>();
        context.Services.AddTransient<IApprovalTaskRepository, ApprovalTaskRepository>();
    }
}
