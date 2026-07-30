using H.Abp.HttpClientProxy;
using H.Assistant.Application.Contracts;
using H.Assistant.Desktop.Services;
using H.Assistant.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.Assistant.Desktop;

/// <summary>
/// 客户端服务注册（与 Web 端共用 Assistant 远程服务）
/// </summary>
public static class ClientServices
{
    public const string AssistantRemoteServiceName = "Assistant";

    public static IServiceProvider Build()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // 远程服务配置（RemoteServices:Assistant:BaseUrl）
        services.AddRemoteServices(configuration);

        var allowInvalidCert = configuration.GetValue<bool>("Http:AllowInvalidCertificates");
        services.AddHttpClient(AssistantRemoteServiceName)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler { UseCookies = true };
                if (allowInvalidCert)
                {
                    // 本地开发环境允许自签名证书
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
                return handler;
            });

        // Assistant Contracts 全部 AppService 的 HTTP 代理（与 Blazor WASM 客户端一致）
        services.AddHttpClientProxies(typeof(AssistantApplicationContractsModule).Assembly, AssistantRemoteServiceName);

        // 客户端基础服务
        services.AddSingleton<ToastService>();
        services.AddSingleton<ChatStreamClient>();

        // ViewModel
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ChatViewModel>();
        services.AddSingleton<TasksViewModel>();
        services.AddSingleton<KnowledgeViewModel>();
        services.AddSingleton<SettingsViewModel>();

        return services.BuildServiceProvider();
    }
}
