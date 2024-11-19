using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application.Contracts;

public abstract class DTOBase
{
    public DateTime CreatedTime { get; set; }

    public string CreatedUser { get; set; }

    public DateTime ModifiedTime { get; set; }

    public string ModifiedUser { get; set; }
}