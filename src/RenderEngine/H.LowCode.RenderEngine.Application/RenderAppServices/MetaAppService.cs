using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngine.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

[RemoteService]
public class MetaAppService : ApplicationService, IMetaAppService
{
    private IMenuDomainService _menuDomainService => LazyServiceProvider.GetRequiredService<IMenuDomainService>();
    private IPageDomainService _pageDomainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();

    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        return await _menuDomainService.GetListAsync(appId);
    }

    public async Task<PageSchema> GetPageAsync(string appId, string pageId)
    {
        return await _pageDomainService.GetAsync(appId, pageId);
    }

    /// <summary>
    /// 获取页面, 并将页面的组件定义合并到组件中
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public async Task<PageSchema> GetPageWithDefineAsync(string appId, string pageId)
    {
        var pageSchema = await _pageDomainService.GetAsync(appId, pageId);

        if (pageSchema?.Components != null)
        {
            foreach (var component in pageSchema.Components)
            {
                if (component == null)
                    continue;

                component.MergeAttributeDefineToFragment();
            }
        }

        return pageSchema;
    }
}
