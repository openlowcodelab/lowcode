using H.LowCode.Designer.Admin;
using H.LowCode.Designer.WasmHost;
using H.LowCode.DesignEngine;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.LowCode.DesignEngine.Components;
using H.LowCode.DesignEngine.Components.Custom;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// �������̨����
builder.AddDesignerAdmin();
// ���������
builder.AddDesignerEngine();

// ���ػ������
builder.AddBasicComponents();
// �����Զ������
builder.AddCustomComponents();

await builder.Build().RunAsync();