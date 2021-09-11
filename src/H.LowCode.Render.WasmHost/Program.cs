using H.LowCode.RenderEngine.AntBlazor;
using H.LowCode.Theme.AntBlazorPro;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Render.WasmHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            //HTML
            //builder.AddHtmlTheme();
            //builder.AddHtmlRenderEngine();

            //AntBlazor
            builder.AddAntBlazorProTheme();
            builder.AddAntBlazorRenderEngine();

            await builder.Build().RunAsync();
        }
    }
}
