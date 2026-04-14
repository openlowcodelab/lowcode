using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Account.Web;

public class AccountWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 AntDesign
        context.Services.AddAntDesign();
    }
}
