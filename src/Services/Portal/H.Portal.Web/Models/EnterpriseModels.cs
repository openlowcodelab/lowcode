namespace H.Portal.Web;

/// <summary>
/// 企业信息（门户前端视图模型，通过 HttpClient 调用 /api/app/portal-enterprise/* 获取）
/// </summary>
public class EnterpriseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DatabaseMode { get; set; } = string.Empty;
    public bool IsActivated { get; set; }
    public int UserCount { get; set; }
}

/// <summary>
/// 创建企业请求（门户前端视图模型）
/// </summary>
public class CreateEnterpriseModel
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}
