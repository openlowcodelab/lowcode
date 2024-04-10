using System;

namespace H.LowCode.MetaSchema
{
    public class DataSourceSchema
    {
        public DataSourceType DataSourceType { get; set; }

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
