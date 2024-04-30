using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.LowCode.Admin;
using H.LowCode.DesignEngine;
using H.LowCode.DesignEngine.DefaultComponents;
using H.LowCode.DesignEngine.CustomComponents;
using H.LowCode.RenderEngine.AntBlazor;
//using H.LowCode.RenderEngine.Html;
using H.LowCode.Theme.AntBlazorPro;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

#region DesignEngine
builder.Services.AddApplication<DesignEngineModule>();
builder.Services.AddApplication<DefaultComponentModule>();
builder.Services.AddApplication<CustomComponentModule>();
#endregion

#region  RenderEngine
builder.Services.AddApplication<RenderEngineForAntBlazorModule>();
//builder.Services.AddApplication<RenderEngineForHtmlModule>();
#endregion

#region ProAdmin
builder.Services.AddApplication<AdminModule>();
builder.Services.AddApplication<AntBlazorThemeModule>();
// builder.Services.AddApplication<HtmlThemeModule>();  //与AntBlazorThemeModule同时只能加载一个
#endregion

await builder.Build().RunAsync();
