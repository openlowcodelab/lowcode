using H.Organization.Host;
using H.Organization.Host.Components;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#region Organization
builder.Host.UseAutofac();
await builder.AddApplicationAsync<OrganizationHostModule>();
builder.Services.AddAntDesign();

// 移除 ABP 对 StaticFileOptions 的配置，避免与 .NET 10 的 MapStaticAssets 冲突
var abpStaticFileOptionsSetup = builder.Services
    .FirstOrDefault(d => d.ServiceType == typeof(IConfigureOptions<StaticFileOptions>));
if (abpStaticFileOptionsSetup != null)
{
    builder.Services.Remove(abpStaticFileOptionsSetup);
}
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

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(H.Organization.Host.Client._Imports).Assembly,
        typeof(H.Organization.Web._Imports).Assembly);

app.Run();
