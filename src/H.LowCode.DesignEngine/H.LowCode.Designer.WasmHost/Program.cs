using H.LowCode.Designer.Admin;
using H.LowCode.Designer.WasmHost;
using H.LowCode.DesignEngine;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.LowCode.DesignEngine.Components;
using H.LowCode.DesignEngine.Components.Custom;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 设计器后台管理
builder.AddDesignerAdmin();
// 设计器引擎
builder.AddDesignerEngine();

// 加载基础组件
builder.AddBasicComponents();
// 加载自定义组件
builder.AddCustomComponents();

await builder.Build().RunAsync();