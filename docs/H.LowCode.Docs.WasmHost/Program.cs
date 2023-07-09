using H.LowCode.Docs.WasmHost;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLowCodeDumiDocs(typeof(H.LowCode.Docs.Pages.Index).Assembly);

await builder.Build().RunAsync();
