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

    // List 数据操作
    MoveUp = 70, // 上移
    MoveDown = 80, // 下移
    CopyRow = 90, // 复制行

    // 表单/列表持久化操作
    SaveForm = 100, // 收集表单状态写入页面数据源
    SaveList = 110, // List 数据按保存映射持久化
    UpdateRow = 120, // 按事件参数更新当前行字段
    ShowDetail = 130, // 弹窗查看行数据详情
}