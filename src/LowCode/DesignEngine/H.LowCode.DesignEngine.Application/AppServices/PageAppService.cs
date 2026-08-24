using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class PageAppService : ApplicationService, IPageAppService
{
    private IPageRepository _repository => LazyServiceProvider.GetRequiredService<IPageRepository>();
    private IComponentPartsAppService _componentPartsAppService => LazyServiceProvider.GetRequiredService<IComponentPartsAppService>();

    public async Task<BaseOutput<List<PageListModel>>> GetListAsync(string appId)
    {
        return new(await _repository.GetListAsync(appId));
    }

    public async Task<BaseOutput<PagePartsSchema>> GetByIdAsync(string appId, string pageId)
    {
        return new(await _repository.GetByIdAsync(appId, pageId));
    }

    /// <summary>
    /// 获取页面 Schema, 并合并组件定义中的属性
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public async Task<BaseOutput<PagePartsSchema>> GetByIdWithDefineAsync(string appId, string pageId)
    {
        var pageSchema = await _repository.GetByIdAsync(appId, pageId);

        //合并组件定义中的属性 (支持组件升级后原有组件获取最新特性)
        foreach (var component in pageSchema.Components)
        {
            await MergeComponentPartsDefineRecursive(component);
        }

        return new(pageSchema);
    }

    [DisableValidation]
    public async Task<BaseOutput<bool>> SaveAsync(PagePartsSchema pageSchema)
    {
        ArgumentNullException.ThrowIfNull(pageSchema);
        ArgumentException.ThrowIfNullOrEmpty(pageSchema.Id);

        await _repository.SaveAsync(pageSchema);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string appId, string pageId)
    {
        await _repository.DeleteAsync(appId, pageId);
        return new(true);
    }

    /// <summary>
    /// 递归合并组件定义中的属性
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    private async Task MergeComponentPartsDefineRecursive(ComponentPartsSchema component)
    {
        //组件定义 Schema（内联组件可能没有对应物料部件，如资源挂载点，找不到时跳过定义合并）
        ComponentPartsSchema? componentPartsDefine = null;
        try
        {
            componentPartsDefine = (await _componentPartsAppService.GetByIdAsync(component.LibraryId,
                component.PartsId)).Data;
        }
        catch
        {
            // 未找到对应物料部件：内联组件，跳过定义合并，保留实例自身的 frag/childs
        }

        if (componentPartsDefine != null)
        {
            //组件实例与组件定义合并,保证历史组件实例升级到最新组件特性
            component.MergeComponentPartsDefine(componentPartsDefine);
        }

        if (component.Childrens != null && component.Childrens.Count > 0)
        {
            foreach (var child in component.Childrens)
            {
                if (child.IsInnerContainer)
                    continue;

                await MergeComponentPartsDefineRecursive(child);
            }
        }
    }

    public async Task<BaseOutput<ComponentPartsSchema>> GetPageComponentAsync(string appId, string pageId, string componentId)
    {
        var page = (await GetByIdAsync(appId, pageId)).Data;
        if (page == null)
        {
            throw new BusinessException("Page not found.");
        }

        if (page.Components == null || page.Components.Count == 0)
        {
            throw new BusinessException("Page Component not found.");
        }

        var component = page.Components.FirstOrDefault(c => c.Id == componentId);
        if (component == null)
        {
            throw new BusinessException($"Component with ID {componentId} not found in page {pageId}.");
        }

        await MergeComponentPartsDefineRecursive(component);

        return new(component);
    }
}
