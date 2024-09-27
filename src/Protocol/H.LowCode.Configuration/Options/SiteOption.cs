using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Configuration;

public class SiteOption
{
    public const string SectionName = "Sites";

    public string AppId { get; set; }

    public string SiteUrl { get; set; }
}
