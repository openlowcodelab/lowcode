using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignerEngine.DragDrop
{
    public class DragDropItem
    {
        public Guid Guid { get; set; }

        public string Title { get; set; }

        public RenderFragment DropRenderFragment { get; set; }

        public string ComponentType { get; set; }

        public string Icon { get; set; }

        public string Style { get; set; }
    }
}
