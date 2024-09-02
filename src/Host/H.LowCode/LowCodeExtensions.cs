using H.LowCode.Workbench;
using H.LowCode.DesignEngine;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.ThemeParts.AntBlazor;
using Microsoft.Extensions.DependencyInjection;
using H.Util.Blazor;
using Microsoft.Extensions.Configuration;
using H.LowCode.DesignEngine.Admin;
using H.LowCode.RenderEngine;

namespace H.LowCode;

public static class LowCodeExtensions
{
    public static void AddLowCode(this IServiceCollection services)
    {
        #region 工作台
        services.AddApplication<LowCodeAdminModule>();
        #endregion

        #region DesignEngine
        services.AddApplication<DesignEngineModule>();
        services.AddApplication<DesignEngineAdminModule>();
        #endregion

        #region RenderEngine
        services.AddApplication<RenderEngineModule>();
        #endregion

        #region 公共
        //ComponentParts
        services.AddApplication<DefaultComponentModule>();
        services.AddApplication<ExtensionComponentModule>();
        //ThemeParts
        services.AddApplication<AntBlazorThemeModule>();

        //状态管理
        services.AddScoped(typeof(ComponentStateWrapper<,>));
        #endregion
    }
}
