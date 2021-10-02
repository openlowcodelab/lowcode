using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace H.LowCode.DesignerEngine.DragDrop
{
    internal class DragDropService
    {
        public DragDropItem CurrentSelectItem { get; set; }

        public DragDropItem DragTargetItem { get; set; }

        public DragDropItem CurrentDragItem { get; set; }

        public IList<DragDropItem> Items { get; set; }

        public void Reset()
        {
            ShouldRender = true;
            CurrentDragItem = default;
            DragTargetItem = default;
        }

        public void Add(DragDropItem item)
        {
            Items.Add(item);
        }

        public bool ShouldRender { get; set; } = true;
    }
}
