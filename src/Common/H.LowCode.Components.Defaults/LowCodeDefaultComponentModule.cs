using H.LowCode.ComponentBase;
using H.LowCode.ComponentBase.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;

namespace H.LowCode.Components.Defaults;

[DependsOn(typeof(ComponentBaseModule))]
public class LowCodeDefaultComponentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAntDesign();
        
        // 注册默认的表格数据提供者
        context.Services.TryAddTransient<ITableDataProvider, DefaultTableDataProvider>();
    }
}
