using System;

namespace H.LowCode.DesignEngineBase;

internal class DragDropElementDimensions
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }
    public double ContainerWidth { get; set; }
    public DragDropElementMargin Margin { get; set; }
    public double OffsetTop { get; set; }
    public double OffsetLeft { get; set; }
}

internal class DragDropElementMargin
{
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }
    public double Left { get; set; }
}