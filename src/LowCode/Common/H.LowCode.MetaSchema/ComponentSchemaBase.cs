using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public abstract class ComponentSchemaBase : StateHasChangeSchema
{
    /// <summary>
    /// 组件实例Id
    /// </summary>
    /// <remarks>一个页面组件唯一Id</remarks>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("pid")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 组件Name
    /// </summary>
    [JsonPropertyName("n")]
    public string? Name { get; set; }

    /// <summary>
    /// 组件显示名称
    /// </summary>
    [JsonPropertyName("lb")]
    public string? Label { get; set; }

    /// <summary>
    /// 组件类型：1-原子组件  2-组合组件
    /// </summary>
    [JsonPropertyName("ct")]
    public int ComponentType { get; set; }

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

    /// <summary>
    /// 是否为内部容器组件
    /// </summary>
    [JsonPropertyName("incontainer")]
    public bool IsInnerContainer { get; set; }

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
    /// 事件
    /// </summary>
    [JsonPropertyName("evs")]
    public IList<EventSchema>? Events { get; set; }

    /// <summary>
    /// 事件消费
    /// </summary>
    [JsonPropertyName("evcs")]
    public IList<EventConsumeSchema>? EventConsumes { get; set; }

    /// <summary>
    /// 校验规则
    /// </summary>
    [JsonPropertyName("valrules")]
    public IList<ValidationRuleSchema>? ValidationRules { get; set; }

    /// <summary>
    /// 显示条件（条件为真时才渲染组件，用于组件显隐联动）
    /// </summary>
    [JsonPropertyName("vcond")]
    public VisibleConditionSchema? VisibleCondition { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    [JsonPropertyName("v")]
    public string Version { get; set; } = "0.0.1";
}