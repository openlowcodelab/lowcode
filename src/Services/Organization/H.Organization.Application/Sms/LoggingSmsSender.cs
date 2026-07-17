using H.Organization.Application.Contracts;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace H.Organization.Application;

/// <summary>
/// 短信发送默认实现（日志桩）：仅记录短信内容，便于联调。
/// 真实短信网关可实现 <see cref="ISmsSender"/> 并替换注册。
/// </summary>
public class LoggingSmsSender : ISmsSender, ITransientDependency
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phone, string content)
    {
        _logger.LogInformation("[短信发送] 手机号: {Phone} 内容: {Content}", phone, content);
        return Task.CompletedTask;
    }
}
