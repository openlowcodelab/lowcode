using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
