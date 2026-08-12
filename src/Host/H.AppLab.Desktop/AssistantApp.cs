using Avalonia.Controls;
using H.Abp.HttpClientProxy;
using H.AppLab.Desktop.Services;
using H.AppLab.Desktop.ViewModels;
using H.AppLab.Desktop.Views;
using H.Assistant.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.AppLab.Desktop;

/// <summary>
/// Assistant 桌面应用插件（与 Web 端共用 Assistant 远程服务），
/// 以 "会话" 应用的形式接入 H.AppLab.Desktop 宿主外壳
/// </summary>
public sealed class AssistantApp : IDesktopApp
{
    public const string AssistantRemoteServiceName = "Assistant";

    public string Id => "assistant";

    public string Name => "会话";

    public string Icon => "💬";

    public string Description => "Agent 助手：会话";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // 远程服务配置（RemoteServices:Assistant:BaseUrl）
        services.AddRemoteServices(configuration);

        var allowInvalidCert = bool.TryParse(configuration["Http:AllowInvalidCertificates"], out var allow) && allow;
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
        services.AddSingleton<AssistantAppViewModel>();
        services.AddSingleton<ChatViewModel>();
        services.AddSingleton<TasksViewModel>();
        services.AddSingleton<KnowledgeViewModel>();
        services.AddSingleton<SettingsViewModel>();
    }

    public Control CreateView(IServiceProvider services)
    {
        return new AssistantAppView
        {
            DataContext = services.GetRequiredService<AssistantAppViewModel>()
        };
    }
}
