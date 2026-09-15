using H.Abp.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Application;

/// <summary>
/// 连接器管理服务实现
/// </summary>
public class ConnectorAppService : ApplicationService, IConnectorAppService
{
    private readonly IRepository<ConnectorEntity, Guid> _connectorRepository;

    public ConnectorAppService(IRepository<ConnectorEntity, Guid> connectorRepository)
    {
        _connectorRepository = connectorRepository;
    }

    public async Task<BaseOutput<PagedResultDto<ConnectorDto>>> GetListAsync(ConnectorQueryDto input)
    {
        var query = await _connectorRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.ConnectorName.Contains(input.Filter) || x.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Source))
        {
            query = query.Where(x => x.Source == input.Source);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Source).ThenByDescending(x => x.CreationTime)
                .Skip(input.SkipCount).Take(input.MaxResultCount));

        return new(new PagedResultDto<ConnectorDto>(totalCount, entities.Select(MapToDto).ToList()));
    }

    public async Task<BaseOutput<ConnectorDto>> GetAsync(Guid id)
    {
        var entity = await _connectorRepository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ConnectorEntity), id);
        }

        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<ConnectorDto>> CreateAsync(CreateConnectorDto input)
    {
        var entity = new ConnectorEntity
        {
            ConnectorKey = $"custom-{Guid.NewGuid():N}"[..18],
            ConnectorName = input.ConnectorName,
            Description = input.Description,
            Source = "Custom",
            Icon = input.Icon,
            Config = input.Config,
            IsEnabled = true
        };

        entity = await _connectorRepository.InsertAsync(entity);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<ConnectorDto>> UpdateAsync(Guid id, UpdateConnectorDto input)
    {
        var entity = await _connectorRepository.GetAsync(id);
        entity.ConnectorName = input.ConnectorName;
        entity.Description = input.Description;
        entity.Icon = input.Icon;
        entity.Config = input.Config;

        entity = await _connectorRepository.UpdateAsync(entity);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await _connectorRepository.GetAsync(id);
        if (entity.Source == "Market")
        {
            throw new InvalidOperationException("连接器市场条目不可删除，只能停用");
        }

        await _connectorRepository.DeleteAsync(id);
        return new();
    }

    public async Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled)
    {
        var entity = await _connectorRepository.GetAsync(id);
        entity.IsEnabled = isEnabled;
        await _connectorRepository.UpdateAsync(entity);
        return new();
    }

    private static ConnectorDto MapToDto(ConnectorEntity entity)
    {
        return new ConnectorDto
        {
            Id = entity.Id,
            ConnectorKey = entity.ConnectorKey,
            ConnectorName = entity.ConnectorName,
            Description = entity.Description,
            Source = entity.Source,
            Icon = entity.Icon,
            IsEnabled = entity.IsEnabled,
            Config = entity.Config,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }
}
