using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.LowCode.Host.All.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var application = await builder.AddApplicationAsync<HostAllClientModule>(options =>
{
    options.UseAutofac();
});

await builder.Build().RunAsync();
