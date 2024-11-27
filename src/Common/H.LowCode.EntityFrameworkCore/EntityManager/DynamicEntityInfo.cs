using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

public class DynamicEntityInfo
{
    public string EntityName { get; set; }

    public string PrimaryKey { get; set; }

    public Type EntityType { get; set; }

    public bool EnableSoftDelete { get; set; }

    public IEnumerable<DynamicEntityField> Fields { get; set; }
}

public class DynamicEntityField
{
    public string Name { get; set; }

    /// <summary>
    /// 字段类型 (CLR 类型)
    /// </summary>
    public Type ClrType { get; set; }

    /// <summary>
    /// 最大长度
    /// </summary>
    /// <remarks>仅string类型字段有效</remarks>
    public int? MaxLength { get; set; }

    /// <summary>
    /// 是否 Unicode 字符
    /// </summary>
    /// <remarks>仅string类型字段有效</remarks>
    public bool? IsUnicode { get; set; }

    /// <summary>
    /// 精度(最大长度)
    /// </summary>
    /// <remarks>仅decimal类型字段有效</remarks>
    public int? Precision { get; set; }

    /// <summary>
    /// 范围(小数位数)
    /// </summary>
    /// <remarks>仅decimal类型字段有效</remarks>
    public int? Scale { get; set; }

    /// <summary>
    /// 是否可空
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public object DefaultValue { get; set; }

    /// <summary>
    /// 字段注释
    /// </summary>
    public string Comment { get; set; }
}