using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 连接器管理服务接口
/// </summary>
public interface IConnectorAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<ConnectorDto>>> GetListAsync(ConnectorQueryDto input);
    Task<BaseOutput<ConnectorDto>> GetAsync(Guid id);
    Task<BaseOutput<ConnectorDto>> CreateAsync(CreateConnectorDto input);
    Task<BaseOutput<ConnectorDto>> UpdateAsync(Guid id, UpdateConnectorDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
}
