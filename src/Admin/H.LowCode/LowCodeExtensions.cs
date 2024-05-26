using H.LowCode.Admin;
using H.LowCode.DesignEngine.ExtensionComponents;
using H.LowCode.DesignEngine.DefaultComponents;
using H.LowCode.DesignEngine;
using H.LowCode.RenderEngine.AntBlazor;
using H.LowCode.Theme.AntBlazorPro;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode
{
    public static class LowCodeExtensions
    {
        public static void AddLowCode(this IServiceCollection services)
        {
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
