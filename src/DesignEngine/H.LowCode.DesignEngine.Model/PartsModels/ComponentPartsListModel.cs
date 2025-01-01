using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.DesignEngine.Model;

public class ComponentPartsListModel
{
    /// <summary>
    /// 组件库Id
    /// </summary>
    public string LibraryId { get; set; }

    [JsonPropertyName("compid")]
    public string ComponentId { get; set; }

    [JsonPropertyName("cn")]
    public string ComponentName { get; set; }

    [JsonPropertyName("lb")]
    public string Label { get; set; }

    /// <summary>
    /// 组件类型：1-原子组件  2-组合组件
    /// </summary>
    [JsonPropertyName("ct")]
    public int ComponentType { get; set; }

    /// <summary>
    /// 是否为容器组件
    /// </summary>
    [JsonPropertyName("container")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsContainer { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    public DateTime ModifiedTime { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }
}
