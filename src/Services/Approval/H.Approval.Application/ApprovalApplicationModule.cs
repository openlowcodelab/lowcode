using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        // TODO: 等待Elsa 3.x API熟悉后再启用自定义活动
        // 当前先使用内存存储实现审批功能

        /*
        context.Services.AddElsa(elsa =>
        {
            elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore())
                .UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore())
                .UseEntityFrameworkCorePersistence(options =>
                {
                    // 使用独立Sqlite数据库存储Elsa工作流数据
                    var connectionString = configuration.GetConnectionString("ElsaDb") 
                        ?? "Data Source=elsa-workflows.db";
                    options.UseSqlite(connectionString);
                })
                // 注册自定义审批活动
                //.AddActivity<StartApprovalActivity>()
                //.AddActivity<ApprovalTaskActivity>()
                //.AddActivity<ConditionActivity>()
                //.AddActivity<CarbonCopyActivity>()
                //.AddActivity<ApprovalEndActivity>();
        });
        */
    }
}
