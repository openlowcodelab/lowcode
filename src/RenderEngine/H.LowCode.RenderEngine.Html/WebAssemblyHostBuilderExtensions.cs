using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Html
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static void AddHtmlRenderEngine(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<IRender, HtmlRender>();
        }
    }
}
