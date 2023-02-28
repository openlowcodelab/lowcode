using H.Extensions.System;
using Microsoft.AspNetCore.Components;
using H.LowCode.Metadata;
using System.Text.Json.Serialization;

namespace H.LowCode.Metadata
{
    /// <summary>
    /// 组件属性（参考 JsonSchema）
    /// </summary>
    public class ComponentSettingSchema
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
        /// 组件宽度（默认 12）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ItemWidth { get; set; }

        /// <summary>
        /// 组件高度（默认 85 px）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ItemHeight { get; set; }

        /// <summary>
        /// 标签宽度（默认 180 px）
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int LabelWidth { get; set; }

        /// <summary>
        /// 自定义样式
        /// </summary>
        public string CustomStyle { get; set; }

        public IDictionary<string, string> ExtensionData { get; }

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
        Null
    }

    public class ComponentExtensionPropertySchema
    {
        public string Label { get; set; }

        public object Value { get; set; }
    }
}