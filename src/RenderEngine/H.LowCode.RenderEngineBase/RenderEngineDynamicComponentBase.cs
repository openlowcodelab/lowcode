using AntDesign;
using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.RenderEngineBase;

public abstract class RenderEngineDynamicComponentBase : LowCodeDynamicComponentBase
{
    [CascadingParameter(Name = "pageCascading")]
    public PageCascadingModel PageCascading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected virtual RenderFragment RenderComponent(ComponentSchema component)
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
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
        ComponentFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new NullReferenceException($"componentId={componentId}, {nameof(componentFragment.TypeName)}");

        Type componentType = componentFragment.TypeName.ResolveType();
        if (componentType == null)
            throw new NullReferenceException($"componentId={componentId}, type={componentFragment.TypeName}");

        builder.OpenComponent(index++, componentType);

        //渲染属性
        RenderComponentAttributes(builder, index, componentId, componentType,
            componentFragment.Attributes);

        if (isSupportDataSource)
        {
            //渲染数据源
            RenderDataSource(componentId, component, dataSource, builder, index);
        }
        else if (componentFragment.HasChildren)
        {
            //渲染 ChildContent
            RenderChildFragments(componentId, component, componentFragment, builder, index);
        }

        builder.CloseComponent();
    }

    private void RenderDataSource(string componentId,
        ComponentSchema component,
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
        else if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Table)
        {
            builder.AddAttribute(index++, "DataSource", component.DataSource);
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

            Type childComponentType = dataSource.DataSourceFragment.TypeName.ResolveType();
            if (childComponentType == null)
                throw new NullReferenceException($"componentId={componentId}, type={dataSource.DataSourceFragment.TypeName}");

            foreach (var option in dataSource.FiexdOptionDataSource)
            {
                childBuilder.OpenComponent(index++, childComponentType);
                foreach (var fragAttr in dataSource.DataSourceFragment.Attributes)
                {
                    if (string.IsNullOrEmpty(fragAttr.AttributeName))
                        throw new NullReferenceException($"componentId={componentId}, {nameof(fragAttr.AttributeName)} is null");

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

    private void RenderChildFragments(string componentId,
        ComponentSchema component,
        ComponentFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        if (componentFragment.HasChildren == false)
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
}
