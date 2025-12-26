using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

/// <summary>
/// 组件数据源分组类型
/// </summary>
public enum ComponentDataSourceGroupTypeEnum
{
    General = 0,
    Option = 1,
    Table = 2,
    Tree = 3,
    List = 4  // 列表循环数据源
}
