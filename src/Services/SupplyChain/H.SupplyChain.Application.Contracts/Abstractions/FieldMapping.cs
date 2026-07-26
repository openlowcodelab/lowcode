using System.Text.Json;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 标准接口字段定义（描述标准接口包含哪些字段）。
/// RequestFieldsJson / ResponseFieldsJson 存储的是 <see cref="List{StandardField}"/> 的 JSON。
/// </summary>
public class StandardField
{
    /// <summary>标准字段名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>字段说明</summary>
    public string? Description { get; set; }

    /// <summary>数据类型（string/number/boolean/datetime/object/array）</summary>
    public string DataType { get; set; } = "string";

    /// <summary>是否必填</summary>
    public bool IsRequired { get; set; }

    /// <summary>示例值</summary>
    public string? Example { get; set; }
}

/// <summary>
/// 字段映射定义（请求参数映射 / 返回值字段映射的通用结构）。
/// RequestMappingJson / ResponseMappingJson 存储的是 <see cref="List{FieldMapping}"/> 的 JSON。
/// SourceField 为标准字段，TargetField 为供应商侧字段。
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

/// <summary>
/// 标准字段定义序列化/反序列化助手
/// </summary>
public static class StandardFieldHelper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>将标准字段列表序列化为 JSON</summary>
    public static string ToJson(List<StandardField>? fields) =>
        fields is null || fields.Count == 0
            ? "[]"
            : JsonSerializer.Serialize(fields, Options);

    /// <summary>从 JSON 反序列化为标准字段列表</summary>
    public static List<StandardField> FromJson(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? new List<StandardField>()
            : JsonSerializer.Deserialize<List<StandardField>>(json, Options) ?? new List<StandardField>();
}