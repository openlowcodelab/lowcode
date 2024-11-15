using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

[RemoteService]
public class TableDataAppService : ApplicationService, ITableDataAppService
{
    public TableDataAppService(ITableDataDomainService tableDataDomainService)
    {

    }

    public Task<TableGetListOutput> GetList(TableGetListInput input)
    {
        throw new NotImplementedException();
    }
}
