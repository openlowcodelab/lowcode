using AntDesign.ProLayout;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace H.LowCode.Theme.AntBlazorPro
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static void AddAntBlazorProTheme(this WebAssemblyHostBuilder builder)
        {
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddAntDesign();
        }
    }
}
