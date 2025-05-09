using Microsoft.AspNetCore.Components;
using System;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.ComponentBase;
using H.Util.Ids;

namespace H.LowCode.DesignEngine.Abstraction;

public abstract class DesignEngineDynamicComponentBase : LowCodeDynamicComponentBase
{
    protected virtual RenderFragment RenderComponent(ComponentPartsSchema component)
        => builder =>
    {
        if (component == null || component.Fragment == null)
            throw new NullReferenceException($"{nameof(component)} or {nameof(component.Fragment)} is null");

        int index = 0;
        RenderComponentRecursive(component.Id, component.IsSupportDataSource,
            component, component.DataSource, component.Fragment, builder, index);
    };

    private void RenderComponentRecursive(
        string componentId, bool isSupportDataSource,
        ComponentPartsSchema component,
        ComponentPartsDataSourceSchema dataSource,
        ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        //TypeName 为空时，使用 DefaultTypeName
        if (string.IsNullOrEmpty(componentFragment.TypeName))
            componentFragment.TypeName = componentFragment.DefaultTypeName;

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new NullReferenceException($"componentId={componentId}, {nameof(componentFragment.TypeName)}");

        Type componentType = Type.GetType(componentFragment.TypeName, true);
        if (componentType == null)
            throw new NullReferenceException($"componentId={componentId}, type={componentFragment.TypeName}");

        builder.OpenComponent(index++, componentType);

        //渲染属性
        RenderComponentAttributes(builder, index, componentId, componentType,
            componentFragment.Attributes);

        //渲染 ChildContent
        if (isSupportDataSource)
        {
            RenderDataSource(componentId, dataSource, builder, index);
        }
        else if (componentFragment.HasChildFragment)
        {
            RenderChildFragments(componentId, component, componentFragment, builder, index);
        }
        else if (componentFragment.Content.IsNullOrWhiteSpace() == false)
        {
            RenderContent(componentId, component, componentFragment, builder, index);
        }

        builder.CloseComponent();
    }

    #region 渲染数据源
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
                    if(string.IsNullOrEmpty(fragAttr.AttributeName))
                        throw new NullReferenceException($"componentId={componentId}, {nameof(fragAttr.AttributeName)} is null");

                    childBuilder.AddAttribute(index++, fragAttr.AttributeName, option.Value);
                }

                childBuilder.AddAttribute(index++, "ChildContent", (RenderFragment)((cb) =>
                {
                    cb.AddContent(index++, option.Label);
                }));

                childBuilder.CloseComponent();
            }
        }));
    }
    #endregion

    #region 渲染子节点
    private void RenderChildFragments(string componentId,
        ComponentPartsSchema component,
        ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        if (componentFragment.HasChildFragment == false)
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            foreach (var childFragment in componentFragment.ChildFragments)
            {
                RenderComponentRecursive(componentId, false,
                    component, null, childFragment, childBuilder, index);
            }
        }));
    }
    #endregion

    #region 渲染 Content
    private void RenderContent(string componentId,
        ComponentPartsSchema component,
        ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        if (componentFragment.Content.IsNullOrWhiteSpace())
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (string.Equals(componentFragment.Content, "$(DropItemContainer)",
                StringComparison.OrdinalIgnoreCase))
            {
                var containerComponent =
                    RenderContainerComponent(component, $"container-{component.Id}-{index++}");

                childBuilder.OpenComponent<DropItemContainer>(index++);
                childBuilder.AddAttribute(index++, "ContainerComponent", containerComponent);
                childBuilder.CloseComponent();
            }
            else
            {
                childBuilder.AddMarkupContent(index++, componentFragment.Content);
            }
        }));
    }
    #endregion

    #region 渲染组件内的 DropItemContainer
    private ComponentPartsSchema RenderContainerComponent(ComponentPartsSchema component, string key, Action<ComponentPartsSchema> action = null)
    {
        var c = component.Childrens.FirstOrDefault(t =>
        {
            return false;
        });
        if (c != null)
            return c;

        var containerComponent = RenderChildContainerComponent(component, key);

        if (action != null) action(containerComponent);

        component.Childrens.Add(containerComponent);

        return containerComponent;
    }

    private ComponentPartsSchema RenderChildContainerComponent(ComponentPartsSchema component, string name)
    {
        var newComponent = new ComponentPartsSchema();
        newComponent.Id = ShortIdGenerator.Generate();
        newComponent.Refresh = component.Refresh;

        newComponent.Fragment = new();
        newComponent.Style = new() { DefaultStyle = "height:100%; width:100%;" };
        newComponent.IsHiddenLabel = true;

        newComponent.IsContainer = true;
        newComponent.ParentId = component.Id;

        return newComponent;
    }
    #endregion
}
