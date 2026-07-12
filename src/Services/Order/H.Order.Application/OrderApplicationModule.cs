using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer;
using H.Order.Application.Contracts;
using H.Order.Application.Services;
using H.Order.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Savorboard.CAP.InMemoryMessageQueue;
using Volo.Abp.Modularity;

namespace H.Order.Application;

[DependsOn(
    typeof(OrderApplicationContractsModule),
    typeof(OrderEntityFrameworkCoreModule)
)]
public class OrderApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        context.Services.AddHttpClient();

        // 供应商对接：注册各协议实现（ISupplierClient），由 SupplierClientFactory 按 Protocol 分发
        // 新增协议（如 MQ、gRPC）只需新增 ISupplierClient 实现并在此注册
        context.Services.AddTransient<ISupplierClient, HttpSupplierClient>();
        context.Services.AddTransient<ISupplierClient, MockSupplierClient>();
        context.Services.AddTransient<ISupplierClientFactory, SupplierClientFactory>();

        // 领域服务
        context.Services.AddScoped<IRouteEngine, RouteEngine>();
        context.Services.AddScoped<IDispatchService, DispatchService>();

        // CAP：SqlServer 存储作为 Outbox 表；In-Memory 消息队列作为传输层（开发环境无外部 MQ 依赖）。
        // 生产环境只需要把 UseInMemoryMessageQueue() 换成 UseRabbitMQ/UseKafka 等，其余代码不用改。
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("OrderDb");

        context.Services.AddCap(capOptions =>
        {
            capOptions.UseSqlServer(connectionString);
            capOptions.UseInMemoryMessageQueue();
            capOptions.FailedRetryCount = 5;          // 失败重试次数
            capOptions.FailedRetryInterval = 60;       // 失败重试间隔（秒）
        });

        // 注册 CAP 消费者
        context.Services.AddTransient<OrderDispatchEventConsumer>();
    }
}