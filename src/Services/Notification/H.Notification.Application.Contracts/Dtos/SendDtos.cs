namespace H.Notification.Application.Contracts;

/// <summary>
/// 触发通知输入
/// </summary>
public class SendNotificationInput
{
    /// <summary>
    /// 业务编码
    /// </summary>
    public string BusinessCode { get; set; } = string.Empty;

    /// <summary>
    /// 通知级别（为空时使用业务默认级别）
    /// </summary>
    public NotificationLevel? Level { get; set; }

    /// <summary>
    /// 模板变量数据（用于 {{key}} 占位符替换）
    /// </summary>
    public Dictionary<string, string> Data { get; set; } = new();

    /// <summary>
    /// 指定通知人（为空时使用业务绑定的通知人）
    /// </summary>
    public List<Guid>? RecipientIds { get; set; }
}

/// <summary>
/// 测试发送输入
/// </summary>
public class TestSendInput
{
    public string BusinessCode { get; set; } = string.Empty;
    public NotificationLevel? Level { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();

    /// <summary>
    /// 指定测试通知人（为空时使用业务绑定的通知人）
    /// </summary>
    public List<Guid>? RecipientIds { get; set; }
}

/// <summary>
/// 发送结果
/// </summary>
public class SendNotificationResult
{
    /// <summary>
    /// 生成的通知消息ID
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// 投递总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string? Message { get; set; }
}
