using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace H.LowCode.DesignerEngine.DesignPanel
{
    internal class DragDropService
    {
        /// <summary>
        /// 最后选中对象
        /// （当 DropItem 失去焦点时，即页面上没有任何项被选中，LastSelectedModel 仍有值）
        /// </summary>
        public DragDropModel LastSelectedModel { get; set; }

        /// <summary>
        /// 被拖拽对象
        /// </summary>
        public DragDropModel CurrentDragModel { get; set; }

        /// <summary>
        /// 拖拽目标对象
        /// </summary>
        public DragDropModel DragTargetModel { get; set; }

        public void Reset()
        {
            LastSelectedModel = default;
            CurrentDragModel = default;
            DragTargetModel = default;
        }

        public void DragItem_DragEnd(IList<DragDropModel> DropModels, DragDropModel model, bool isSelected = false)
        {
            var dropModel = model.Clone();
            dropModel.IsDropModel = true;
            if (isSelected)
            {
                dropModel.IsSelected = isSelected;
                LastSelectedModel = dropModel;
            }
            DropModels.Add(dropModel);
        }

        public void DropItem_DragEnd()
        {
            CurrentDragModel.Opacity = 1;
            CurrentDragModel.ModelStyle = string.Empty;
            DragTargetModel.ModelStyle = string.Empty;
        }
    }
}
