using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using H.LowCode.MetaSchema;
using System.Reflection;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.ComponentBase;

namespace H.LowCode.DesignEngine.Abstraction;

public abstract class DesignEngineDynamicComponentBase : LowCodeDynamicComponentBase
{
    protected virtual RenderFragment RenderComponent(
        string componentId, bool isSupportDataSource, ComponentPartsDataSourceSchema dataSource,
        ComponentPartsFragmentSchema componentFragment)
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

            //渲染 ChildContent
            if (isSupportDataSource)
            {
                RenderDataSource(componentId, dataSource, builder, index);
            }
            else
            {
                RenderChildrens(componentId, componentFragment, builder, index);
            }

            builder.CloseComponent();
        };
    }

    private void RenderDataSource(string componentId, 
        ComponentPartsDataSourceSchema dataSource,
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
        ComponentPartsDataSourceSchema dataSource,
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

    private void RenderChildrens(string componentId,
        ComponentPartsFragmentSchema componentFragment,
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
