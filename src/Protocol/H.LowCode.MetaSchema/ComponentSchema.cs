using H.Extensions.System;
using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class ComponentSchema
{
    /// <summary>
    /// 组件实例Id
    /// </summary>
    /// <remarks>一个页面组件唯一Id</remarks>
    [JsonPropertyName("id")]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    /// <summary>
    /// 组件库Id
    /// </summary>
    [JsonPropertyName("libid")]
    public string LibraryId { get; set; }

    /// <summary>
    /// 组件Id
    /// </summary>
    /// <remarks>一类组件唯一Id</remarks>
    [JsonPropertyName("compid")]
    public string ComponentId { get; set; } = ShortIdGenerator.Generate();

    [JsonPropertyName("pid")]
    public string ParentId { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("lb")]
    public string Label { get; set; }

    /// <summary>
    /// 是否隐藏标题
    /// </summary>
    [JsonPropertyName("hlb")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsHiddenLabel { get; set; }

    [JsonPropertyName("desc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Description { get; set; }

    /// <summary>
    /// 是否为容器组件
    /// </summary>
    [JsonPropertyName("container")]
    public bool IsContainer { get; set; }

    /// <summary>
    /// 组件渲染 Fragment
    /// </summary>
    [JsonPropertyName("frag")]
    public ComponentFragmentSchema Fragment { get; set; }

    /// <summary>
    /// Property 分组
    /// </summary>
    [JsonPropertyName("pgroups")]
    public IList<ComponentPropertyGroupSchema> PropertyGroups { get; set; } = [];

    /// <summary>
    /// 组件样式
    /// </summary>
    [JsonPropertyName("stl")]
    public ComponentStyleSchema Style { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("ev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ComponentEventSchema Event { get; set; } = new();
    
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("childs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IList<ComponentSchema> Childrens { get; set; } = [];

    [JsonPropertyName("ds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ComponentDataSourceSchema DataSource { get; set; } = new();
}