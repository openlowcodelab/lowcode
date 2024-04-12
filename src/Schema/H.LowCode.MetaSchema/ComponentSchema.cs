using H.Extensions.System;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class ComponentSchema : MetaSchema
    {
        public ComponentSchema(string ComponentType)
        {
            this.ComponentType = ComponentType;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 组件类型
        /// </summary>
        public string ComponentType { get; }

        public ComponentCategory ComponentCategory { get; set; } = ComponentCategory.Basic;

        public ComponentPropertySchema ComponentPropertySchema { get; set; }

        /// <summary>
        /// 是否隐藏标题
        /// </summary>
        public bool IsHiddenTitle { get; set; }

        public Guid ParentId { get; set; }

        /// <summary>
        /// 支持的属性
        /// </summary>
        public IList<string> SupportProperties { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IList<ComponentSchema> Childrens { get; set; } = [];

        #region JsonIgnore Attributes
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

        [JsonIgnore]
        public RenderFragment<ComponentSchema> RenderFragment { get; set; }

        [JsonIgnore]
        public Action Refresh { get; set; }
        #endregion

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

        public ComponentSchema DeepClone()
        {
            ComponentSchema component = ObjectExtension.DeepClone(this);
            component.Id = Guid.NewGuid();
            component.ParentId = Guid.Empty;
            component.RenderFragment = RenderFragment;
            component.Refresh = Refresh;
            return component;
        }

        public void RefreshState()
        {
            if (Refresh != null) Refresh();
        }
    }

    public enum ComponentCategory
    {
        Basic,
        Layout,
        Container
    }
}