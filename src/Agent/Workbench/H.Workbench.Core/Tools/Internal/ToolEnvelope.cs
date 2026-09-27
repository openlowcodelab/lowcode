using System.Text.Encodings.Web;
using System.Text.Json;

namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// 工具返回统一信封：<c>{success:true,data:…}</c> / <c>{success:false,error:…}</c>。
/// 失败必须以信封返回，否则 ToolExecutor 只能按"未抛异常"判定成功，
/// 模型会把错误文本当事实继续加工、轨迹也会把失败记成成功行。
/// </summary>
public static class ToolEnvelope
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public static string Ok(object? data) =>
        JsonSerializer.Serialize(new { success = true, data }, Options);

    public static string Fail(string error) =>
        JsonSerializer.Serialize(new { success = false, error }, Options);

    /// <summary>
    /// 判定工具返回是否为失败信封。无 success 字段的非 JSON 文本按成功处理
    /// （MCP 工具与自定义工具返回形态不受本仓库约束）。
    /// </summary>
    public static bool IsFailure(string? resultText, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(resultText)) return false;

        var trimmed = resultText.TrimStart();
        if (trimmed.Length == 0 || trimmed[0] != '{') return false;

        try
        {
            using var doc = JsonDocument.Parse(resultText);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!root.TryGetProperty("success", out var success)) return false;
            if (success.ValueKind != JsonValueKind.False) return false;

            error = root.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String
                ? e.GetString()
                : "工具返回失败";
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
