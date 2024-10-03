using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

/// <summary>
/// 数据源类型
/// </summary>
public enum DataSourceTypeEnum
{
    None = 0,
    Table = 1,
    API = 2,
    Option = 3,
    SQL = 6,
    Expression = 7, //表达式
    Fiexd = 8  //固定值
}
