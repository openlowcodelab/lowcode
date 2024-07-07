using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class DataSourceSchema
    {
        [JsonPropertyName("type")]
        public DataSourceType DataSourceType { get; set; }

        [JsonPropertyName("vals")]
        public IDictionary<string, string> Values { get; set; }
    }

    public enum DataSourceType
    {
        Default,
        API,
        SQL,
        Expression, //表达式
        Fiexd  //固定值
    }
}
