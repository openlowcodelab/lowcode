using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

namespace H.Assistant.Desktop;

/// <summary>
/// 共享值转换器
/// </summary>
public static class Converters
{
    /// <summary>开关轨道颜色：开 #52c41a / 关 #d9d9d9</summary>
    public static readonly IValueConverter ToggleTrackBrush =
        new FuncValueConverter<bool, IBrush>(on =>
            new SolidColorBrush(Color.Parse(on ? "#52c41a" : "#d9d9d9")));

    /// <summary>开关滑块位置：开靠右 / 关靠左</summary>
    public static readonly IValueConverter ToggleThumbAlignment =
        new FuncValueConverter<bool, HorizontalAlignment>(on =>
            on ? HorizontalAlignment.Right : HorizontalAlignment.Left);

    /// <summary>启用状态透明度：禁用任务卡片 0.55</summary>
    public static readonly IValueConverter EnabledOpacity =
        new FuncValueConverter<bool, double>(enabled => enabled ? 1.0 : 0.55);

    /// <summary>字符串颜色值转画刷</summary>
    public static readonly IValueConverter StringToBrush =
        new FuncValueConverter<string?, IBrush>(color =>
            new SolidColorBrush(Color.Parse(string.IsNullOrEmpty(color) ? "#8c8c8c" : color)));
}
