using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignerEngine.DesignPanel
{
    public class DragDropItem
    {
        public DragDropItem()
        {
            Guid = Guid.NewGuid();
        }

        public Guid Guid { get; private set; }

        public string Name {  get; set; }

        public string Title { get; set; }

        public RenderFragment DropRenderFragment { get; set; }

        public string ComponentType { get; set; }

        public bool IsSelected {  get; set; }

        public bool IsDropItem {  get; set; }

        public DragDropItem Clone()
        {
            DragDropItem clone = new DragDropItem();
            clone.IsDropItem = true;
            clone.Name = Name;
            clone.Title = Title;
            clone.DropRenderFragment = DropRenderFragment;
            clone.ComponentType = ComponentType;
            return clone;
        }
    }
}
