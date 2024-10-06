using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.ComponentBase;

public abstract class LowCodeLayoutComponentBase : Microsoft.AspNetCore.Components.LayoutComponentBase
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsDesignEngine { get; set; }

}
