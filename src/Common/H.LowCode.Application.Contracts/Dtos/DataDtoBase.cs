using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Application.Contracts;

public abstract class DataDtoBase
{
    public DateTime CreatedTime { get; set; }

    public string CreatedUser { get; set; }

    public DateTime ModifiedTime { get; set; }

    public string ModifiedUser { get; set; }
}