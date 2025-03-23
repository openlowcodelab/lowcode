using H.Extensions.System;
using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public abstract class ComponentSchemaBase
{
    /// <summary>
    /// 组件实例Id
    /// </summary>
    /// <remarks>一个页面组件唯一Id</remarks>
    [JsonPropertyName("id")]
    public string Id { get; set; } = ShortIdGenerator.Generate();

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
    public bool IsHiddenLabel { get; set; }

    /// <summary>
    /// 是否为容器组件
    /// </summary>
    [JsonPropertyName("container")]
    public bool IsContainer { get; set; }

    private bool _isSupportDataSource;
    /// <summary>
    /// 是否支持数据源
    /// </summary>
    [JsonPropertyName("sptds")]
    public bool IsSupportDataSource
    {
        get
        {
            if (IsContainer)
                return false;
            return _isSupportDataSource;
        }
        set
        {
            if (IsContainer)
                _isSupportDataSource = false;
            else
                _isSupportDataSource = value;
        }
    }

    /// <summary>
    /// 组件样式
    /// </summary>
    [JsonPropertyName("stl")]
    public ComponentStyleSchema Style { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("ev")]
    public ComponentEventSchema Event { get; set; } = new();

    [JsonPropertyName("desc")]
    public string Description { get; set; }
}