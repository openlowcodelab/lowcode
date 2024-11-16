using H.LowCode.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.EntityFrameworkCore;

public class FormDataRepository : IFormDataRepository
{
    public bool? IsChangeTrackingEnabled => true;

    public FormDataRepository()
    {

    }

    public Task<bool> Delete<TKey>(TKey id)
    {
        throw new NotImplementedException();
    }

    public Task<FormEntity> Get<TKey>(TKey id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Save(FormEntity entity)
    {
        throw new NotImplementedException();
    }
}
