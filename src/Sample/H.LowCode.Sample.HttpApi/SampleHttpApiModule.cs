using H.LowCode.Sample.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Loader;
using Volo.Abp.Modularity;

namespace H.LowCode.Sample.HttpApi
{
    [DependsOn(typeof(SampleAppServiceModule))]
    public class SampleHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddControllers().AddApplicationPart(typeof(SampleHttpApiModule).Assembly);
        }
    }
}
