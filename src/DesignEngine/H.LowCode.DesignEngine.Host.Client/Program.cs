using Autofac.Extensions.DependencyInjection;
using H.LowCode.DesignEngine.Host.Client;
using H.Util.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 配置 HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//注册 IHttpClientFactory
//builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);  // 需要正确设置基础地址
});

#region  LowCode
builder.ConfigureContainer(new AutofacServiceProviderFactory());

builder.Services.AddApplication<DesignEngineHostClientModule>();
#endregion

var app = builder.Build();

ServiceLocator.SetServiceProvider(app.Services);

await app.RunAsync();
