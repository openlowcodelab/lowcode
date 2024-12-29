using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.PartsMetaSchema;

/// <summary>
/// 组件设计状态
/// </summary>
/// <remarks>ComponentDesignStateSchema 用于记录 DesignEngine 设计器页面操作过程中的状态，无需持久化存储</remarks>
public class ComponentDesignStateSchema
{
    [JsonIgnore]
    public bool IsSelected { get; set; }

    [JsonIgnore]
    public double Opacity { get; set; } = 1;

    [JsonIgnore]
    public string DragEffectStyle { get; set; }

    /// <summary>
    /// 是否由组件面板拖拽而来
    /// </summary>
    [JsonIgnore]
    public bool IsDroppedFromComponentPanel { get; set; }

    /// <summary>
    /// 拖拽到后面（true：后面  false：前面）
    /// </summary>
    [JsonIgnore]
    public bool IsDropedAfter { get; set; }
}
