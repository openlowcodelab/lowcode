using H.LowCode.DesignEngine.HttpApi;
using H.LowCode.DesignEngine.Host.Client;
using System.Text.Json;
using H.LowCode.DesignEngine.Host.Components;
using H.Util.Blazor;
using Microsoft.Extensions.Hosting;
using H.LowCode.Repository.JsonFile;
using H.LowCode.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

#region  LowCode
builder.Services.AddDesignEngine();
builder.Services.AddDesignEngineHttpApi();
builder.Services.AddApplication<LowCodeEntityFrameworkCoreModule>();
builder.Services.AddApplication<MetaJsonFileRepositoryModule>();
#endregion

var app = builder.Build();

ServiceLocator.SetServiceProvider(app.Services);



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseRouting();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(LowCodeGlobalVariables.AdditionalAssemblies);  //LowCode

app.Run();
