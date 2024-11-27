using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.Domain;

public interface IFormDataRepository : IRepository
{
    Task<bool> AddAsync(FormEntity entity);

    Task<bool> UpdateAsync(FormEntity entity);

    Task<FormEntity> GetAsync(string entityName, string id);

    Task<bool> DeleteAsync(string entityName, string id);
}
