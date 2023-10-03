using H.LowCode.DesignEngine.Components.Custom;
using H.LowCode.DesignEngine.Components;
using H.LowCode.DesignEngine;
using H.LowCode.RenderEngine.AntBlazor;
using H.LowCode.Sample.Admin;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace H.LowCode.Render.WasmHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            #region DesignEngine
            // 设计器引擎
            builder.AddDesignerEngine();

            // 加载基础组件
            builder.AddBasicComponents();
            // 加载自定义组件
            builder.AddCustomComponents();
            #endregion

            #region  RenderEngine
            //HTML
            //builder.AddHtmlTheme();
            //builder.AddHtmlRenderEngine();

            //AntBlazor
            builder.AddAntBlazorProTheme();
            builder.AddAntBlazorRenderEngine();
            #endregion

            await builder.Build().RunAsync();
        }
    }
}
