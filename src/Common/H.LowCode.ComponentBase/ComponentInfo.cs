using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.ComponentBase;

public class ComponentInfo
{
    public string FullTypeName { get; set; }

    public string Name { get; set; }

    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}