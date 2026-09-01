using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试数据集服务（数据驱动测试）
/// </summary>
public class TestDatasetAppService : ApplicationService, ITestDatasetAppService
{
    private readonly IRepository<TestDatasetEntity, long> _repository;

    public TestDatasetAppService(IRepository<TestDatasetEntity, long> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<List<TestDatasetDto>>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            query.Where(e => e.ProjectId == projectId).OrderBy(e => e.Name));

        return new(list.Select(e =>
        {
            var dto = ToDto(e);
            dto.RowCount = dto.Rows.Count;
            dto.Rows = new List<Dictionary<string, string>>();
            return dto;
        }).ToList());
    }

    public async Task<BaseOutput<TestDatasetDto?>> GetByIdAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        return new(entity != null ? ToDto(entity) : null);
    }

    public async Task<BaseOutput<long>> CreateAsync(TestDatasetDto dto)
    {
        var entity = new TestDatasetEntity { ProjectId = dto.ProjectId };
        ApplyDto(entity, dto);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return new(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, TestDatasetDto dto)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        ApplyDto(entity, dto);
        await _repository.UpdateAsync(entity, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        await _repository.DeleteAsync(entity, autoSave: true);
        return new(true);
    }

    private static void ApplyDto(TestDatasetEntity entity, TestDatasetDto dto)
    {
        entity.Name = dto.Name;
        entity.ColumnsJson = JsonSerializer.Serialize(dto.Columns, TestingMappers.JsonOptions);
        entity.RowsJson = JsonSerializer.Serialize(dto.Rows, TestingMappers.JsonOptions);
    }

    private static TestDatasetDto ToDto(TestDatasetEntity entity)
    {
        var rows = string.IsNullOrWhiteSpace(entity.RowsJson)
            ? new List<Dictionary<string, string>>()
            : JsonSerializer.Deserialize<List<Dictionary<string, string>>>(entity.RowsJson, TestingMappers.JsonOptions)
                ?? new List<Dictionary<string, string>>();

        return new TestDatasetDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            CreationTime = entity.CreationTime,
            Columns = string.IsNullOrWhiteSpace(entity.ColumnsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.ColumnsJson, TestingMappers.JsonOptions) ?? new List<string>(),
            Rows = rows,
            RowCount = rows.Count
        };
    }
}
