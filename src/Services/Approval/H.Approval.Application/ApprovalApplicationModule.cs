using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Extensions;
using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        var connectionString = configuration.GetConnectionString("ApprovalDb");

        context.Services.AddElsa(elsa =>
        {
            elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef =>
                ef.UseSqlServer(connectionString!)));

            elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef =>
                ef.UseSqlServer(connectionString!)));

            elsa.UseWorkflowsApi();

            // 注册自定义审批活动
            //elsa.AddActivity<StartApprovalActivity>()
            //.AddActivity<ApprovalTaskActivity>()
            //.AddActivity<ConditionActivity>()
            //.AddActivity<CarbonCopyActivity>()
            //.AddActivity<ApprovalEndActivity>();
        });
    }
}
