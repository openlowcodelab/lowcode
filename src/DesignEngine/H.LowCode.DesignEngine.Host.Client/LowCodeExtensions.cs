using H.LowCode.Workbench;
using H.LowCode.DesignEngine;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using Microsoft.Extensions.DependencyInjection;
using H.Util.Blazor;
using H.LowCode.MyApp;
using H.LowCode.PartsDesignEngine;
using H.LowCode.ComponentBase;

namespace H.LowCode.DesignEngine.Host.Client;

public static class LowCodeExtensions
{
    public static void AddDesignEngine(this IServiceCollection services)
    {
        #region Workbench
        services.AddApplication<LowCodeWorkbenchModule>();
        #endregion

        #region DesignEngine
        services.AddApplication<DesignEngineModule>();
        services.AddApplication<MyAppModule>();
        services.AddApplication<PartsDesignEngineModule>();
        #endregion

        #region Parts
        //ComponentParts
        services.AddApplication<DefaultComponentModule>();
        services.AddApplication<ExtensionComponentModule>();
        #endregion

        //状态管理
        services.AddScoped(typeof(ComponentStateWrapper<,>));

        //应用状态
        services.AddSingleton(new LowCodeAppState(true));
    }
}
