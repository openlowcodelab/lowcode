using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application.Contracts;

public class FormDataDTO : DTOBase
{
    public string FormId { get; set; }

    public IEnumerable<FieldDataDTO> Fields { get; set; } = [];

    public IDictionary<string, object> FormDatas { get; set; } = new Dictionary<string, object>();

    public IEnumerable<ValidationRuleSchema> ValidationRules { get; set; } = [];
}

public class FieldDataDTO
{
    public string Id { get; set; }

    public string Label { get; set; }

    public string Type { get; protected set; }

    public string Value { get; set; }
}