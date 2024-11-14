using Autofac.Extensions.DependencyInjection;
using H.LowCode.RenderEngine.Host.Client;
using H.Util.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient();

#region  LowCode
builder.ConfigureContainer(new AutofacServiceProviderFactory());

builder.Services.AddApplication<RenderEngineHostClientModule>();
#endregion

var app = builder.Build();

ServiceLocator.SetServiceProvider(app.Services);

await app.RunAsync();
