using H.LowCode.MetaSchema;
using System;

namespace H.LowCode.Application.Contracts;

public class FormDataDto : DataDtoBase
{
    public string Name { get; set; }

    public IList<FormFieldDto> Fields { get; set; }

    public IList<ValidationRuleSchema> ValidationRules { get; set; } = [];
}

public class FormFieldDto
{
    public string Name { get; set; }

    /// <summary>
    /// Value 值的类型
    /// </summary>
    /// <remarks>init 用于控制允许在 AutoMapper 场景赋值, 不允许外部赋值</remarks>
    public string TypeName { get; init; }

    private object? _value;
    public object? Value
    {
        get
        {
            if (string.IsNullOrEmpty(TypeName))
                throw new InvalidDataException($"TypeName is null or empty, field={this.ToJson()}");

            var type = Type.GetType(TypeName);
            if (type == null)
                throw new InvalidDataException($"Type '{TypeName}' not found, field={this.ToJson()}");

            try
            {
                var realValue = _value.ConvertToRealType(type);
                return realValue;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to convert value '{_value}' to type '{TypeName}', field={this.ToJson()}", ex);
            }
        }
        set
        {
            _value = value;
        }
    }
}