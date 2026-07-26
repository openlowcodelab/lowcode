using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using H.AppLab.Host.All.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 组合式容器：支持懒加载模块在导航时向模块子容器追加服务注册
var lazyModuleRegistry = new LazyModuleRegistry();
builder.Services.AddSingleton(lazyModuleRegistry);
builder.ConfigureContainer(new CompositeServiceProviderFactory(lazyModuleRegistry));

ClientServices.Configure(builder.Services, builder.Configuration, builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
