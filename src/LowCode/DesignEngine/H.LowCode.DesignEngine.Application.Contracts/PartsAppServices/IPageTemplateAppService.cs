using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IPageTemplateAppService : IApplicationService
{
    Task<List<PageTemplateListModel>> GetListAsync();

    Task<PageTemplateSchema> GetByIdAsync(string templateId);

    Task<bool> SaveAsync(PageTemplateSchema pageTemplate);

    Task<bool> DeleteAsync(string templateId);
}
