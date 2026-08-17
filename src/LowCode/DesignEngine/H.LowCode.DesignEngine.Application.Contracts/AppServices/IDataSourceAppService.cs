using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IDataSourceAppService : IAppService
{
    Task<BaseOutput<IList<DataSourceListModel>>> GetListAsync(string appId, DataSourceInput input);

    Task<BaseOutput<DataSourceSchema>> GetByIdAsync(string appId, string id);

    Task<BaseOutput<bool>> SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task<BaseOutput<bool>> DeleteAsync(string appId, string id);
}