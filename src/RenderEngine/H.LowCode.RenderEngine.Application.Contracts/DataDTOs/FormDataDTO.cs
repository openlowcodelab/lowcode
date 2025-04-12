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

    /// <summary>
    /// Value 值的类型
    /// </summary>
    /// <remarks>init 用于控制允许在 AutoMapper 场景赋值, 不允许外部赋值</remarks>
    public string TypeName { get; init; }

    private object _value;
    public object Value
    {
        get
        {
            if (string.IsNullOrEmpty(TypeName))
                return new InvalidDataException($"TypeName is null or empty, field={this.ToJson()}");

            var type = Type.GetType(TypeName);
            if (type == null)
                throw new InvalidDataException($"Type '{TypeName}' not found, field={this.ToJson()}");

            var realValue = _value.ConvertToRealType(type);
            return realValue;
        }
        set
        {
            _value = value;
        }
    }
}