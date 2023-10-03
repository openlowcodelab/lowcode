using H.Extensions.System;
using Microsoft.AspNetCore.Components;
using H.LowCode.Metadata;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.Metadata
{
    public class ComponentSchema
    {
        public ComponentSchema(string ComponentType)
        {
            this.ComponentType = ComponentType;
        }

        /// <summary>
        /// 组件类型
        /// </summary>
        public string ComponentType { get; }

        public ComponentCategory ComponentCategory { get; set; } = ComponentCategory.Basic;

        public ComponentSettingSchema ComponentPropertySchema { get; set; }

        [JsonIgnore]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public double Opacity { get; set; } = 1;

        /// <summary>
        /// 是否由组件面板拖拽而来
        /// </summary>
        [JsonIgnore]
        public bool IsDroppedFromComponentPanel { get; set; }

        /// <summary>
        /// 拖拽到后面（true：后面  false：前面）
        /// </summary>
        [JsonIgnore]
        public bool IsDroppedToBack { get; set; }

        /// <summary>
        /// 支持的属性
        /// </summary>
        public IList<string> SupportProperties{get;set;}

        [JsonIgnore]
        public RenderFragment<ComponentSchema> RenderFragment { get; set; }

        /// <summary>
        /// 是否隐藏标题
        /// </summary>
        public bool IsHiddenTitle { get; set; }

        public ComponentContainerSchema ParentComponentContainerSchema { get; set; }

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

        public ComponentSchema Clone()
        {
            ComponentSchema componentSchema = this.DeepClone();
            componentSchema.RenderFragment = this.RenderFragment;
            return componentSchema;
        }
    }

    public enum ComponentCategory
    {
        Basic,
        Layout,
        Custom
    }
}