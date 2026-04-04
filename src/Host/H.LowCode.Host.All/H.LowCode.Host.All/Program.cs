using H.LowCode.Host.All.Client.Pages;
using H.LowCode.Host.All.Components;
using H.LowCode.Host.All.Shared.Services;
using H.LowCode.Host.All.Services;
using Microsoft.EntityFrameworkCore;
using H.Account.EntityFrameworkCore;
using H.Organization.EntityFrameworkCore;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.EntityFrameworkCore;
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

// Session support
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<H.LowCode.ComponentBase.ISessionStorageService, H.LowCode.Host.All.Services.ServerSessionStorageService>();

// 注册应用状态管理器
builder.Services.AddSingleton<AppStateManager>();

#region ABP Modules
builder.Host.UseAutofac();
await builder.AddApplicationAsync<HostAllModule>();
builder.Services.AddAntDesign();
#endregion

var app = builder.Build();

await app.InitializeApplicationAsync();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // 初始化 Account 数据库
    try
    {
        var accountDb = services.GetService<AccountDbContext>();
        accountDb?.Database.EnsureCreated();
        logger.LogInformation("Account database initialized");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Account database initialization skipped");
    }
    
    // 初始化 Organization 数据库
    try
    {
        var orgDb = services.GetService<OrganizationDbContext>();
        orgDb?.Database.EnsureCreated();
        logger.LogInformation("Organization database initialized");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Organization database initialization skipped");
    }
    
    // 初始化 DesignEngine 数据库
    try
    {
        var designDb = services.GetService<DesignEngineDbContext>();
        designDb?.Database.EnsureCreated();
        logger.LogInformation("DesignEngine database initialized");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "DesignEngine database initialization skipped");
    }
    
    // 初始化 RenderEngine 数据库
    try
    {
        var renderDb = services.GetService<RenderEngineDbContext>();
        renderDb?.Database.EnsureCreated();
        logger.LogInformation("RenderEngine database initialized");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "RenderEngine database initialization skipped");
    }
}

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
app.UseSession();
app.UseAntiforgery();

app.MapControllers();

// Map Razor Components with all assemblies
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.LowCode.Host.All.Client._Imports).Assembly,
        typeof(H.LowCode.Portal._Imports).Assembly);

app.Run();
