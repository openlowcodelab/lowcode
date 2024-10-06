using H.LowCode.DesignEngine.Host.Client;
using H.Util.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient();

#region  LowCode
builder.Services.AddDesignEngine();
#endregion

var app = builder.Build();

ServiceLocator.SetServiceProvider(app.Services);

await app.RunAsync();
