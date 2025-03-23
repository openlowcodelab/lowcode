using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using H.LowCode.ComponentBase;
using AntDesign;

namespace H.LowCode.DesignEngine.Abstraction;

public abstract class DesignEngineLowCodeComponentBase : LowCodeComponentBase
{
    [Inject]
    protected new IMessageService Message { get; set; }
}
