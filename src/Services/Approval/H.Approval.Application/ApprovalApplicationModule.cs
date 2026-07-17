using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Extensions;
using H.Approval.Application.Contracts;
using H.Approval.Application.Data;
using H.Approval.Application.Services;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace H.Approval.Application;

[DependsOn(
    typeof(ApprovalApplicationContractsModule),
    typeof(ApprovalEntityFrameworkCoreModule)
)]
public class ApprovalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("ApprovalDb");

        context.Services.AddElsa(elsa =>
        {
            elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef =>
                ef.UseSqlServer(connectionString!)));

            elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef =>
                ef.UseSqlServer(connectionString!)));

            elsa.UseWorkflowsApi();
        });

        // 注册自包含审批工作流引擎
        context.Services.AddTransient<ApprovalWorkflowEngine>();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        // 启动时幂等初始化常见审批模板(分类 + 预置审批定义)
        using var scope = context.ServiceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ApprovalTemplateSeeder>();
        await seeder.SeedAsync();
    }
}
