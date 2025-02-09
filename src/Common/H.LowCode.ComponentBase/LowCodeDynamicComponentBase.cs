using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.ComponentBase;

public abstract class LowCodeDynamicComponentBase : LowCodeComponentBase
{
    [Parameter]
    public EventCallback<object> ValueChanged { get; set; }

    protected virtual void GetRenderTreeBuilder(ComponentSchema component,
        RenderTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(component);

        Type componentType = Type.GetType(component.Fragment.FullTypeName, true);
        if(componentType == null)
            throw new NotSupportedException($"{component.Fragment.FullTypeName}");

        int index = 0;
        builder.OpenComponent(index++, componentType);

        //Parameters
        foreach (var attr in component.Fragment.Parameters)
        {
            if (attr.Name == "Value" && SupportsValueBinding(componentType))
            {
                builder.AddAttribute(1, "Value", attr.Value);
                builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create(this, (object newValue) =>
                {
                    attr.Value = newValue;
                    ValueChanged.InvokeAsync(newValue);
                }));
            }
            else if (attr.ValueType == ComponentValueTypeEnum.Integer)
                builder.AddComponentParameter(index++, attr.Name, attr.IntValue);
            else if (attr.ValueType == ComponentValueTypeEnum.String)
            {
                if ("{self}".Equals(attr.StringValue, StringComparison.OrdinalIgnoreCase))
                    builder.AddAttribute(index++, attr.Name, component);
                else
                    builder.AddComponentParameter(index++, attr.Name, attr.StringValue);
            }
            else
            {
                throw new NotSupportedException($"{attr.ValueType}");
            }
        }
        builder.CloseComponent();
    }

    private bool SupportsValueBinding(Type componentType)
    {
        // Check if the component has a "Value" parameter and a "ValueChanged" parameter
        var valueProperty = componentType.GetProperty("Value");
        var valueChangedProperty = componentType.GetProperty("ValueChanged");

        return valueProperty != null && valueChangedProperty != null;
    }
}
