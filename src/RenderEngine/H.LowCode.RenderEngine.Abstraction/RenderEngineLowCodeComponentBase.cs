using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using H.LowCode.ComponentBase;
using AntDesign;

namespace H.LowCode.RenderEngine.Abstraction;

public abstract class RenderEngineLowCodeComponentBase : LowCodeComponentBase
{
    [Inject]
    protected new IMessageService Message { get; set; }
}
