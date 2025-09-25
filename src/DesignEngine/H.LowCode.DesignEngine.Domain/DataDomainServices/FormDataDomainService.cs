using H.LowCode.Entity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public class FormDataDomainService : DomainService, IFormDataDomainService
{
    private IFormDataRepository _formDataRepository => LazyServiceProvider.GetRequiredService<IFormDataRepository>();
    private IPageDomainService _pageDomainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();

    public async Task<FormEntity> GetAsync(string appId, string pageId, string id)
    {
        var formPageSchema = await _pageDomainService.GetByIdAsync(appId, pageId);
        if (formPageSchema == null)
            throw new KeyNotFoundException($"page not found: appId={appId}, pageId={pageId}");

        string entityName = formPageSchema.DataSource.DataSourceValue;

        if (string.IsNullOrEmpty(id))
        {
            var defaultEntity = new FormEntity()
            {
                Name = entityName,
                Fields = formPageSchema.Components.Where(t => t.IsContainer == false)
                    .Select(t => new FormFieldEntity()
                    {
                        Name = t.Name,
                        TypeName = t.Fragment.ValueType,
                        Value = t.Fragment.GetDefaultValue()
                    }).ToList()
            };
            return defaultEntity;
        }

        var entity = await _formDataRepository.GetAsync(entityName, id);
        if (entity == null)
            throw new EntityNotFoundException($"Entity {entityName} Not Found: {id}");

        return entity;
    }

    public async Task<bool> SaveAsync(FormEntity entity)
    {
        //if (string.IsNullOrEmpty(entity.Id))
            await _formDataRepository.AddAsync(entity);
        //else
        //    await _formDataRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(string appId, string pageId, string id)
    {
        var formPageSchema = await _pageDomainService.GetByIdAsync(appId, pageId);

        return await _formDataRepository.DeleteAsync(formPageSchema.DataSource.DataSourceValue, id);
    }
}
