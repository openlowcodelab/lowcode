using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace H.LowCode.Domain;

public interface IFormDataDomainService : IDomainService
{
    Task<bool> SaveAsync(FormEntity entity);

    Task<FormEntity> GetAsync(string appId, string pageId, string id);

    Task<bool> DeleteAsync(string appId, string pageId, string id);
}
