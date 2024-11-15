using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Domain;

public class TableDataDomainService : ITableDataDomainService, IScopedDependency
{
    public TableDataDomainService(ITableDataRepository tableDataRepository)
    {

    }
}
