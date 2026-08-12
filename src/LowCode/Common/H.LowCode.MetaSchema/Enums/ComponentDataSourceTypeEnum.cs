namespace H.LowCode.MetaSchema;

/// <summary>
/// 组件数据源类型
/// </summary>
public enum ComponentDataSourceTypeEnum
{
    None = 0,
    DB = 1,
    API = 2,
    Option = 3,
    SQL = 6,
    Expression = 7, //表达式
    Fiexd = 8  //固定值
}