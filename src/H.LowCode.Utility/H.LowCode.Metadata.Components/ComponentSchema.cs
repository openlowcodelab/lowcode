using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Schema;

namespace H.LowCode.Metadata.Components
{
    public class ComponentSchema
    {
        public JSchema ComponentJSchema { get; set; }

        public RenderFragment DropRenderFragment { get; set; }

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
        /// 组件属性
        /// </summary>
        public ComponentPropertySchema ComponentPropertySchema { get; set; }

        public ComponentSchema Clone()
        {
            if (ComponentPropertySchema == null)
                throw new NullReferenceException($"the property [{nameof(ComponentPropertySchema)}] is null");

            ComponentSchema clone = new()
            {
                ComponentJSchema = ComponentJSchema,
                DropRenderFragment = DropRenderFragment,
                ComponentType = ComponentType,
                ComponentPropertySchema = ComponentPropertySchema
            };
            return clone;
        }
    }
}