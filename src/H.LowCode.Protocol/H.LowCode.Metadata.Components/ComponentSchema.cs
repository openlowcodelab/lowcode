using H.Extensions.System;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Schema;

namespace H.LowCode.Metadata.Components
{
    public class ComponentSchema
    {
        /// <summary>
        /// 
        /// </summary>
        public JSchema ComponentJsonSchema { get; set; }

        public RenderFragment ComponentRenderFragment { get; set; }

        public string Name { get; set; }

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
                ComponentJsonSchema = ObjectExtension<JSchema, JSchema>.DeepClone(ComponentJsonSchema),
                ComponentRenderFragment = ComponentRenderFragment,
                ComponentType = ComponentType,
                ComponentPropertySchema = ObjectExtension<ComponentPropertySchema, ComponentPropertySchema>.DeepClone(ComponentPropertySchema)
            };
            return clone;
        }
    }
}