using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace H.LowCode.ComponentBase;

public abstract class LowCodeDynamicComponentBase : LowCodeComponentBase
{
    /// <summary>
    /// 解析组件类型名为 Type。
    /// 解析失败返回 null（调用方应跳过该节点渲染，避免整页崩溃）。
    /// </summary>
    protected static Type ResolveComponentType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        var type = Type.GetType(typeName, throwOnError: false);
        if (type != null)
            return type;

        return null;
    }

    /// <summary>
    /// 组件属性渲染
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="index"></param>
    /// <param name="componentId"></param>
    /// <param name="componentType"></param>
    /// <param name="attributes"></param>
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

        if (string.IsNullOrEmpty(attr.AttributeName))
            throw new NullReferenceException($"{nameof(attr.AttributeName)} is empty");

        var propertyInfo = componentType.GetProperty(attr.AttributeName);
        if (propertyInfo == null)
            return;

        if (attr.AttributeValue == null)
        {
            Logger.LogWarning($"componentId={componentId}, {nameof(attr.AttributeValue)} is null");
        }

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

        if (string.IsNullOrEmpty(attr.AttributeClrType))
            return;

        var attrType = Type.GetType(attr.AttributeClrType, throwOnError: false);
        if (attrType == null)
            return;

        var realValue = attr.AttributeValue.ConvertToRealType(attrType);
        builder.AddAttribute(index++, attr.AttributeName, realValue);
    }

    private bool SupportsValueBinding(Type componentType)
    {
        // Check if the component has a "Value" parameter and a "ValueChanged" parameter
        var valueProperty = componentType.GetProperty("Value");
        var valueChangedProperty = componentType.GetProperty("ValueChanged");

        return valueProperty != null && valueChangedProperty != null;
    }
}
