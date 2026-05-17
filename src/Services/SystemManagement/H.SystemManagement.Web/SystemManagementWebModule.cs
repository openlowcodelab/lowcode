using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace H.SystemManagement.Web;

public class SystemManagementWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAntDesign();
    }
}
