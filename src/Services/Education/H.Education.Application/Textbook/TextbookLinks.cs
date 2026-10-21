using Volo.Abp;

namespace H.Education.Application.Textbook;

/// <summary>
/// 教材与课时资源的链接处理：外链会直接渲染成 a 标签的 href，因此只放行 http/https
/// </summary>
internal static class TextbookLinks
{
    /// <summary>公开教材仓库，PDF 按「学段/学科/版本/年级」目录存放</summary>
    private const string EbookRepoBase = "https://github.com/TapXWorld/ChinaTextbook/blob/master/";

    /// <summary>归一化外链，空串归为 null，非 http/https 直接拒绝</summary>
    public static string? NormalizeUrl(string? url, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var trimmed = url.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new UserFriendlyException($"{fieldName}需要以 http:// 或 https:// 开头的完整地址");
        }

        return trimmed;
    }

    /// <summary>
    /// 把仓库内的相对路径拼成电子课本页地址。逐段转义，路径里的空格与「·」才不会把链接打断
    /// </summary>
    public static string EbookUrl(string repoPath) =>
        EbookRepoBase + string.Join('/', repoPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));
}
