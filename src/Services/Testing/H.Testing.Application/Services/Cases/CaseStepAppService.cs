using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试用例步骤服务（数据库存储）
/// </summary>
public class CaseStepAppService : ApplicationService, ICaseStepAppService
{
    private readonly IRepository<CaseStepEntity, long> _repository;

    public CaseStepAppService(IRepository<CaseStepEntity, long> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<List<CaseStepDto>>> GetByCaseIdAsync(long caseId)
    {
        var query = await _repository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(
            query.Where(s => s.CaseId == caseId).OrderBy(s => s.Order).ThenBy(s => s.Id));
        return new(steps.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<Dictionary<long, List<CaseStepDto>>>> GetByCaseIdsAsync(IEnumerable<long> caseIds)
    {
        var ids = caseIds.ToList();
        if (ids.Count == 0)
        {
            return new(new Dictionary<long, List<CaseStepDto>>());
        }

        var query = await _repository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(
            query.Where(s => ids.Contains(s.CaseId)).OrderBy(s => s.Order).ThenBy(s => s.Id));
        return new(steps.GroupBy(s => s.CaseId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ToDto()).ToList()));
    }

    public async Task<BaseOutput> SaveAsync(long caseId, List<CaseStepDto> steps)
    {
        if (steps.Count == 0)
        {
            return new();
        }

        var entities = steps.Select(s => s.ToEntity(caseId)).ToList();
        await _repository.InsertManyAsync(entities, autoSave: true);
        return new();
    }

    public async Task<BaseOutput> SyncAsync(long caseId, List<CaseStepDto> steps)
    {
        var query = await _repository.GetQueryableAsync();
        var existing = await AsyncExecuter.ToListAsync(query.Where(s => s.CaseId == caseId));
        var existingMap = existing.ToDictionary(e => e.Id);

        var keepIds = new HashSet<long>();
        var toUpdate = new List<CaseStepEntity>();
        var toInsert = new List<CaseStepEntity>();
        foreach (var dto in steps)
        {
            if (long.TryParse(dto.Id, out var stepId) && existingMap.TryGetValue(stepId, out var stepEntity))
            {
                dto.Apply(stepEntity);
                keepIds.Add(stepId);
                toUpdate.Add(stepEntity);
            }
            else
            {
                toInsert.Add(dto.ToEntity(caseId));
            }
        }

        var toDelete = existing.Where(e => !keepIds.Contains(e.Id)).ToList();
        if (toDelete.Count > 0) await _repository.DeleteManyAsync(toDelete, autoSave: true);
        if (toUpdate.Count > 0) await _repository.UpdateManyAsync(toUpdate, autoSave: true);
        if (toInsert.Count > 0) await _repository.InsertManyAsync(toInsert, autoSave: true);
        return new();
    }

    public async Task<BaseOutput> DeleteByCaseIdAsync(long caseId)
    {
        await _repository.DeleteAsync(s => s.CaseId == caseId, autoSave: true);
        return new();
    }
}
