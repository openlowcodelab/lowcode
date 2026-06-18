using H.LowCode.Host.All.Components;
using H.YunXiaoMcpServer;
using H.LowCode.Host.All;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Configure SignalR
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

#region
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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapMcp("/yunxiao").AllowAnonymous();

// current-user 端点已由 ABP 约定控制器自动生成（AccountAppService.GetCurrentUserAsync）
// 路由: GET /api/app/account/current-user，返回 UserDto?（已登录返回用户信息，未登录返回 null）

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.LowCode.Host.All.Client._Imports).Assembly,
        typeof(H.Portal.Web._Imports).Assembly,
        typeof(H.Account.Web._Imports).Assembly,
        typeof(H.Organization.Web._Imports).Assembly,
        typeof(H.Approval.Web._Imports).Assembly,
        typeof(H.LowCode.Workbench._Imports).Assembly,
        typeof(H.LowCode.DesignEngine._Imports).Assembly,
        typeof(H.LowCode.MyApp._Imports).Assembly,
        typeof(H.LowCode.PartsDesignEngine._Imports).Assembly,
        typeof(H.LowCode.Themes.AntBlazor._Imports).Assembly,
        typeof(H.Util.Blazor._Imports).Assembly,
        typeof(H.AutoTest.Web._Imports).Assembly,
        typeof(H.SystemManagement.Web._Imports).Assembly,
        typeof(H.Assistant.Web._Imports).Assembly);

app.Run();
