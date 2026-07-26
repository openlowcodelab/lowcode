using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 组件设计状态
/// </summary>
/// <remarks>ComponentDesignStateSchema 用于记录 DesignEngine 设计器页面操作过程中的状态，无需持久化存储</remarks>
public class ComponentDesignStateSchema
{
    [JsonIgnore]
    public bool IsSelected { get; set; }

    [JsonIgnore]
    public string? DragEffectStyle { get; set; }

    /// <summary>
    /// 是否由组件面板拖拽而来
    /// </summary>
    [JsonIgnore]
    public bool IsDroppedFromComponentPanel { get; set; }

    /// <summary>
    /// 动画变换样式（用于平滑让位动画）
    /// </summary>
    [JsonIgnore]
    public string AnimationTransform { get; set; } = string.Empty;

    /// <summary>
    /// 是否正在进行让位动画
    /// </summary>
    [JsonIgnore]
    public bool IsAnimating { get; set; }

    /// <summary>
    /// 是否显示拖拽放置指示线（当前拖拽经过的目标）
    /// </summary>
    [JsonIgnore]
    public bool ShowDropIndicator { get; set; }
}