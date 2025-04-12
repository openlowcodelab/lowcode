using H.Util.Blazor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Modularity;

namespace H.LowCode.ComponentBase;

public class LowCodeComponentBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //状态管理
        context.Services.AddScoped(typeof(ComponentState<>));
        context.Services.AddScoped(typeof(ComponentState<,>));
    }
}
