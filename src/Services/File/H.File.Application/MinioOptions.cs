namespace H.File.Application;

/// <summary>
/// MinIO 连接配置选项
/// </summary>
public class MinioOptions
{
    public string Endpoint { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public bool UseSsl { get; set; }
    /// <summary>外部访问地址（用于生成预览/下载URL）</summary>
    public string? ExternalEndpoint { get; set; }
}
