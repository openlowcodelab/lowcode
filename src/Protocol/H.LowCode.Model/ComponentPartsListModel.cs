using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Model;

public class ComponentPartsListModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int Order { get; set; }

    public int PublishState { get; set; }

    public DateTime ModifiedTime { get; set; }
}
