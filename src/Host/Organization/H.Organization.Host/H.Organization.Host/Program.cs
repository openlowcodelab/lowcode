using H.Organization.Host.Components;
using H.Organization.HttpApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#region Organization
builder.Host.UseAutofac();
await builder.AddApplicationAsync<OrganizationHttpApiModule>();
await builder.AddApplicationAsync<global::H.Organization.Host.OrganizationHostAccountClientModule>();
builder.Services.AddAntDesign();
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
