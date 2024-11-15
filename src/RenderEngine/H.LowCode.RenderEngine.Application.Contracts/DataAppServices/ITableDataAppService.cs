using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface ITableDataAppService : IApplicationService
{
    Task<TableGetListOutput> GetList(TableGetListInput input);
}