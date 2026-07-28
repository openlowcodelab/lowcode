namespace H.YunXiaoMcpServer;

public class YunXiaoOptions
{
    public const string SectionName = "YunXiao";

    /// <summary>
    /// 企业标识（organizationId），可在云效访问链接中获取
    /// </summary>
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// 个人访问令牌（Personal Access Token）
    /// </summary>
    public string PersonalAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 云效 API 端点
    /// </summary>
    public string Endpoint { get; set; } = "https://openapi-rdc.aliyuncs.com";
}
