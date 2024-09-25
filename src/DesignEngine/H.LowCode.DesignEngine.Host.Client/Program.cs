using H.LowCode.DesignEngine.Host.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient();

#region  LowCode
builder.Services.AddDesignEngine();
#endregion

await builder.Build().RunAsync();
