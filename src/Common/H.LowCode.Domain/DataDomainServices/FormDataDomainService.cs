using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace H.LowCode.Domain;

public class FormDataDomainService : DomainService, IFormDataDomainService
{
    private IFormDataRepository _formDataRepository => LazyServiceProvider.GetRequiredService<IFormDataRepository>();
    private IPageDomainService _pageDomainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();

    public async Task<FormEntity> GetAsync(string appId, string pageId, string id)
    {
        var formPageSchema = await _pageDomainService.GetAsync(appId, pageId);
        if (formPageSchema == null)
            throw new KeyNotFoundException($"page not found: appId={appId}, pageId={pageId}");

        string entityName = formPageSchema.DataSource.DataSourceValue;

        var defaultEntity = new FormEntity()
        {
            Name = entityName,
            Fields = formPageSchema.Components.ToDictionary(key => key.ComponentProperty.Name, c => c.ComponentProperty.GetDefaultValue())
        };
        if (string.IsNullOrEmpty(id))
            return defaultEntity;

        var entity = await _formDataRepository.GetAsync(entityName, id);
        if (entity == null)
            return defaultEntity;

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
        var formPageSchema = await _pageDomainService.GetAsync(appId, pageId);

        return await _formDataRepository.DeleteAsync(formPageSchema.DataSource.DataSourceValue, id);
    }
}
