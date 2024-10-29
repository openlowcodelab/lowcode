using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Application.Contracts;

public interface ITableDataAppService
{
    Task<TableGetListOutput> GetList(TableGetListInput input);
}