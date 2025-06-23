using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class PageAppService : ApplicationService, IPageAppService
{
    private IPageDomainService _domainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();
    private IComponentPartsAppService _componentPartsAppService => LazyServiceProvider.GetRequiredService<IComponentPartsAppService>();

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        return await _domainService.GetListAsync(appId);
    }

    public async Task<PagePartsSchema> GetByIdAsync(string appId, string pageId)
    {
        return await _domainService.GetByIdAsync(appId, pageId);
    }

    /// <summary>
    /// 获取页面 Schema, 并合并组件定义中的属性
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public async Task<PagePartsSchema> GetByIdWithDefineAsync(string appId, string pageId)
    {
        var pageSchema = await _domainService.GetByIdAsync(appId, pageId);

        //合并组件定义中的属性
        foreach (var component in pageSchema.Components)
        {
            await MergeComponentPartsDefineRecursive(component);
        }

        return pageSchema;
    }

    public async Task<bool> SaveAsync(PagePartsSchema pageSchema)
    {
        ArgumentNullException.ThrowIfNull(pageSchema);
        ArgumentException.ThrowIfNullOrEmpty(pageSchema.Id);

        await _domainService.SaveAsync(pageSchema);
        return true;
    }

    public async Task<bool> DeleteAsync(string appId, string pageId)
    {
        await _domainService.DeleteAsync(appId, pageId);
        return true;
    }

    /// <summary>
    /// 递归合并组件定义中的属性
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    private async Task MergeComponentPartsDefineRecursive(ComponentPartsSchema component)
    {
        //组件定义 Schema
        var componentPartsDefine = await _componentPartsAppService.GetByIdAsync(component.LibraryId,
            component.ComponentId);

        //组件实例与组件定义合并,保证历史组件实例升级到最新组件特性
        component.MergeComponentPartsDefine(componentPartsDefine);

        if (component.Childrens != null && component.Childrens.Count > 0)
        {
            foreach (var child in component.Childrens)
            {
                await MergeComponentPartsDefineRecursive(child);
            }
        }
    }

    public async Task<ComponentPartsSchema> GetPageComponentAsync(string appId, string pageId, string componentId)
    {
        var page = await GetByIdAsync(appId, pageId);
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

        return component;
    }
}
