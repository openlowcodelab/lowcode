using H.LowCode.DesignEngine.Components.Custom.ComponentProviders;
using H.LowCode.Metadata;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.DesignEngine.Components.Custom
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static void AddCustomComponents(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<IComponentProvider, CustomComponentProvider>();
        }
    }
}
