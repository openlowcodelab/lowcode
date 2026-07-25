using H.AppLab.Host.All.Components;
using H.YunXiaoMcpServer;
using H.AppLab.Host.All;
using Hangfire;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
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

// Response compression - 启用 Brotli 压缩以减少 WASM 资源传输体积
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
        "application/octet-stream",
        "application/wasm",
        "application/dll"
    ]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

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

// 由 MapStaticAssets 统一处理静态资源缓存：
// 指纹化(内容哈希)的 WASM 程序集会自动应用 immutable 长缓存，而 Blazor 启动清单
// (boot manifest，本身不带指纹)会保留重新校验语义。
// 切勿手动把整个 /_framework 标记为 immutable —— 否则应用重新构建后指纹程序集文件名
// 变化，但浏览器仍复用过期的启动清单，去请求已不存在的旧指纹程序集(静默 404)，
// 导致 WASM 运行时无法完成启动，页面永久卡在“加载中...”，且前后端均无错误日志。
app.MapStaticAssets();

app.UseRouting();
app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapMcp("/yunxiao").AllowAnonymous();

// Hangfire 后台任务仪表盘
app.UseHangfireDashboard("/hangfire");

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.AppLab.Host.All.Client._Imports).Assembly,
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
        typeof(H.Testing.Web._Imports).Assembly,
        typeof(H.Notification.Web._Imports).Assembly,
        typeof(H.Assistant.Web._Imports).Assembly,
        typeof(H.Order.Web._Imports).Assembly,
        typeof(H.SupplyChain.Web._Imports).Assembly,
        typeof(H.BackgroundTask.Web._Imports).Assembly);

app.Run();
