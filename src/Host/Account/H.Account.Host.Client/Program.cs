using H.Account.Host.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var application = await builder.AddApplicationAsync<AccountHostClientModule>(options =>
{
    options.UseAutofac();
});

var host = builder.Build();

await application.InitializeApplicationAsync(host.Services);

await host.RunAsync();
