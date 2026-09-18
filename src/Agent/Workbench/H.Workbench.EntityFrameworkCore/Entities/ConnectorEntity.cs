using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 连接器实体（集成外部应用、设备及其他系统）
/// </summary>
public class ConnectorEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 连接器标识（唯一，如 browser / computer）
    /// </summary>
    public string ConnectorKey { get; set; } = string.Empty;

    /// <summary>
    /// 连接器名称
    /// </summary>
    public string ConnectorName { get; set; } = string.Empty;

    /// <summary>
    /// 连接器描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 来源：Market(连接器市场)/Custom(自定义)
    /// </summary>
    public string Source { get; set; } = "Custom";

    /// <summary>
    /// 图标（emoji 或标识文本）
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 配置（JSON 格式，自定义连接器使用）
    /// </summary>
    public string? Config { get; set; }
}
