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

    public bool IsNullable { get; set; }
}