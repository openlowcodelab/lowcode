using AntDesign;
using H.LowCode.ComponentBase;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.DesignEngineBase;

/// <summary>
/// 页面组件基类
/// </summary>
public abstract class DesignEngineLowCodePageComponentBase : LowCodePageComponentBase
{
    [Inject]
    protected new IMessageService Message { get; set; }
}
