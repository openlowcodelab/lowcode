using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppTemplateAppService : IApplicationService
{
    Task<List<AppTemplateListModel>> GetListAsync();

    Task<AppTemplateSchema> GetByIdAsync(string templateId);

    Task<bool> DeleteAsync(string templateId);

    /// <summary>
    /// 将已有应用另存为应用模板
    /// </summary>
    Task<bool> SaveFromAppAsync(string appId, string name, string description);

    /// <summary>
    /// 从模板创建新应用
    /// </summary>
    Task<AppPartsSchema> CreateAppFromTemplateAsync(string templateId, string newAppId, string newName);
}
