namespace H.LowCode.MetaSchema;

public class TableColumnSchema
{
    public string Name { get; set; }

    public string Title { get; set; }

    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 列顺序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否支持过滤
    /// </summary>
    public bool Filterable { get; set; }

    /// <summary>
    /// 是否支持排序
    /// </summary>
    public bool Sortable { get; set; }
}
