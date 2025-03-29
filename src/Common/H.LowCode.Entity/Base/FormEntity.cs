using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Entity;

public class FormEntity : EntityBase
{
    public string Id => GetPrimaryKey();

    public string Name { get; set; }

    public Dictionary<string, object> Fields { get; set; } = [];

    private string GetPrimaryKey()
    {
        string primaryFieldName = "f_id";
        var bol = Fields.ContainsKey(primaryFieldName);
        if (bol == false)
            return null;

        return Fields[primaryFieldName]?.ToString();
    }
}