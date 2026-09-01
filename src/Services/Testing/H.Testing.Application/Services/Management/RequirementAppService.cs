using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 需求管理服务：需求增删改查、用例关联与追溯视图
/// </summary>
public class RequirementAppService : ApplicationService, IRequirementAppService
{
    private readonly IRepository<RequirementEntity, long> _repository;
    private readonly IRepository<CaseRequirementLinkEntity, long> _linkRepository;
    private readonly IRepository<CaseEntity, long> _caseRepository;
    private readonly IRepository<DefectEntity, long> _defectRepository;

    public RequirementAppService(
        IRepository<RequirementEntity, long> repository,
        IRepository<CaseRequirementLinkEntity, long> linkRepository,
        IRepository<CaseEntity, long> caseRepository,
        IRepository<DefectEntity, long> defectRepository)
    {
        _repository = repository;
        _linkRepository = linkRepository;
        _caseRepository = caseRepository;
        _defectRepository = defectRepository;
    }

    public async Task<BaseOutput<List<RequirementDto>>> GetByProjectIdAsync(long projectId)
    {
        var requirementQuery = await _repository.GetQueryableAsync();
        var requirements = await AsyncExecuter.ToListAsync(
            requirementQuery.Where(e => e.ProjectId == projectId).OrderBy(e => e.Priority).ThenByDescending(e => e.CreationTime));

        var linkQuery = await _linkRepository.GetQueryableAsync();
        var linkCounts = await AsyncExecuter.ToListAsync(
            linkQuery.Where(l => requirements.Select(r => r.Id).Contains(l.RequirementId))
                     .GroupBy(l => l.RequirementId)
                     .Select(g => new { RequirementId = g.Key, Count = g.Count() }));
        var countMap = linkCounts.ToDictionary(x => x.RequirementId, x => x.Count);

        return new(requirements.Select(e =>
        {
            var dto = ToDto(e);
            dto.LinkedCaseCount = countMap.GetValueOrDefault(e.Id);
            return dto;
        }).ToList());
    }

    public async Task<BaseOutput<long>> CreateAsync(RequirementDto dto)
    {
        var entity = new RequirementEntity { ProjectId = dto.ProjectId };
        ApplyDto(entity, dto);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return new(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, RequirementDto dto)
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

        var linkQuery = await _linkRepository.GetQueryableAsync();
        var links = await AsyncExecuter.ToListAsync(linkQuery.Where(l => l.RequirementId == id));
        await _linkRepository.DeleteManyAsync(links, autoSave: true);

        await _repository.DeleteAsync(entity, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<List<long>>> GetLinkedCaseIdsAsync(long requirementId)
    {
        var linkQuery = await _linkRepository.GetQueryableAsync();
        var links = await AsyncExecuter.ToListAsync(linkQuery.Where(l => l.RequirementId == requirementId));
        return new(links.Select(l => l.CaseId).ToList());
    }

    public async Task<BaseOutput<bool>> SetCaseLinksAsync(long requirementId, List<long> caseIds)
    {
        var requirement = await _repository.FindAsync(requirementId);
        if (requirement == null)
        {
            return new(false);
        }

        var distinctIds = caseIds.Distinct().ToList();

        var linkQuery = await _linkRepository.GetQueryableAsync();
        var existingLinks = await AsyncExecuter.ToListAsync(linkQuery.Where(l => l.RequirementId == requirementId));

        var toDelete = existingLinks.Where(l => !distinctIds.Contains(l.CaseId)).ToList();
        var existingCaseIds = existingLinks.Select(l => l.CaseId).ToHashSet();
        var toAdd = distinctIds.Where(id => !existingCaseIds.Contains(id))
            .Select(caseId => new CaseRequirementLinkEntity { RequirementId = requirementId, CaseId = caseId })
            .ToList();

        if (toDelete.Count > 0)
        {
            await _linkRepository.DeleteManyAsync(toDelete, autoSave: true);
        }
        if (toAdd.Count > 0)
        {
            await _linkRepository.InsertManyAsync(toAdd, autoSave: true);
        }

        return new(true);
    }

    public async Task<BaseOutput<RequirementTraceDto>> GetTraceAsync(long requirementId)
    {
        var requirement = await _repository.FindAsync(requirementId);
        if (requirement == null)
        {
            return new(null!);
        }

        var linkQuery = await _linkRepository.GetQueryableAsync();
        var caseIds = await AsyncExecuter.ToListAsync(
            linkQuery.Where(l => l.RequirementId == requirementId).Select(l => l.CaseId));

        var trace = new RequirementTraceDto { Requirement = ToDto(requirement) };

        if (caseIds.Count == 0)
        {
            return new(trace);
        }

        var caseQuery = await _caseRepository.GetQueryableAsync();
        var cases = await AsyncExecuter.ToListAsync(caseQuery.Where(c => caseIds.Contains(c.Id)));

        var defectQuery = await _defectRepository.GetQueryableAsync();
        var defects = await AsyncExecuter.ToListAsync(
            defectQuery.Where(d => d.CaseId != null && caseIds.Contains(d.CaseId.Value))
                       .OrderByDescending(d => d.CreationTime));

        var defectCountByCase = defects.Where(d => d.CaseId.HasValue)
            .GroupBy(d => d.CaseId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        trace.Cases = cases.Select(c => new RequirementTraceCase
        {
            CaseId = c.Id,
            CaseName = c.CaseName,
            Level = (CaseLevel)c.Level,
            LastExecutionResult = (ExecutionStatus?)c.LastExecutionResult,
            LastExecutionTime = c.LastExecutionTime,
            DefectCount = defectCountByCase.GetValueOrDefault(c.Id)
        }).ToList();

        trace.Defects = defects.Select(ToDefectDto).ToList();

        return new(trace);
    }

    private static void ApplyDto(RequirementEntity entity, RequirementDto dto)
    {
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Priority = (int)dto.Priority;
        entity.Status = (int)dto.Status;
    }

    private static RequirementDto ToDto(RequirementEntity entity) => new()
    {
        Id = entity.Id,
        ProjectId = entity.ProjectId,
        Title = entity.Title,
        Description = entity.Description ?? string.Empty,
        Priority = (RequirementPriority)entity.Priority,
        Status = (RequirementStatus)entity.Status,
        CreationTime = entity.CreationTime
    };

    private static DefectDto ToDefectDto(DefectEntity entity) => new()
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
