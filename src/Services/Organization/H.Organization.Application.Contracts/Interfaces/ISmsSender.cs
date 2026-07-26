namespace H.Organization.Application.Contracts;

/// <summary>
/// 短信发送抽象（默认日志桩实现，真实网关可插拔替换）
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// 发送短信
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <param name="content">短信内容</param>
    Task SendAsync(string phone, string content);
}
