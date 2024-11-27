using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class ComponentDataSourceSchema
{
    [JsonPropertyName("dst")]
    public DataSourceTypeEnum DataSourceType { get; set; }

    [JsonPropertyName("dsid")]
    public string DataSourceId { get; set; }

    [JsonPropertyName("dsn")]
    public string DataSourceName { get; set; }

    [JsonPropertyName("dsv")]
    public string DataSourceValue { get; set; }
}
