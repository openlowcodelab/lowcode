using H.LowCode.DesignEngine.Components.ComponentProviders;
using H.LowCode.Metadata.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.DesignEngine.Components
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public static void AddBasicComponents(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<IComponentProvider, BasicComponentProvider>();
            builder.Services.AddScoped<IComponentProvider, SeniorComponentProvider>();
            builder.Services.AddScoped<IComponentProvider, LayoutComponentProvider>();
        }
    }
}
