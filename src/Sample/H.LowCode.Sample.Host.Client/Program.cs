using H.LowCode.DesignEngine.Components.Custom;
using H.LowCode.DesignEngine.Components;
using H.LowCode.DesignEngine;
using H.LowCode.RenderEngine.AntBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.LowCode.Sample.Admin;
using H.LowCode.RenderEngine.Html;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

#region DesignEngine
builder.Services.AddApplication<DesignEngineModule>();
builder.Services.AddApplication<DesignEngineComponentModule>();
builder.Services.AddApplication<DesignEngineCustomComponentModule>();
#endregion

#region  RenderEngine
builder.Services.AddApplication<RenderEngineForAntBlazorModule>();
builder.Services.AddApplication<RenderEngineForHtmlModule>();
#endregion

#region Sample
builder.Services.AddApplication<AdminSampleModule>();
//builder.Services.AddApplication<HttpApiSampleModule>();  //不要依赖 HttpApi 项目,否则会报错：NETSDK1082	Microsoft.AspNetCore.App 没有运行时包可用于指定的 RuntimeIdentifier“browser-wasm”
#endregion

await builder.Build().RunAsync();
