using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.ThemeParts.AntBlazor;
using H.Util.Blazor;

namespace H.LowCode.RenderEngine.Host.Client;

public static class LowCodeExtensions
{
    public static void AddRenderEngine(this IServiceCollection services)
    {
        services.AddApplication<RenderEngineModule>();

        //ComponentParts
        services.AddApplication<DefaultComponentModule>();
        services.AddApplication<ExtensionComponentModule>();

        //ThemeParts
        services.AddApplication<AntBlazorThemeModule>();

        //状态管理
        services.AddScoped(typeof(ComponentStateWrapper<,>));
    }
}
