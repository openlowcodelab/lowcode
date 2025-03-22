using H.Util.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class SQLDataSourceSchema
{
    [JsonPropertyName("dt")]
    public string DbType { get; set; }

    [JsonPropertyName("sql")]
    public string Sql { get; set; }
}
