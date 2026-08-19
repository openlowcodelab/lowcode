using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

/// <summary>
/// List 循环数据源配置
/// </summary>
public class ListDataSourceSchema
{
    /// <summary>
    /// 固定数据源（用于设计时预览）
    /// </summary>
    [JsonPropertyName("fxdata")]
    public IList<Dictionary<string, object>>? FixedData { get; set; }

    /// <summary>
    /// API 数据源配置
    /// </summary>
    [JsonPropertyName("apids")]
    public APIDataSourceSchema? APIDataSource { get; set; }

    /// <summary>
    /// SQL 数据源配置
    /// </summary>
    [JsonPropertyName("sqlds")]
    public SQLDataSourceSchema? SQLDataSource { get; set; }

    /// <summary>
    /// 数据响应路径（用于提取数组数据，如 "data.list"）
    /// </summary>
    [JsonPropertyName("datapath")]
    public string? DataPath { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("orderby")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// 是否倒序
    /// </summary>
    [JsonPropertyName("orderdesc")]
    public bool OrderDesc { get; set; }

    /// <summary>
    /// 表数据源引用（加载来源，app 级表数据源 Id）
    /// </summary>
    [JsonPropertyName("tbdsid")]
    public string? TableDataSourceId { get; set; }

    /// <summary>
    /// 加载过滤映射（key: 表字段名, value: 表达式，如 $query(id)、$(item.f_x)）
    /// </summary>
    [JsonPropertyName("flts")]
    public IDictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// 保存目标表数据源 Id（可与加载来源不同）
    /// </summary>
    [JsonPropertyName("saveto")]
    public string? SaveToDataSourceId { get; set; }

    /// <summary>
    /// 保存模式
    /// </summary>
    [JsonPropertyName("savemode")]
    public ListSaveModeEnum SaveMode { get; set; } = ListSaveModeEnum.Upsert;

    /// <summary>
    /// 保存字段映射（key: 目标表字段名, value: 表达式）
    /// </summary>
    /// <remarks>表达式支持行数据 $(item.f_x)、表单值 $(form.key)、URL 参数 $query(x)、$(now) 等</remarks>
    [JsonPropertyName("savemap")]
    public IDictionary<string, string>? SaveMap { get; set; }
}

/// <summary>
/// List 数据保存模式
/// </summary>
public enum ListSaveModeEnum
{
    /// <summary>
    /// 按主键新增或更新
    /// </summary>
    Upsert = 0,

    /// <summary>
    /// 重新生成主键后新增（用于从模板另存等场景）
    /// </summary>
    InsertNew = 1
}
