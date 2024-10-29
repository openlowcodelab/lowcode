using H.LowCode.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

public class FormDataRepository : IFormDataRepository
{
    private LowCodeDbContext _dbContext;

    public FormDataRepository(LowCodeDbContext dbContext)
    {
        _dbContext = dbContext;
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

    public void Test()
    {
        _dbContext.Database.GetConnectionString();
    }
}
