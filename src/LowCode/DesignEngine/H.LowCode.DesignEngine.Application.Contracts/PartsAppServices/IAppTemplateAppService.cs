using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppTemplateAppService : IAppService
{
    Task<BaseOutput<List<AppTemplateListModel>>> GetListAsync();

    Task<BaseOutput<AppTemplateSchema>> GetByIdAsync(string templateId);

    Task<BaseOutput<bool>> DeleteAsync(string templateId);

    /// <summary>
    /// 将已有应用另存为应用模板
    /// </summary>
    Task<BaseOutput<bool>> SaveFromAppAsync(string appId, string name, string description);

    /// <summary>
    /// 从模板创建新应用
    /// </summary>
    Task<BaseOutput<AppPartsSchema>> CreateAppFromTemplateAsync(string templateId, string newAppId, string newName);
}
