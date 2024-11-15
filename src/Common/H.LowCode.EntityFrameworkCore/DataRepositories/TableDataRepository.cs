using H.LowCode.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository, IScopedDependency
{
    public TableDataRepository(LowCodeDbContext dbContext)
    {
    }
}
