using H.Extensions.System;
using Microsoft.AspNetCore.Components;
using H.LowCode.Schema;
using System.Text.Json.Serialization;

namespace H.LowCode.Schema
{
    public class ComponentPropertySchema
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

        public bool IsRequired { get; set; }

        public bool? ReadOnly { get; set; }

        public string DefaultValue { get; set; }

        /// <summary>
        /// 组件宽度
        /// </summary>
        public double ItemWidth { get; set; } = 12;

        /// <summary>
        /// 标签宽度
        /// </summary>
        public double LabelWidth { get; set; } = 180;

        /// <summary>
        /// 自定义样式
        /// </summary>
        public string CustomStyle { get; set; }

        public IDictionary<string, ComponentPropertySchema> Properties { get; }

        public IDictionary<string, string> ExtensionData { get; }
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
}