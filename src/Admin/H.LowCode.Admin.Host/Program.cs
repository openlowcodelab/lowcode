using H.LowCode.DesignEngine;
using H.LowCode.DesignEngine.DefaultComponents;
using H.LowCode.DesignEngine.CustomComponents;
using H.LowCode.Admin;
using H.LowCode.Admin.Host.Components;
using H.LowCode.Admin.HttpApi;
using H.LowCode.RenderEngine.AntBlazor;
//using H.LowCode.RenderEngine.Html;
using H.LowCode.Theme.AntBlazorPro;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#region DesignEngine
builder.Services.AddApplication<DesignEngineModule>();
builder.Services.AddApplication<DefaultComponentModule>();
builder.Services.AddApplication<CustomComponentModule>();
#endregion

#region  RenderEngine
builder.Services.AddApplication<RenderEngineForAntBlazorModule>();
//builder.Services.AddApplication<RenderEngineForHtmlModule>();
#endregion

#region Admin
builder.Services.AddApplication<AdminModule>();
builder.Services.AddApplication<AntBlazorThemeModule>();
builder.Services.AddApplication<AdminHttpApiModule>();
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
    .AddAdditionalAssemblies(typeof(H.LowCode.Admin._Imports).Assembly, typeof(H.LowCode.DesignEngine._Imports).Assembly,
    typeof(H.LowCode.Theme.AntBlazorPro._Imports).Assembly
    );

app.Run();
