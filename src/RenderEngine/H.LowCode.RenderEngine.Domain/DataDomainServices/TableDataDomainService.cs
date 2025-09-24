using H.LowCode.RenderEngine.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public class TableDataDomainService : DomainService, ITableDataDomainService
{
    private readonly ITableDataRepository _tableDataRepository;

    public TableDataDomainService(ITableDataRepository tableDataRepository)
    {
        _tableDataRepository = tableDataRepository;
    }

    /// <summary>
    /// 获取表格数据列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>分页数据结果</returns>
    public async Task<TableGetListOutput> GetListAsync(TableGetListInput input)
    {
        return await _tableDataRepository.GetListAsync(input);
    }
}
