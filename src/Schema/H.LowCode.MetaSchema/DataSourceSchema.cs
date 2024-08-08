using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class DataSourceSchema
    {
        [JsonPropertyName("type")]
        public DataSourceEnum DataSourceType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Value {  get; set; }

        [JsonPropertyName("vals")]
        public IDictionary<string, string> Values { get; set; }
    }

    public enum DataSourceEnum
    {
        Default,
        API,
        SQL,
        Expression, //表达式
        Fiexd  //固定值
    }
}
