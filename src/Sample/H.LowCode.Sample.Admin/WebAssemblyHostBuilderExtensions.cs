using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.Sample.Admin
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
