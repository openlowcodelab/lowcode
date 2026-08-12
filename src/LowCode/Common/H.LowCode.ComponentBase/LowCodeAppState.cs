namespace H.LowCode.ComponentBase;

public class LowCodeAppState
{
    public LowCodeAppState(bool isDesign)
    {
        IsDesign = isDesign;
    }

    /// <summary>
    /// 是否设计时
    /// </summary>
    public bool IsDesign { get; }
}
