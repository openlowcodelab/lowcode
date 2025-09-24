using H.LowCode.RenderEngine.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public interface ITableDataDomainService : IDomainService
{
    /// <summary>
    /// 获取表格数据列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>分页数据结果</returns>
    Task<TableGetListOutput> GetListAsync(TableGetListInput input);
}
