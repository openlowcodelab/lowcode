using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 页面组件基类
/// </summary>
public abstract class LowCodePageComponentBase : LowCodeComponentBase
{
    protected static T GetQueryValue<T>(string name)
    {
        return default;
    }
}
