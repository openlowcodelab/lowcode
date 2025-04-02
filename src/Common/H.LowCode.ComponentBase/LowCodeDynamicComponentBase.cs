using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using H.LowCode.MetaSchema;
using System.Reflection;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text.Json;

namespace H.LowCode.ComponentBase;

public abstract class LowCodeDynamicComponentBase : LowCodeComponentBase
{
    /// <summary>
    /// 组件属性渲染
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="index"></param>
    /// <param name="componentId"></param>
    /// <param name="componentType"></param>
    /// <param name="attributes"></param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    protected virtual void RenderComponentAttributes(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema[] attributes)
    {
        if (attributes == null || attributes.Length == 0)
            return;

        foreach (var attr in attributes)
        {
            RenderComponentAttribute(builder, index, componentId, componentType, attr);
        }
    }

    private void RenderComponentAttribute(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema attr)
    {
        ArgumentNullException.ThrowIfNull(attr);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(attr.AttributeName);

        var propertyInfo = componentType.GetProperty(attr.AttributeName);
        if (propertyInfo == null)
            return;

        if (propertyInfo.PropertyType == typeof(RenderFragment))
        {
            //渲染 RenderFragment 属性（如 ChildContent）
            builder.AddAttribute(index++, attr.AttributeName, (RenderFragment)(childBuilder =>
            {
                childBuilder.AddContent(index++, attr.AttributeValue?.ToString());
            }));
        }
        else if (propertyInfo.PropertyType == typeof(EventCallback))
        {
            //渲染事件回调属性
            var method = GetType().GetMethod(attr.AttributeValue?.ToString(), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                var delegateType = typeof(EventCallback);
                var eventCallback = Delegate.CreateDelegate(delegateType, this, method);
                builder.AddAttribute(index++, attr.AttributeName, eventCallback);
            }
        }
        else
        {
            //渲染简单属性
            RenderComponentSimpleAttribute(builder, index, componentId, componentType, attr);
        }
    }

    /// <summary>
    /// 渲染简单属性
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="index"></param>
    /// <param name="componentId"></param>
    /// <param name="componentType"></param>
    /// <param name="attr"></param>
    /// <exception cref="NullReferenceException"></exception>
    private void RenderComponentSimpleAttribute(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema attr)
    {
        //if (prop.Name == "Value" && SupportsValueBinding(componentType))
        //{
        //    builder.AddAttribute(index++, "Value", prop.Value);
        //    builder.AddAttribute(index++, "ValueChanged", EventCallback.Factory.Create(this, (object newValue) =>
        //    {
        //        prop.Value = newValue;
        //        ValueChanged.InvokeAsync(newValue);
        //    }));
        //}

        if (attr.AttributeClrType.IsNullOrEmpty())
            throw new NullReferenceException($"componentId={componentId}, {nameof(attr.AttributeClrType)}");

        var attrValue = ConvertValue(attr.AttributeClrType, attr.AttributeValue);
        if (attrValue != null)
        {
            builder.AddAttribute(index++, attr.AttributeName, attrValue);
        }
    }

    private static object ConvertValue(string typeName, object value)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var type = Type.GetType(typeName);
        if (value == null)
        {
            return type.GetDefaultValue();
        }

        if (value is JsonElement valueElement)
        {
            return typeName switch
            {
                "System.Int32" => valueElement.GetInt32(),
                "System.Boolean" => valueElement.GetBoolean(),
                "System.String" => valueElement.GetString(),
                "System.Double" => valueElement.GetDouble(),
                "System.Decimal" => valueElement.GetDecimal(),
                "System.DateTime" => valueElement.GetDateTime(),
                "System.Int64" => valueElement.GetInt64(),
                _ => throw new NotSupportedException($"Type '{typeName}' is not supported.")
            };
        }
        else
        {
            return type.GetDefaultValue();
        }
    }

    private bool SupportsValueBinding(Type componentType)
    {
        // Check if the component has a "Value" parameter and a "ValueChanged" parameter
        var valueProperty = componentType.GetProperty("Value");
        var valueChangedProperty = componentType.GetProperty("ValueChanged");

        return valueProperty != null && valueChangedProperty != null;
    }
}
