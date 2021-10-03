using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace H.LowCode.DesignerEngine.DesignPanel
{
    internal class DragDropService
    {
        public DragDropItem CurrentSelectItem { get; set; }

        public DragDropItem CurrentDragItem { get; set; }

        public DragDropItem DragTargetItem { get; set; }

        public IList<DragDropItem> Items { get; set; }

        public void Reset()
        {
            ShouldRender = true;
            DragTargetItem = default;
        }

        public bool ShouldRender { get; set; } = true;
    }
}
