using H.Account.Host.Components;
using H.Account.Host;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#region Account
builder.Host.UseAutofac();
await builder.AddApplicationAsync<AccountHostModule>();
#endregion

var app = builder.Build();

await app.InitializeApplicationAsync();

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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();

// current-user 端点已由 ABP 约定控制器自动生成（AccountAppService.GetCurrentUserAsync）
// 路由: GET /api/app/account/current-user，返回 UserDto?（已登录返回用户信息，未登录返回 null）

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.Account.Host.Client._Imports).Assembly,
        typeof(H.Account.Web._Imports).Assembly);

app.Run();
