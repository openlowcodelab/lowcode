using Autofac.Extensions.DependencyInjection;
using H.LowCode.DesignEngine.Host.Client;
using H.Util.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ���� HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//ע�� IHttpClientFactory
//builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);  // ��Ҫ��ȷ���û�����ַ
});

#region  LowCode
builder.ConfigureContainer(new AutofacServiceProviderFactory());

builder.Services.AddApplication<DesignEngineHostClientModule>();
#endregion

var app = builder.Build();

ServiceLocator.SetServiceProvider(app.Services);

await app.RunAsync();
