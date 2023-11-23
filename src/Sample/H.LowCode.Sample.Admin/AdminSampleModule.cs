using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Sample.Admin
{
    public class AdminSampleModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAntDesign();
            context.Services.AddScoped(sp => new HttpClient());
        }
    }
}
