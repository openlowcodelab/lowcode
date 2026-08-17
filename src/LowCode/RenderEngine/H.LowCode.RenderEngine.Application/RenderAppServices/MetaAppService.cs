using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngine.Domain.Repositories;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

[RemoteService]
public class MetaAppService : ApplicationService, IMetaAppService
{
    private IMenuRepository _menuRepository => LazyServiceProvider.GetRequiredService<IMenuRepository>();
    private IPageRepository _pageRepository => LazyServiceProvider.GetRequiredService<IPageRepository>();

    public async Task<BaseOutput<IList<MenuSchema>>> GetMenusAsync(string appId)
    {
        return new(await _menuRepository.GetListAsync(appId));
    }

    public async Task<BaseOutput<PageSchema>> GetPageAsync(string appId, string pageId)
    {
        return new(await _pageRepository.GetAsync(appId, pageId));
    }

    /// <summary>
    /// 获取页面, 并将页面的组件定义合并到组件中
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public async Task<BaseOutput<PageSchema>> GetPageWithDefineAsync(string appId, string pageId)
    {
        var pageSchema = await _pageRepository.GetAsync(appId, pageId);

        if (pageSchema?.Components != null)
        {
            foreach (var component in pageSchema.Components)
            {
                if (component == null)
                    continue;

                component.MergeAttributeDefineToFragment();
            }
        }

        return new(pageSchema);
    }
}
