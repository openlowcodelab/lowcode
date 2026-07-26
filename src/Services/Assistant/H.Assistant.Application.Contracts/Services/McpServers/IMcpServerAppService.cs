using H.Abstractions;

namespace H.Assistant.Application.Contracts;

public interface IMcpServerAppService : IAppService
{
    Task<List<McpServerDto>> GetAllAsync();
    Task<McpServerDto> CreateAsync(CreateMcpServerDto input);
    Task<McpServerDto> UpdateAsync(Guid id, UpdateMcpServerDto input);
    Task DeleteAsync(Guid id);
    Task ToggleEnabledAsync(Guid id, bool isEnabled);
}
