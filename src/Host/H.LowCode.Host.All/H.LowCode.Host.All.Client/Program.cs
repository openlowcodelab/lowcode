using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.LowCode.Host.All.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 注册应用状态管理器
builder.Services.AddSingleton<AppStateManager>();

await builder.Build().RunAsync();
