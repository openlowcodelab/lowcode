using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using H.Util.Ids;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class AppTemplateAppService : ApplicationService, IAppTemplateAppService
{
    private IAppTemplateRepository _templateRepository => LazyServiceProvider.GetRequiredService<IAppTemplateRepository>();
    private IAppRepository _appRepository => LazyServiceProvider.GetRequiredService<IAppRepository>();
    private IPageRepository _pageRepository => LazyServiceProvider.GetRequiredService<IPageRepository>();
    private IMenuRepository _menuRepository => LazyServiceProvider.GetRequiredService<IMenuRepository>();
    private IDataSourceRepository _dataSourceRepository => LazyServiceProvider.GetRequiredService<IDataSourceRepository>();

    public async Task<BaseOutput<List<AppTemplateListModel>>> GetListAsync()
    {
        return BaseOutput<List<AppTemplateListModel>>.Ok(await _templateRepository.GetListAsync());
    }

    public async Task<BaseOutput<AppTemplateSchema>> GetByIdAsync(string templateId)
    {
        return BaseOutput<AppTemplateSchema>.Ok(await _templateRepository.GetByIdAsync(templateId));
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string templateId)
    {
        return BaseOutput<bool>.Ok(await _templateRepository.DeleteAsync(templateId));
    }

    /// <summary>
    /// 将已有应用另存为应用模板
    /// </summary>
    [DisableValidation]
    public async Task<BaseOutput<bool>> SaveFromAppAsync(string appId, string name, string description)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var app = await _appRepository.GetAsync(appId)
            ?? throw new BusinessException($"应用不存在：{appId}");

        //页面
        var pages = new List<PagePartsSchema>();
        var pageList = await _pageRepository.GetListAsync(appId);
        foreach (var pageModel in pageList)
        {
            var page = await _pageRepository.GetByIdAsync(appId, pageModel.PageId);
            if (page != null)
                pages.Add(page);
        }

        //菜单（GetListAsync 返回树，快照存扁平列表）
        var menuTree = await _menuRepository.GetListAsync(appId);
        var menus = FlattenMenus(menuTree);

        //数据源
        var dataSources = (await _dataSourceRepository.GetListAsync(appId)).ToList();

        var template = new AppTemplateSchema
        {
            TemplateId = ShortIdGenerator.Generate(),
            Name = string.IsNullOrWhiteSpace(name) ? app.Name : name,
            Description = description,
            Icon = app.Icon,
            ThemeColor = app.ThemeColor,
            SupportPlatforms = app.SupportPlatforms,
            Order = 0,
            PublishStatus = 1,
            App = app,
            Pages = pages,
            Menus = menus,
            DataSources = dataSources
        };

        return BaseOutput<bool>.Ok(await _templateRepository.SaveAsync(template));
    }

    /// <summary>
    /// 从模板创建新应用
    /// </summary>
    [DisableValidation]
    public async Task<BaseOutput<AppPartsSchema>> CreateAppFromTemplateAsync(string templateId, string newAppId, string newName)
    {
        ArgumentException.ThrowIfNullOrEmpty(templateId);
        ArgumentException.ThrowIfNullOrEmpty(newAppId);

        var template = await _templateRepository.GetByIdAsync(templateId)
            ?? throw new BusinessException($"应用模板不存在：{templateId}");

        //应用元信息
        var newApp = Clone(template.App) ?? new AppPartsSchema { Id = newAppId };
        newApp.Id = newAppId;
        newApp.Name = string.IsNullOrWhiteSpace(newName) ? template.Name : newName;
        newApp.PublishStatus = PublishStatusEnum.Development;
        await _appRepository.SaveAsync(newApp);

        //页面（保留页面内部Id以维持菜单/事件引用，仅改 AppId，新应用目录隔离）
        foreach (var page in template.Pages ?? [])
        {
            var newPage = Clone(page);
            if (newPage == null) continue;
            newPage.AppId = newAppId;
            await _pageRepository.SaveAsync(newPage);
        }

        //菜单（扁平保存，清空运行期 Childrens）
        foreach (var menu in template.Menus ?? [])
        {
            var newMenu = Clone(menu);
            if (newMenu == null) continue;
            newMenu.AppId = newAppId;
            newMenu.Childrens = [];
            await _menuRepository.SaveAsync(newMenu);
        }

        //数据源
        foreach (var ds in template.DataSources ?? [])
        {
            var newDs = Clone(ds);
            if (newDs == null) continue;
            newDs.AppId = newAppId;
            await _dataSourceRepository.SaveAsync(newAppId, newDs);
        }

        return BaseOutput<AppPartsSchema>.Ok(newApp);
    }

    private static T Clone<T>(T source) where T : class
    {
        if (source == null) return null;
        var json = source.ToJson();
        return json.FromJson<T>();
    }

    private static List<MenuSchema> FlattenMenus(IList<MenuSchema> menus)
    {
        var result = new List<MenuSchema>();
        if (menus == null) return result;

        foreach (var menu in menus)
        {
            var clone = Clone(menu);
            if (clone == null) continue;

            var children = clone.Childrens;
            clone.Childrens = [];
            result.Add(clone);

            if (children != null && children.Count > 0)
                result.AddRange(FlattenMenus(children));
        }

        return result;
    }
}
