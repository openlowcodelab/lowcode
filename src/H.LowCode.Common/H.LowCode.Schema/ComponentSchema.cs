using H.Extensions.System;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Schema;

namespace H.LowCode.Schema
{
    public class ComponentSchema
    {
        public string Name { get; set; }

        //public double? Maximum { get; set; }

        //public double? Minimum { get; set; }

        //public long? MaximumLength { get; set; }

        //public long? MinimumLength { get; set; }

        //public bool? ReadOnly { get; set; }
        
        //public string Title { get; set; }

        //public string Description { get; set; }
        
        //public string Format { get; set; }
        
        //public string Id { get; set; }

        //public IList<string> Required { get; }

        //public IDictionary<string, JSchema> Properties { get; }

        public JSchema ComponentJsonSchema { get; set; }

        public RenderFragment ComponentRenderFragment { get; set; }

        public ComponentCategory ComponentCategory { get; set; } = ComponentCategory.Basic;

        public string ComponentType { get; set; }

        public bool IsSelected { get; set; }

        public double Opacity { get; set; } = 1;

        public string Style { get; set; }

        /// <summary>
        /// 是否由组件面板拖拽而来
        /// </summary>
        public bool IsDroppedFromComponentPanel { get; set; }

        /// <summary>
        /// 拖拽到后面（true：后面  false：前面）
        /// </summary>
        public bool IsDroppedToBack { get; set; }

        /// <summary>
        /// 组件属性
        /// </summary>
        public ComponentPropertySchema ComponentPropertySchema { get; set; }

        public ComponentSchema Clone()
        {
            if (ComponentPropertySchema == null)
                throw new NullReferenceException($"the property [{nameof(ComponentPropertySchema)}] is null");

            ComponentSchema clone = new()
            {
                ComponentJsonSchema = ComponentJsonSchema.DeepClone(),
                ComponentRenderFragment = ComponentRenderFragment,
                ComponentType = ComponentType,
                ComponentPropertySchema = ComponentPropertySchema.DeepClone()
            };
            return clone;
        }
    }

    public enum ComponentCategory
    {
        Basic,
        Layout,
        Custom
    }
}