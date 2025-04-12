using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application.Contracts;

public class FormDataDTO : DataDTOBase
{
    public string Name { get; set; }

    public IList<FormFieldDTO> Fields { get; set; }

    public IList<ValidationRuleSchema> ValidationRules { get; set; } = [];
}

public class FormFieldDTO
{
    public string Name { get; set; }

    public string TypeName { get; set; }

    public object Value { get; set; }
}