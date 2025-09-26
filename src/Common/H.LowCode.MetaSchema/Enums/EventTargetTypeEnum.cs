using System;

namespace H.LowCode.MetaSchema;

public enum EventTargetTypeEnum
{
    None = 0,
    Page = 10, //打开页面
    Component = 30, //组件
    Data = 40, //数据操作
    Custom = 99, // 自定义事件
}

public enum EventPageHandlerTypeEnum
{
    None = 0,
    Refresh = 11, //刷新
    Modal = 12, // 弹窗
    Self = 13, // 当前页
    Blank = 14, // 空白页
}

public enum EventCustomLanguageEnum
{
    None = 0,
    JavaScript = 1, // JavaScript
    Python = 2, // Python
    CSharp = 3, // C#
}