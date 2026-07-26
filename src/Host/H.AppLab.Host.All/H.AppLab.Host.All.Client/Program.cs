using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.AppLab.Host.All.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

ClientServices.Configure(builder.Services, builder.Configuration, builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
