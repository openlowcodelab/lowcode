using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using H.LowCode.MetaSchema;
using System.Reflection;
using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.DesignEngine.Abstraction;

public abstract class DesignEngineDynamicComponentBase : LowCodeComponentBase
{
    protected virtual RenderFragment RenderComponent(
        bool isSupportDataSource, ComponentDataSourceSchema dataSource,
        ComponentPartsFragmentSchema componentFragment)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new ArgumentNullException(nameof(componentFragment.TypeName));

        return builder =>
        {
            Type componentType = Type.GetType(componentFragment.TypeName, true);
            if (componentType == null)
                throw new NotSupportedException($"{componentFragment.TypeName}");

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

            //渲染 ChildContent
            if (isSupportDataSource)
            {
                RenderDataSource(dataSource, builder, index);
            }
            else
            {
                RenderChildrens(componentFragment, builder, index);
            }

            builder.CloseComponent();
        };
    }

    private void RenderDataSource(ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource == null)
            return;

        if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Option)
        {

            switch (dataSource.DataSourceType)
            {
                case ComponentDataSourceTypeEnum.Fiexd:
                    RenderOptionDataSource(dataSource, builder, index);
                    break;
                case ComponentDataSourceTypeEnum.SQL:
                    break;
                case ComponentDataSourceTypeEnum.API:
                    break;
                default:
                    break;
            }
        }
    }

    private void RenderOptionDataSource(ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.FiexdOptionDataSource != null && dataSource.FiexdOptionDataSource.Count > 0)
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                Type childComponentType = Type.GetType(dataSource.DataSourceFragmentType, true);
                foreach (var option in dataSource.FiexdOptionDataSource)
                {
                    childBuilder.OpenComponent(index++, childComponentType);

                    childBuilder.AddAttribute(index++, "Value", option.Value);
                    //childBuilder.AddContent(index++, option.Label);
                    childBuilder.AddAttribute(index++, "ChildContent", (RenderFragment)((cb) =>
                    {
                        cb.AddContent(index++, option.Label);
                    }));

                    childBuilder.CloseComponent();
                }
            }));
        }
    }

    private void RenderChildrens(ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        var childrens = componentFragment.Childrens;
        var hasChildren = childrens != null && childrens.Length > 0;
        if (hasChildren)
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                foreach (var child in componentFragment.Childrens)
                {
                    childBuilder.AddContent(index++, RenderComponent(false, null, child));
                }
            }));
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
