using H.SupplyChain.Application.Contracts;
using H.SupplyChain.Application.Services;
using H.SupplyChain.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.SupplyChain.Application;

[DependsOn(
    typeof(SupplyChainApplicationContractsModule),
    typeof(SupplyChainEntityFrameworkCoreModule)
)]
public class SupplyChainApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        context.Services.AddHttpClient();

        // 供应商接口调用：注册各协议实现（ISupplierApiInvoker），由工厂按 Protocol 分发
        // 新增协议（如 MQ、gRPC）只需新增 ISupplierApiInvoker 实现并在此注册
        context.Services.AddTransient<ISupplierApiInvoker, HttpSupplierApiInvoker>();
        context.Services.AddTransient<ISupplierApiInvoker, MockSupplierApiInvoker>();
        context.Services.AddTransient<ISupplierApiInvokerFactory, SupplierApiInvokerFactory>();

        // 对外 API 服务
        context.Services.AddScoped<ISupplyChainApiAppService, SupplyChainApiAppService>();
    }
}