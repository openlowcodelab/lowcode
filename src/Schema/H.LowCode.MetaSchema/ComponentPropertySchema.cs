using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    /// <summary>
    /// 组件属性
    /// </summary>
    public class ComponentPropertySchema : BaseMetaSchema
    {
        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("t")]
        public string Title { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        [JsonPropertyName("max")]
        public double? Maximum { get; set; }

        [JsonPropertyName("min")]
        public double? Minimum { get; set; }

        [JsonPropertyName("maxlen")]
        public long? MaximumLength { get; set; }

        [JsonPropertyName("minlen")]
        public long? MinimumLength { get; set; }

        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }

        [JsonPropertyName("compvaltype")]
        public ComponentValueType ComponentValueType { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRequired { get; set; }

        [JsonPropertyName("readonly")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool ReadOnly { get; set; }

        [JsonPropertyName("default")]
        public string DefaultValue { get; set; }

        [JsonPropertyName("enum")]
        public string[] Enum { get; set; }

        [JsonPropertyName("enumnames")]
        public string[] EnumNames { get; set; }

        /// <summary>
        /// 支持的通用属性
        /// </summary>
        [JsonPropertyName("supportprops")]
        public IList<string> SupportProperties { get; set; }

        /// <summary>
        /// 扩展的简单属性
        /// </summary>
        [JsonPropertyName("extprops")]
        public IList<PropertyItemSchema> ExtensionProperties { get; set; }

        /// <summary>
        /// 扩展的高级属性
        /// </summary>
        [JsonPropertyName("renderfragprop")]
        public RenderFragment RenderFragmentProperties { get; set; }

        [JsonPropertyName("extdata")]
        public IDictionary<string, string> ExtensionData { get; } = new Dictionary<string, string>();

        #region method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual bool IsShowProperty(string propertyName)
        {
            if (SupportProperties == null)
                return false;

            if (SupportProperties.Contains(propertyName))
                return true;
            return false;
        }
        #endregion
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
        Date
    }
}