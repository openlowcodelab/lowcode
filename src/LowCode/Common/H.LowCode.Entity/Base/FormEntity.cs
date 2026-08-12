namespace H.LowCode.Entity;

public class FormEntity : EntityBase
{
    public string Id => GetPrimaryKeyValue();

    public string Name { get; set; }

    public IList<FormFieldEntity> Fields { get; set; } = [];

    private string GetPrimaryKeyValue()
    {
        string primaryFieldName = "f_id";
        var field = Fields?.FirstOrDefault(t => string.Equals(t.Name, primaryFieldName));

        var id = field?.Value?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException($"primary key '{primaryFieldName}' value is null, fields={Fields?.ToJson()}");

        return id;
    }
}

public class FormFieldEntity
{
    public string Name { get; set; }

    public string TypeName { get; set; }

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