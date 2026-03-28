using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Configuration;

public class MetaOption
{
    public const string SectionName = "Meta";

    public string AppsFilePath { get; set; }

    public  string PartsFilePath { get; set; }
}
