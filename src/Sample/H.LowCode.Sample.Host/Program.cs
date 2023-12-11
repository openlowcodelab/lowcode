using H.LowCode.DesignEngine.Components.Custom;
using H.LowCode.DesignEngine.Components;
using H.LowCode.DesignEngine;
using H.LowCode.RenderEngine.AntBlazor;
using H.LowCode.RenderEngine.Html;
using H.LowCode.Sample.Admin;
using H.LowCode.Sample.Host.Components;
using H.LowCode.Sample.HttpApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

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
builder.Services.AddApplication<HttpApiSampleModule>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(H.LowCode.Sample.Admin._Imports).Assembly, typeof(H.LowCode.DesignEngine._Imports).Assembly);

app.Run();
