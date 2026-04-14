using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Organization.Web;

public class OrganizationWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 AntDesign
        context.Services.AddAntDesign();
    }
}
