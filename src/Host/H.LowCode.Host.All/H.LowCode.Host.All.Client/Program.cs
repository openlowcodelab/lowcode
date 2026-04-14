using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using H.Admin.AppDrawer;
using H.Account.Application.Contracts;
using H.Organization.Application.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 注册应用状态管理器
builder.Services.AddSingleton<AppStateManager>();

// 注册 HTTP Client 代理
builder.Services.AddHttpClientProxies(
    typeof(AccountApplicationContractsModule).Assembly,
    "Account"
);

builder.Services.AddHttpClientProxies(
    typeof(OrganizationApplicationContractsModule).Assembly,
    "Organization"
);

await builder.Build().RunAsync();
