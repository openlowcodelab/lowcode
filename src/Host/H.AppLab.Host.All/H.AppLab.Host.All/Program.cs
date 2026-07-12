using H.AppLab.Host.All.Components;
using H.YunXiaoMcpServer;
using H.AppLab.Host.All;
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
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // _framework 目录下的 WASM 资源使用指纹文件名，可以长期缓存
        if (ctx.Context.Request.Path.StartsWithSegments("/_framework"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000,immutable");
        }
        else
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
        }
    }
});
app.MapStaticAssets();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapMcp("/yunxiao").AllowAnonymous();

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
        typeof(H.AutoTest.Web._Imports).Assembly,
        typeof(H.Notification.Web._Imports).Assembly,
        typeof(H.Assistant.Web._Imports).Assembly,
        typeof(H.Order.Web._Imports).Assembly);

app.Run();
