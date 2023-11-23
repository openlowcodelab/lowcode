using H.LowCode.Sample.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Sample.HttpApi
{
    [DependsOn(typeof(AppServiceSampleModule))]
    public class HttpApiSampleModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddControllers().AddApplicationPart(typeof(HttpApiSampleModule).Assembly);
        }
    }
}
