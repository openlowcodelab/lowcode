using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application.Contracts;

public class FormDataDTO : DataDTOBase
{
    public string Id { get; set; }

    public string Name { get; set; }

    public IDictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

    public IList<ValidationRuleSchema> ValidationRules { get; set; } = [];
}