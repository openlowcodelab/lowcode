using H.LowCode.Render.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;

namespace H.LowCode.Render.HttpApi
{
    [DependsOn(typeof(RenderAppServiceModule))]
    public class RenderHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}
