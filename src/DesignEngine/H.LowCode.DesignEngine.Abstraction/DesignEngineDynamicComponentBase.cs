using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using H.LowCode.MetaSchema;
using System.Reflection;
using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Abstraction;

public abstract class DesignEngineDynamicComponentBase : LowCodeComponentBase
{
    protected virtual RenderFragment RenderComponent(ComponentPartsFragmentSchema componentFragment)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

        return builder =>
        {
            Type componentType = Type.GetType(componentFragment.DefaultTypeName, true);
            if (componentType == null)
                throw new NotSupportedException($"{componentFragment.DefaultTypeName}");

            int index = 0;
            builder.OpenComponent(index++, componentType);

            //渲染属性
            foreach (var prop in componentFragment.Attributes)
            {
                var propertyInfo = componentType.GetProperty(prop.Name);
                if (propertyInfo == null)
                    continue;

                if (propertyInfo.PropertyType == typeof(RenderFragment))
                {
                    // 处理 RenderFragment 属性（如 ChildContent）
                    builder.AddAttribute(index++, prop.Name, (RenderFragment)(childBuilder =>
                    {
                        childBuilder.AddContent(index++, prop.Value.ToString());
                    }));
                }
                else if (propertyInfo.PropertyType == typeof(EventCallback))
                {
                    // 处理事件回调属性
                    var method = GetType().GetMethod(prop.Value.ToString(), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                    {
                        var delegateType = typeof(EventCallback);
                        var eventCallback = Delegate.CreateDelegate(delegateType, this, method);
                        builder.AddAttribute(index++, prop.Name, eventCallback);
                    }
                }
                else
                {
                    // 设置普通属性
                    //if (prop.Name == "Value" && SupportsValueBinding(componentType))
                    //{
                    //    builder.AddAttribute(index++, "Value", prop.Value);
                    //    builder.AddAttribute(index++, "ValueChanged", EventCallback.Factory.Create(this, (object newValue) =>
                    //    {
                    //        prop.Value = newValue;
                    //        ValueChanged.InvokeAsync(newValue);
                    //    }));
                    //}
                    //else
                    if (prop.ValueType == ComponentValueTypeEnum.Integer)
                        builder.AddAttribute(index++, prop.Name, prop.IntValue);
                    else if (prop.ValueType == ComponentValueTypeEnum.String)
                    {
                        //if ("{self}".Equals(prop.StringValue, StringComparison.OrdinalIgnoreCase))
                        //    builder.AddAttribute(index++, prop.Name, component);
                        //else
                            builder.AddAttribute(index++, prop.Name, prop.StringValue);
                    }
                    else
                    {
                        throw new NotSupportedException($"{prop.ValueType}");
                    }
                }
            }

            //渲染子组件
            var childrens = componentFragment.Childrens;
            var hasChildren = childrens != null && childrens.Length > 0;
            if (hasChildren)
            {
                builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
                {
                    foreach (var child in componentFragment.Childrens)
                    {
                        childBuilder.AddContent(index++, RenderComponent(child));
                    }
                }));
            }

            builder.CloseComponent();
        };
    }

    private bool SupportsValueBinding(Type componentType)
    {
        // Check if the component has a "Value" parameter and a "ValueChanged" parameter
        var valueProperty = componentType.GetProperty("Value");
        var valueChangedProperty = componentType.GetProperty("ValueChanged");

        return valueProperty != null && valueChangedProperty != null;
    }
}
