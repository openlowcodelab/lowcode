using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application;

public class TableDataAppService : ITableDataAppService
{
    public TableDataAppService(ITableDataDomainService tableDataDomainService)
    {

    }

    public Task<TableGetListOutput> GetList(TableGetListInput input)
    {
        throw new NotImplementedException();
    }
}
