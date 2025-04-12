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

    public IList<FormFieldEntity> Fields { get; set; } = [];

    private string GetPrimaryKey()
    {
        string primaryFieldName = "f_id";
        var field = Fields?.FirstOrDefault(t => string.Equals(t.Name, primaryFieldName));

        return field.Name;
    }
}

public class FormFieldEntity
{
    public string Name { get; set; }
    public string TypeName { get; set; }
    public object Value { get; set; }
}