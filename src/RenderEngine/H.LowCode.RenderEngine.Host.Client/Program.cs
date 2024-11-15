using H.LowCode.RenderEngine.Host.Client;
using H.Util.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddHttpClient();

#region  LowCode
var application = await builder.AddApplicationAsync<RenderEngineHostClientModule>(options =>
{
    options.UseAutofac();
});
#endregion

var host = builder.Build();

await application.InitializeApplicationAsync(host.Services);

ServiceLocator.SetServiceProvider(host.Services);

await host.RunAsync();