using H.LowCode.DesignerEngine.PropertyPanel;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.DesignerEngine.DesignPanel
{
    public class DragDropModel
    {
        public DragDropModel()
        {
            Guid = Guid.NewGuid();
        }

        public Guid Guid { get; private set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public RenderFragment DropRenderFragment { get; set; }

        public string ComponentType { get; set; }

        public bool IsSelected { get; internal set; }

        public double Opacity { get; internal set; } = 1;

        public string ModelStyle { get; internal set; }

        /// <summary>
        /// 是否由组件面板拖拽而来
        /// </summary>
        public bool IsDropModel { get; set; }

        /// <summary>
        /// 组件属性
        /// </summary>
        public ComponentPropertyBaseModel ComponentPropertyModel { get; private set; } = new ComponentPropertyBaseModel();

        public DragDropModel Clone()
        {
            if(ComponentPropertyModel == null)
                throw new NullReferenceException($"the property [{nameof(ComponentPropertyModel)}] is null");

            DragDropModel clone = new DragDropModel();
            clone.Name = Name;
            clone.Title = Title;
            clone.DropRenderFragment = DropRenderFragment;
            clone.ComponentType = ComponentType;
            return clone;
        }
    }
}
