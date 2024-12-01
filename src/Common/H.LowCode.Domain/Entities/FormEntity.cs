using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Domain;

public class FormEntity : EntityBase
{
    public string Name { get; set; }

    public Dictionary<string, object> Fields { get; set; } = [];
}