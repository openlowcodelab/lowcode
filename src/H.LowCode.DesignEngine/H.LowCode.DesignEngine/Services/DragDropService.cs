using H.LowCode.Metadata;
using Microsoft.AspNetCore.Components.Web;
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

        public void Reset()
        {
            LastSelectedComponent = default;
            CurrentDragComponent = default;
            LastDragOverComponent = default;
        }

        public void DragItem_Add(IList<ComponentSchema> ComponentSchemas, ComponentSchema componentSchemas, bool isSelected = false)
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
            CurrentDragComponent.ComponentPropertySchema.Style = string.Empty;
            if (LastDragOverComponent != null)
                LastDragOverComponent.ComponentPropertySchema.Style = string.Empty;
        }

        public void DragItem_Effect_Move(DragEventArgs dragEventArgs)
        {
            dragEventArgs.DataTransfer.DropEffect = "move";
            
        }

        public void DragItem_Copy(DragEventArgs dragEventArgs)
        {
            dragEventArgs.DataTransfer.DropEffect = "copy";

        }
    }
}
