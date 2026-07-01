using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.YunXiaoMcpServer;

public class YunXiaoMcpServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 绑定云效配置
        context.Services.Configure<YunXiaoOptions>(
            configuration.GetSection(YunXiaoOptions.SectionName));

        // 注册 HttpClient
        context.Services.AddHttpClient("YunXiao");

        // 注册 API Client
        context.Services.AddTransient<YunXiaoApiClient>();

        // 注册 MCP Server（Streamable HTTP 传输）
        context.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithTools<YunXiaoMcpTools>();
    }
}
