using H.LowCode.Admin;
using H.LowCode.DesignEngine;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.BasicComponents.Render;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.ThemeParts.AntBlazor;
using Microsoft.Extensions.DependencyInjection;
using H.Util.Blazor;
using Microsoft.Extensions.Configuration;

namespace H.LowCode
{
    public static class LowCodeExtensions
    {
        public static void AddLowCode(this IServiceCollection services)
        {
            services.AddScoped(typeof(ComponentStateWrapper<,>));

            #region DesignEngine
            services.AddApplication<DesignEngineModule>();
            services.AddApplication<DefaultComponentModule>();
            services.AddApplication<ExtensionComponentModule>();
            #endregion

            #region  RenderEngine
            services.AddApplication<RenderEngineForAntBlazorModule>();
            #endregion

            #region Admin
            services.AddApplication<LowCodeAdminModule>();
            services.AddApplication<AntBlazorThemeModule>();
            #endregion
        }
    }
}
