using H.LowCode.RenderEngine.Host.Client;
using H.Util.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

ClientServices.Configure(builder.Services, builder.Configuration, builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
