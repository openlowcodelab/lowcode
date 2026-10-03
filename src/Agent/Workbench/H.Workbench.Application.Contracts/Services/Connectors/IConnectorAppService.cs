using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 连接器管理服务接口
/// </summary>
public interface IConnectorAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<ConnectorDto>>> GetListAsync(ConnectorQueryDto input);
    Task<BaseOutput<ConnectorDto>> GetAsync(Guid id);

    /// <summary>
    /// 按员工绑定关系取连接器（运行时判定工具授予用）。不对外开 HTTP 入口：
    /// 传入任意 id 列表就能读到别人的连接器配置，而调用方本来就是同进程。
    /// </summary>
    Task<BaseOutput<List<ConnectorDto>>> ListByIdsAsync(List<Guid> ids);

    Task<BaseOutput<ConnectorDto>> CreateAsync(CreateConnectorDto input);
    Task<BaseOutput<ConnectorDto>> UpdateAsync(Guid id, UpdateConnectorDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
}
