using H.LowCode.RenderEngine.Host.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient();

#region  LowCode
builder.Services.AddRenderEngine();
#endregion

await builder.Build().RunAsync();
