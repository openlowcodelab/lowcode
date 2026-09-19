using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

public interface IMcpServerAppService : IAppService
{
    Task<BaseOutput<List<McpServerDto>>> GetAllAsync();

    /// <summary>
    /// 获取含真实凭据的完整列表（仅限服务端进程内使用，不对外暴露 HTTP 端点）
    /// </summary>
    Task<BaseOutput<List<McpServerDto>>> GetRawListAsync();

    Task<BaseOutput<McpServerDto>> CreateAsync(CreateMcpServerDto input);
    Task<BaseOutput<McpServerDto>> UpdateAsync(Guid id, UpdateMcpServerDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
}
