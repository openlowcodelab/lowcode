using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class SQLDataSourceSchema
{
    [JsonPropertyName("dt")]
    public string DbType { get; set; }

    [JsonPropertyName("sql")]
    public string Sql { get; set; }
}
