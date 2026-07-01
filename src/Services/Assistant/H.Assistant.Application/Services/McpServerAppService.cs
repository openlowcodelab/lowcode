using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using H.Assistant.EntityFrameworkCore;
using H.Assistant.Application.Contracts;

namespace H.Assistant.Application;

/// <summary>
/// MCP 服务管理实现
/// </summary>
public class McpServerAppService : ApplicationService, IMcpServerAppService
{
    private readonly IRepository<McpServerEntity, Guid> _repository;

    public McpServerAppService(IRepository<McpServerEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<List<McpServerDto>> GetAllAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Name));
        return entities.Select(MapToDto).ToList();
    }

    public async Task<McpServerDto> CreateAsync(CreateMcpServerDto input)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.Name == input.Name)))
        {
            throw new InvalidOperationException($"MCP 服务 '{input.Name}' 已存在");
        }

        var entity = new McpServerEntity
        {
            Name = input.Name,
            DisplayName = input.DisplayName,
            Endpoint = input.Endpoint,
            TransportType = input.TransportType,
            AuthToken = input.AuthToken,
            ApiKey = input.ApiKey,
            Headers = input.Headers,
            TimeoutSeconds = input.TimeoutSeconds,
            IsEnabled = input.IsEnabled
        };

        await _repository.InsertAsync(entity);
        return MapToDto(entity);
    }

    public async Task<McpServerDto> UpdateAsync(Guid id, UpdateMcpServerDto input)
    {
        var entity = await _repository.GetAsync(id);

        entity.DisplayName = input.DisplayName;
        entity.Endpoint = input.Endpoint;
        entity.TransportType = input.TransportType;
        entity.AuthToken = input.AuthToken;
        entity.ApiKey = input.ApiKey;
        entity.Headers = input.Headers;
        entity.TimeoutSeconds = input.TimeoutSeconds;
        entity.IsEnabled = input.IsEnabled;

        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task ToggleEnabledAsync(Guid id, bool isEnabled)
    {
        var entity = await _repository.GetAsync(id);
        entity.IsEnabled = isEnabled;
        await _repository.UpdateAsync(entity);
    }

    private static McpServerDto MapToDto(McpServerEntity entity)
    {
        return new McpServerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Endpoint = entity.Endpoint,
            TransportType = entity.TransportType,
            AuthToken = entity.AuthToken,
            ApiKey = entity.ApiKey,
            Headers = entity.Headers,
            TimeoutSeconds = entity.TimeoutSeconds,
            IsEnabled = entity.IsEnabled,
            CreationTime = entity.CreationTime
        };
    }
}
