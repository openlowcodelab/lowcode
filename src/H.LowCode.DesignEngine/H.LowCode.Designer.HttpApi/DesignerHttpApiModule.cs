using H.LowCode.Designer.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Loader;
using Volo.Abp.Modularity;

namespace H.LowCode.Designer.HttpApi
{
    [DependsOn(typeof(DesignerAppServiceModule))]
    public class DesignerHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddControllers().AddApplicationPart(typeof(DesignerHttpApiModule).Assembly);
        }
    }
}
