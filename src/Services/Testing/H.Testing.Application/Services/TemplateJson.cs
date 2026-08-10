using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace H.Testing.Application;

/// <summary>
/// 模板 JSON 读写辅助（兼容大小写混用的属性名）
/// </summary>
internal static class TemplateJson
{
    public static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// 读取 JSON 文件为对象数组；文件不存在或格式不符时返回空列表
    /// </summary>
    public static List<JsonObject> ReadArray(string path)
    {
        if (!File.Exists(path)) return new List<JsonObject>();
        var node = JsonNode.Parse(File.ReadAllText(path));
        return node is JsonArray arr ? arr.OfType<JsonObject>().ToList() : new List<JsonObject>();
    }

    /// <summary>
    /// 将 JSON 节点写入文件（自动创建目录，缩进且不转义中文）
    /// </summary>
    public static void Write(string path, JsonNode node)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, node.ToJsonString(WriteOptions));
    }

    /// <summary>
    /// 解析 JSON 字符串为对象数组
    /// </summary>
    public static JsonArray? ParseArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonNode.Parse(json) as JsonArray;
    }

    /// <summary>大小写不敏感地获取属性</summary>
    public static JsonNode? Get(JsonObject o, string name)
    {
        foreach (var kv in o)
        {
            if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)) return kv.Value;
        }
        return null;
    }

    public static string Str(JsonObject o, string name)
    {
        var v = Get(o, name);
        if (v is null) return string.Empty;
        try { return v.GetValue<string>(); }
        catch { return v.ToString(); }
    }

    public static int IntVal(JsonObject o, string name, int def = 0)
    {
        var v = Get(o, name);
        if (v is null) return def;
        try { return v.GetValue<int>(); }
        catch { return int.TryParse(v.ToString(), out var i) ? i : def; }
    }

    public static long LongVal(JsonObject o, string name, long def = 0)
    {
        var v = Get(o, name);
        if (v is null) return def;
        try { return v.GetValue<long>(); }
        catch { return long.TryParse(v.ToString(), out var l) ? l : def; }
    }

    public static bool BoolVal(JsonObject o, string name, bool def = false)
    {
        var v = Get(o, name);
        if (v is null) return def;
        try { return v.GetValue<bool>(); }
        catch { return bool.TryParse(v.ToString(), out var b) ? b : def; }
    }

    public static DateTime? DateVal(JsonObject o, string name)
    {
        var s = Str(o, name);
        return DateTime.TryParse(s, out var d) ? d : (DateTime?)null;
    }

    /// <summary>原样取出子节点的 JSON 字符串</summary>
    public static string? RawJson(JsonObject o, string name)
    {
        var v = Get(o, name);
        return v?.ToJsonString();
    }
}
