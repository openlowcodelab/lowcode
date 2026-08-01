namespace H.File.Application;

/// <summary>
/// MinIO 连接配置选项
/// </summary>
public class MinioOptions
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public bool UseSsl { get; set; }
    /// <summary>外部访问地址（用于生成预览/下载URL）</summary>
    public string? ExternalEndpoint { get; set; }
}
