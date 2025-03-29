using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using H.LowCode.MetaSchema;
using System.Reflection;
using H.LowCode.MetaSchema.RenderEngine;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.ComponentBase;
using AntDesign;
using System.ComponentModel;

namespace H.LowCode.RenderEngine.Abstraction;

public abstract class RenderEngineDynamicComponentBase : LowCodeDynamicComponentBase
{
    [Inject]
    protected new IMessageService Message { get; set; }

    protected virtual RenderFragment RenderComponent(
        string componentId, bool isSupportDataSource, ComponentDataSourceSchema dataSource,
        ComponentFragmentSchema componentFragment)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new NullReferenceException($"componentId={componentId}, {nameof(componentFragment.TypeName)}");

        return builder =>
        {
            Type componentType = Type.GetType(componentFragment.TypeName, true);
            if (componentType == null)
                throw new NullReferenceException($"componentId={componentId}, type={componentFragment.TypeName}");

            int index = 0;
            builder.OpenComponent(index++, componentType);

            //渲染属性
            RenderComponentAttributes(builder, index, componentId, componentType,
                componentFragment.Attributes);

            if (isSupportDataSource)
            {
                //渲染数据源
                RenderDataSource(componentId, dataSource, builder, index);
            }
            else
            {
                //渲染 ChildContent
                RenderChildrens(componentId, componentFragment, builder, index);
            }

            builder.CloseComponent();
        };
    }

    private void RenderDataSource(string componentId,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource == null)
            return;

        if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Option)
        {
            switch (dataSource.DataSourceType)
            {
                case ComponentDataSourceTypeEnum.Fiexd:
                    RenderOptionDataSource(componentId, dataSource, builder, index);
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

    private void RenderOptionDataSource(string componentId,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.FiexdOptionDataSource == null
            || dataSource.FiexdOptionDataSource.Count == 0)
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (string.IsNullOrEmpty(dataSource.DataSourceFragment.TypeName))
                throw new NullReferenceException($"componentId={componentId}, {nameof(dataSource.DataSourceFragment.TypeName)}");

            Type childComponentType = Type.GetType(dataSource.DataSourceFragment.TypeName, true);
            if (childComponentType == null)
                throw new NullReferenceException($"componentId={componentId}, type={dataSource.DataSourceFragment.TypeName}");

            foreach (var option in dataSource.FiexdOptionDataSource)
            {
                childBuilder.OpenComponent(index++, childComponentType);
                foreach (var fragAttr in dataSource.DataSourceFragment.Attributes)
                {
                    childBuilder.AddAttribute(index++, fragAttr.AttributeName, option.Value);
                }

                //childBuilder.AddContent(index++, option.Label);
                childBuilder.AddAttribute(index++, "ChildContent", (RenderFragment)((cb) =>
                {
                    cb.AddContent(index++, option.Label);
                }));

                childBuilder.CloseComponent();
            }
        }));
    }

    private void RenderChildrens(string componentId, ComponentFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        var childrens = componentFragment?.Childrens;
        if (childrens == null)
            return;

        if (childrens.Length == 0)
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            foreach (var child in componentFragment.Childrens)
            {
                childBuilder.AddContent(index++, RenderComponent(componentId, false, null, child));
            }
        }));
    }
}
