using System.Text.Json;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 字段映射定义（请求参数映射 / 返回值字段映射的通用结构）。
/// RequestMappingJson / ResponseMappingJson 存储的是 <see cref="List{FieldMapping}"/> 的 JSON。
/// </summary>
public class FieldMapping
{
    /// <summary>源字段名称（标准字段）</summary>
    public string SourceField { get; set; } = string.Empty;

    /// <summary>目标字段名称（供应商侧字段）</summary>
    public string TargetField { get; set; } = string.Empty;

    /// <summary>是否必填</summary>
    public bool IsRequired { get; set; }

    /// <summary>默认值（源字段缺失时使用）</summary>
    public string? DefaultValue { get; set; }
}

/// <summary>
/// 字段映射序列化/反序列化助手
/// </summary>
public static class FieldMappingHelper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>将映射列表序列化为 JSON</summary>
    public static string ToJson(List<FieldMapping>? mappings) =>
        mappings is null || mappings.Count == 0
            ? "[]"
            : JsonSerializer.Serialize(mappings, Options);

    /// <summary>从 JSON 反序列化为映射列表</summary>
    public static List<FieldMapping> FromJson(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? new List<FieldMapping>()
            : JsonSerializer.Deserialize<List<FieldMapping>>(json, Options) ?? new List<FieldMapping>();
}