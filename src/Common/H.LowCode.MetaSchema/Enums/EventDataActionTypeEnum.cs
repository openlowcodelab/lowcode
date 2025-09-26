using System;

namespace H.LowCode.MetaSchema;

public enum EventDataActionTypeEnum
{
    None = 0,
    EditRow = 10, // 编辑行
    DeleteRow = 20, // 删除行
    SaveRow = 30, // 保存行
    CancelEdit = 40, // 取消编辑
    AddRow = 50, // 添加行
    RefreshData = 60, // 刷新数据
}