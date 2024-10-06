using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.ComponentBase;

public abstract class LowCodeComponentBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Inject]
    LowCodeAppState lowCodeAppState { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsDesign
    {
        get
        {
            return lowCodeAppState.IsDesign;
        }
    }
}
