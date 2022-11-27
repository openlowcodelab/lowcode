using System;

namespace H.LowCode.Schema
{
    public class DataSource
    {
        public DataSourceType DataSourceType { get; set; }

        public IDictionary<string, string> Values { get; set; }
    }

    public enum DataSourceType
    {
        Database,
        Api
    }
}
