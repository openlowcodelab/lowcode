using H.Account.Host.Components;
using H.Account.Host;
using System.Security.Claims;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

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

// 认证检查端点（供 InteractiveWebAssembly 客户端验证 Cookie 登录态，注册在 Blazor 路由之前）
app.MapGet("/api/app/account/current-user", async (HttpContext httpContext, IdentityUserManager userManager) =>
{
    if (httpContext.User?.Identity?.IsAuthenticated != true)
        return Results.Ok(new { isAuthenticated = false });

    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        return Results.Ok(new { isAuthenticated = false });

    var user = await userManager.FindByIdAsync(userId.ToString());
    if (user == null)
        return Results.Ok(new { isAuthenticated = false });

    return Results.Ok(new { isAuthenticated = true, id = user.Id, userName = user.UserName, email = user.Email, phoneNumber = user.PhoneNumber, isActive = user.IsActive });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.Account.Host.Client._Imports).Assembly,
        typeof(H.Account.Web._Imports).Assembly);

app.Run();
