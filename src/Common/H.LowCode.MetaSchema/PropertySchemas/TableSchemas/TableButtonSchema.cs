using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class TableButtonSchema
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("n")]
    public string? Name { get; set; }

    [JsonPropertyName("t")]
    public string? Title { get; set; }

    [JsonPropertyName("bt")]
    public int ButtonType { get; set; }

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 列顺序
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 组件支持的事件
    /// </summary>
    [JsonPropertyName("sptevs")]
    public string[] SupportEvents { get; set; } = ["OnClick"];

    /// <summary>
    /// 事件
    /// </summary>
    [JsonPropertyName("evs")]
    public IList<EventSchema>? Events { get; set; }

    public void Update(TableButtonSchema update)
    {
        this.Name = update.Name;
        this.Title = update.Title;
        this.ButtonType = update.ButtonType;
        this.Disabled = update.Disabled;
        this.Order = update.Order;
    }
}
