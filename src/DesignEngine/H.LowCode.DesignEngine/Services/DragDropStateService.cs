using H.LowCode.Metadata;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;

namespace H.LowCode.DesignEngine.Services
{
    /// <summary>
    /// 拖拽状态服务
    /// </summary>
    internal class DragDropStateService
    {
        /// <summary>
        /// 最后选中对象
        /// （当 DropItem 失去焦点时，即页面上没有任何项被选中，LastSelectedModel 仍有值）
        /// </summary>
        public ComponentSchema LastSelectedComponent { get; set; }

        /// <summary>
        /// 当前被拖拽对象
        /// </summary>
        public ComponentSchema CurrentDragComponent { get; set; }

        /// <summary>
        /// 最后一次拖拽到上面的对象
        /// </summary>
        public ComponentSchema LastDragOverComponent { get; set; }

        /// <summary>
        /// 最后一次拖拽到上面的对象的时间
        /// </summary>
        public DateTime LastDragOverTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 根 DropItemContainerSchema
        /// </summary>
        public DropItemContainerSchema RootDropItemContainerSchema { get; set; }

        /// <summary>
        /// 当前拖拽到的容器
        /// </summary>
        [Obsolete("ParentDropItemContainerSchema 不能作为全局对象")]
        public DropItemContainerSchema ParentDropItemContainerSchema { get; set; }
        
        public void ResetComponent()
        {
            LastSelectedComponent = default;
            CurrentDragComponent = default;
            LastDragOverComponent = default;
        }

        public void ResetComponentStyle()
        {
            //CurrentDragComponent.Opacity = 1;
            CurrentDragComponent.ComponentPropertySchema.Style = string.Empty;
            if (LastDragOverComponent != null)
                LastDragOverComponent.ComponentPropertySchema.Style = string.Empty;
        }
    }
}
