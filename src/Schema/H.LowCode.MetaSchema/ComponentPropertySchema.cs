using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    /// <summary>
    /// 组件属性
    /// </summary>
    public class ComponentPropertySchema : MetaSchema
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public double? Maximum { get; set; }

        public double? Minimum { get; set; }

        public long? MaximumLength { get; set; }

        public long? MinimumLength { get; set; }

        public string Pattern { get; set; }

        public ComponentValueType ComponentValueType { get; set; }

        public string Format { get; set; }

        public string Style { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRequired { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool ReadOnly { get; set; }

        public string DefaultValue { get; set; }

        public string[] Enum { get; set; }

        public string[] EnumNames { get; set; }

        /// <summary>
        /// 组件宽度（默认12：12/24*100 = 50%）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double ItemWidth { get; set; } = 12;

        /// <summary>
        /// 组件高度（默认 85 px）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double ItemHeight { get; set; } = 85;

        /// <summary>
        /// 标签宽度（默认 180 px）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double LabelWidth { get; set; } = 180;

        /// <summary>
        /// 自定义样式
        /// </summary>
        public string CustomStyle { get; set; }

        public IDictionary<string, string> ExtensionData { get; } = new Dictionary<string, string>();

        public IDictionary<string, ComponentExtensionPropertySchema> ExtensionProperties { get; set; }
    }

    public enum ComponentValueType
    {
        None,
        String,
        Number,
        Integer,
        Boolean,
        Object,
        Array,
        Date,
        Null
    }

    public class ComponentExtensionPropertySchema
    {
        public string Label { get; set; }

        public object Value { get; set; }
    }
}