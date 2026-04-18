using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Approval.Web;

public class ApprovalWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // ע�� AntDesign
        context.Services.AddAntDesign();
    }
}
