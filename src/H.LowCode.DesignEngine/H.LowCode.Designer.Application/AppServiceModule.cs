using H.LowCode.Designer.Application.Abstraction.AppService;
using H.LowCode.Designer.Application.AppServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;

namespace H.LowCode.Designer.Application
{
    public class AppServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerAppService, DesignerAppService>();
        }
    }
}
