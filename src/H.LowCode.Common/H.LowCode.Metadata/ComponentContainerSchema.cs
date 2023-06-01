using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Metadata
{
    public class ComponentContainerSchema
    {
        public string Class { get; set; }

        public string Style { get; set; }

        public ComponentContainerSchema ParentComponentContainerSchema { get; set; }

        public IList<ComponentSchema> ComponentSchemas { get; set; } = new List<ComponentSchema>();

        public IList<ComponentContainerSchema> ChildComponentContainerSchema { get; set; }
    }
}
