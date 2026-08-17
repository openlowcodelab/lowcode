using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

public interface IMcpServerAppService : IAppService
{
    Task<BaseOutput<List<McpServerDto>>> GetAllAsync();
    Task<BaseOutput<McpServerDto>> CreateAsync(CreateMcpServerDto input);
    Task<BaseOutput<McpServerDto>> UpdateAsync(Guid id, UpdateMcpServerDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
}
