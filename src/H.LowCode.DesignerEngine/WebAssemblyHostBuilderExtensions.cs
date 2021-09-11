using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignerEngine
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static void AddDesignerEngine(this WebAssemblyHostBuilder builder)
        {
            //builder.RootComponents.Add<App>("#app");

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddAntDesign();
        }
    }
}
