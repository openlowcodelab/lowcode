using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageTemplateAppService : IAppService
{
    Task<BaseOutput<List<PageTemplateListModel>>> GetListAsync();

    Task<BaseOutput<PageTemplateSchema>> GetByIdAsync(string templateId);

    Task<BaseOutput<bool>> SaveAsync(PageTemplateSchema pageTemplate);

    Task<BaseOutput<bool>> DeleteAsync(string templateId);
}
