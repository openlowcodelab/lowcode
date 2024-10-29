using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.Domain;

public interface IFormDataDomainService
{
    Task<bool> Save(FormEntity entity);

    Task<FormEntity> Get<TKey>(TKey id);

    Task<bool> Delete<TKey>(TKey id);
}
