using Volo.Abp;

namespace H.Education.Application.Textbook;

/// <summary>
/// 教材与课时资源的链接处理：外链会直接渲染成 a 标签的 href，因此只放行 http/https
/// </summary>
internal static class TextbookLinks
{
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
}
