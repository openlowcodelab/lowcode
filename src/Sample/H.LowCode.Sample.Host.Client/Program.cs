using H.LowCode.DesignEngine.CustomComponents;
using H.LowCode.DesignEngine.DefaultComponents;
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
//builder.Services.AddApplication<HttpApiSampleModule>();  //��Ҫ���� HttpApi ��Ŀ,����ᱨ����NETSDK1082	Microsoft.AspNetCore.App û������ʱ��������ָ���� RuntimeIdentifier��browser-wasm��
#endregion

await builder.Build().RunAsync();
