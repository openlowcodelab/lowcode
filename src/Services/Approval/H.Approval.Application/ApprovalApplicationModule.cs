using Volo.Abp.Modularity;
using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;

namespace H.Approval.Application;

[DependsOn(
    typeof(ApprovalApplicationContractsModule),
    typeof(ApprovalEntityFrameworkCoreModule)
)]
public class ApprovalApplicationModule : AbpModule
{
}
