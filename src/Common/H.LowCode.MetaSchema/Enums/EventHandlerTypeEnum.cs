using System;

namespace H.LowCode.MetaSchema;

public enum EventHandlerTypeEnum
{
    None = 0,
    Page = 10, //打开页面
    Data = 30, //操作数据
    Custom = 99, // 自定义事件
}

public enum EventPageHandlerTypeEnum
{
    None = 0,
    Modal = 11, // 弹窗
    Self = 12, // 当前页
    Blank = 13, // 空白页
}

public enum EventDataHandlerTypeEnum
{
    None = 0,
    Reload = 31, //重新加载
    Delete = 32, //删除指定数据
    DeleteSelection = 33  //删除选中(常用于列表)
}