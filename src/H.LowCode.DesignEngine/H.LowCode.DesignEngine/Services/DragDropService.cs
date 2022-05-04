using H.LowCode.DesignEngine.PropertyPanel;
using H.LowCode.Metadata.Components;
using System.Collections.Generic;

namespace H.LowCode.DesignEngine.Services
{
    internal class DragDropService
    {
        /// <summary>
        /// 最后选中对象
        /// （当 DropItem 失去焦点时，即页面上没有任何项被选中，LastSelectedModel 仍有值）
        /// </summary>
        public ComponentSchema LastSelectedComponent { get; set; }

        /// <summary>
        /// 被拖拽对象
        /// </summary>
        public ComponentSchema CurrentDragComponent { get; set; }

        /// <summary>
        /// 最后一次拖拽到上面的对象
        /// </summary>
        public ComponentSchema LastDragOverComponent { get; set; }

        /// <summary>
        /// 页面属性
        /// </summary>
        public PagePropertySchema PagePropertySchema { get; set; } = new PagePropertySchema();

        public void Reset()
        {
            LastSelectedComponent = default;
            CurrentDragComponent = default;
            LastDragOverComponent = default;
        }

        public void DragItem_DragEnd(IList<ComponentSchema> ComponentSchemas, ComponentSchema componentSchemas, bool isSelected = false)
        {
            var dropComponentSchemas = componentSchemas.Clone();
            dropComponentSchemas.IsDroppedFromComponentPanel = true;
            if (isSelected)
            {
                dropComponentSchemas.IsSelected = isSelected;
                LastSelectedComponent = dropComponentSchemas;
            }
            ComponentSchemas.Add(dropComponentSchemas);
        }

        public void DropItem_DragEnd()
        {
            CurrentDragComponent.Opacity = 1;
            CurrentDragComponent.Style = string.Empty;
            LastDragOverComponent.Style = string.Empty;
        }
    }
}
