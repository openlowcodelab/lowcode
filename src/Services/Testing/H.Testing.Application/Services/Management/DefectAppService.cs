using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 缺陷管理服务：缺陷的增删改查
/// </summary>
public class DefectAppService : ApplicationService, IDefectAppService
{
    private readonly IRepository<DefectEntity, long> _repository;
    private readonly IRepository<CaseEntity, long> _caseRepository;

    public DefectAppService(
        IRepository<DefectEntity, long> repository,
        IRepository<CaseEntity, long> caseRepository)
    {
        _repository = repository;
        _caseRepository = caseRepository;
    }

    public async Task<BaseOutput<List<DefectDto>>> GetByProjectIdAsync(long projectId, int? status = null, int? severity = null)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(e => e.ProjectId == projectId);

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }
        if (severity.HasValue)
        {
            query = query.Where(e => e.Severity == severity.Value);
        }

        var defects = await AsyncExecuter.ToListAsync(query.OrderByDescending(e => e.CreationTime));
        return new(await FillCaseNamesAsync(defects));
    }

    public async Task<BaseOutput<List<DefectDto>>> GetByCaseIdAsync(long caseId)
    {
        var query = await _repository.GetQueryableAsync();
        var defects = await AsyncExecuter.ToListAsync(
            query.Where(e => e.CaseId == caseId).OrderByDescending(e => e.CreationTime));
        return new(await FillCaseNamesAsync(defects));
    }

    public async Task<BaseOutput<long>> CreateAsync(DefectDto dto)
    {
        var entity = new DefectEntity { ProjectId = dto.ProjectId };
        ApplyDto(entity, dto);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return new(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, DefectDto dto)
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

    private async Task<List<DefectDto>> FillCaseNamesAsync(List<DefectEntity> defects)
    {
        var caseIds = defects.Where(d => d.CaseId.HasValue).Select(d => d.CaseId!.Value).Distinct().ToList();

        Dictionary<long, string> caseNameMap = new();
        if (caseIds.Count > 0)
        {
            var caseQuery = await _caseRepository.GetQueryableAsync();
            var cases = await AsyncExecuter.ToListAsync(caseQuery.Where(c => caseIds.Contains(c.Id)));
            caseNameMap = cases.ToDictionary(c => c.Id, c => c.CaseName);
        }

        return defects.Select(e =>
        {
            var dto = ToDto(e);
            if (e.CaseId.HasValue)
            {
                dto.CaseName = caseNameMap.GetValueOrDefault(e.CaseId.Value);
            }
            return dto;
        }).ToList();
    }

    private static void ApplyDto(DefectEntity entity, DefectDto dto)
    {
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Severity = (int)dto.Severity;
        entity.Status = (int)dto.Status;
        entity.CaseId = dto.CaseId;
        entity.RecordId = dto.RecordId;
        entity.Assignee = string.IsNullOrWhiteSpace(dto.Assignee) ? null : dto.Assignee.Trim();
    }

    private static DefectDto ToDto(DefectEntity entity) => new()
    {
        Id = entity.Id,
        ProjectId = entity.ProjectId,
        Title = entity.Title,
        Description = entity.Description ?? string.Empty,
        Severity = (DefectSeverity)entity.Severity,
        Status = (DefectStatus)entity.Status,
        CaseId = entity.CaseId,
        RecordId = entity.RecordId,
        Assignee = entity.Assignee ?? string.Empty,
        CreationTime = entity.CreationTime
    };
}
