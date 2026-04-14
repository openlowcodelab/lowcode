using H.LowCode.Host.All.Components;
using H.Admin.AppDrawer;
using Microsoft.EntityFrameworkCore;
using H.LowCode.Host.All;
using H.LowCode.ComponentBase;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Configure SignalR (DesignEngine 需要)
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Configure JSON serialization
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Response compression
builder.Services.AddResponseCompression();

// 注册应用状态管理器
builder.Services.AddSingleton<AppStateManager>();

// 注册 LowCodeAppState (设计时为 true)
builder.Services.AddScoped(sp => new LowCodeAppState(isDesign: true));

#region ABP Modules
builder.Host.UseAutofac();
await builder.AddApplicationAsync<HostAllModule>();
#endregion

var app = builder.Build();

await app.InitializeApplicationAsync();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});
app.MapStaticAssets();

app.UseRouting();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.LowCode.Host.All.Client._Imports).Assembly,
        typeof(H.LowCode.Portal._Imports).Assembly,
        typeof(H.Account.Web._Imports).Assembly,
        typeof(H.Organization.Web._Imports).Assembly,
        typeof(H.LowCode.Workbench._Imports).Assembly,
        typeof(H.LowCode.DesignEngine._Imports).Assembly,
        typeof(H.LowCode.MyApp._Imports).Assembly,
        typeof(H.LowCode.PartsDesignEngine._Imports).Assembly,
        typeof(H.LowCode.Themes.AntBlazor._Imports).Assembly,
        typeof(H.Util.Blazor._Imports).Assembly);

app.Run();
