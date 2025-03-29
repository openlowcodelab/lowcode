using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public abstract class ComponentDataSourceSchemaBase
{
    /// <summary>
    /// 数据源分组类型
    /// </summary>
    [JsonPropertyName("dsgt")]
    public ComponentDataSourceGroupTypeEnum DataSourceGroupType { get; set; }

    /// <summary>
    /// 数据源类型
    /// </summary>
    [JsonPropertyName("dst")]
    public ComponentDataSourceTypeEnum DataSourceType { get; set; }

    [JsonPropertyName("dsid")]
    public string DataSourceId { get; set; }

    [JsonPropertyName("dsn")]
    public string DataSourceName { get; set; }

    [JsonPropertyName("dsv")]
    public string DataSourceValue { get; set; }

    /// <summary>
    /// 固定选项数据源
    /// </summary>
    [JsonPropertyName("fxopds")]
    public IList<OptionDataSourceSchema> FiexdOptionDataSource { get; set; }

    /// <summary>
    /// API选项数据源
    /// </summary>
    [JsonPropertyName("apiopds")]
    public APIDataSourceSchema APIOptionDataSource { get; set; }

    /// <summary>
    /// SQL选项数据源
    /// </summary>
    [JsonPropertyName("sqlopds")]
    public SQLDataSourceSchema SQLOptionDataSource { get; set; }
}
