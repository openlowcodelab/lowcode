using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class AppPublishAppService : ApplicationService, IAppPublishAppService
{
    private IAppPublishRepository _publishRepository => LazyServiceProvider.GetRequiredService<IAppPublishRepository>();
    private IAppRepository _appRepository => LazyServiceProvider.GetRequiredService<IAppRepository>();
    private IPageRepository _pageRepository => LazyServiceProvider.GetRequiredService<IPageRepository>();

    public async Task<List<AppPublishRecordSchema>> GetRecordsAsync(string appId)
    {
        return await _publishRepository.GetListAsync(appId);
    }

    public async Task<AppPublishRecordSchema> PublishAsync(string appId, string version, string description)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentException.ThrowIfNullOrEmpty(version);

        var app = await _appRepository.GetAsync(appId)
            ?? throw new BusinessException("应用不存在");

        var pages = await _pageRepository.GetListAsync(appId);

        var record = new AppPublishRecordSchema
        {
            AppId = appId,
            Version = version,
            Description = description,
            Status = AppPublishStatusEnum.Published,
            Operator = CurrentUser?.UserName ?? "system",
            PublishTime = DateTime.UtcNow,
            PageCount = pages?.Count ?? 0
        };
        await _publishRepository.SaveAsync(record);

        //更新应用发布状态与版本
        app.PublishStatus = PublishStatusEnum.Published;
        app.Version = version;
        await _appRepository.SaveAsync(app);

        return record;
    }

    public async Task<bool> RollbackAsync(string appId, string recordId)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);
        ArgumentException.ThrowIfNullOrEmpty(recordId);

        var records = await _publishRepository.GetListAsync(appId);
        var record = records.FirstOrDefault(t => t.Id == recordId)
            ?? throw new BusinessException("发布记录不存在");

        record.Status = AppPublishStatusEnum.Rollback;
        await _publishRepository.SaveAsync(record);

        //应用回退到开发状态
        var app = await _appRepository.GetAsync(appId);
        if (app != null)
        {
            app.PublishStatus = PublishStatusEnum.Development;
            await _appRepository.SaveAsync(app);
        }

        return true;
    }
}
