using H.LowCode.Sample.Application.Abstraction.AppServices;
using H.LowCode.Sample.Application.AppServices;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Sample.Application
{
    public class SampleAppServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<ISampleAppService, SampleAppService>();
        }
    }
}
