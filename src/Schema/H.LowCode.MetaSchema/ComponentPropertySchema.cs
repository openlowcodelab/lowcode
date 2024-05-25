using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    /// <summary>
    /// 组件属性
    /// </summary>
    public class ComponentPropertySchema : BaseMetaSchema
    {
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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsRequired { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool ReadOnly { get; set; }

        public string DefaultValue { get; set; }

        public string[] Enum { get; set; }

        public string[] EnumNames { get; set; }

        /// <summary>
        /// 支持的通用属性
        /// </summary>
        public IList<string> SupportProperties { get; set; }

        /// <summary>
        /// 扩展的简单属性
        /// </summary>
        public IList<PropertyItemSchema> ExtensionProperties { get; set; }

        /// <summary>
        /// 扩展的高级属性
        /// </summary>
        public RenderFragment RenderFragmentProperties { get; set; }

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