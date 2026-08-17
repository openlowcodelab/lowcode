using H.Assistant.Application.Contracts;
using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 项目服务（数据库存储）
/// </summary>
public class ProjectAppService : ApplicationService, IProjectAppService
{
    private readonly IRepository<ProjectEntity, long> _repository;
    private readonly IRepository<ProjectServiceEntity, long> _serviceRepository;
    private readonly IRepository<ProjectEnvEntity, long> _environmentRepository;
    private readonly IRepository<CaseCategoryEntity, long> _categoryRepository;
    private readonly IRepository<CaseEntity, long> _caseRepository;
    private readonly IRepository<CaseStepEntity, long> _caseStepRepository;
    private readonly IRepository<CaseRecordEntity, long> _executionRepository;
    private readonly IProjectEnvAppService _environmentService;
    private readonly IKnowledgeBaseAppService _knowledgeBaseService;

    public ProjectAppService(
        IRepository<ProjectEntity, long> repository,
        IRepository<ProjectServiceEntity, long> serviceRepository,
        IRepository<ProjectEnvEntity, long> environmentRepository,
        IRepository<CaseCategoryEntity, long> categoryRepository,
        IRepository<CaseEntity, long> caseRepository,
        IRepository<CaseStepEntity, long> caseStepRepository,
        IRepository<CaseRecordEntity, long> executionRepository,
        IProjectEnvAppService environmentService,
        IKnowledgeBaseAppService knowledgeBaseService)
    {
        _repository = repository;
        _serviceRepository = serviceRepository;
        _environmentRepository = environmentRepository;
        _categoryRepository = categoryRepository;
        _caseRepository = caseRepository;
        _caseStepRepository = caseStepRepository;
        _executionRepository = executionRepository;
        _environmentService = environmentService;
        _knowledgeBaseService = knowledgeBaseService;
    }

    public async Task<BaseOutput<List<ProjectDto>>> GetAllAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(p => p.Id));
        return BaseOutput<List<ProjectDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<ProjectDto?>> GetByIdAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        return BaseOutput<ProjectDto?>.Ok(entity?.ToDto());
    }

    public async Task<BaseOutput<long>> CreateAsync(ProjectDto project)
    {
        var entity = new ProjectEntity();
        project.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return BaseOutput<long>.Ok(entity.Id);
    }

    public Task<BaseOutput<long>> CreateProjectAsync(ProjectDto project) => CreateAsync(project);

    public async Task<BaseOutput<bool>> UpdateAsync(long id, ProjectDto project)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return BaseOutput<bool>.Ok(false);
        }

        project.Apply(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return BaseOutput<bool>.Ok(false);
        }

        // 级联删除项目下的所有相关数据（服务配置随环境一同删除）
        var caseQuery = await _caseRepository.GetQueryableAsync();
        var caseIds = await AsyncExecuter.ToListAsync(caseQuery.Where(c => c.ProjectId == id).Select(c => c.Id));

        await _caseStepRepository.DeleteAsync(s => caseIds.Contains(s.CaseId), autoSave: true);
        await _caseRepository.DeleteAsync(c => c.ProjectId == id, autoSave: true);
        await _categoryRepository.DeleteAsync(c => c.ProjectId == id, autoSave: true);
        await _serviceRepository.DeleteAsync(s => s.ProjectId == id, autoSave: true);
        await _environmentRepository.DeleteAsync(e => e.ProjectId == id, autoSave: true);
        await _executionRepository.DeleteAsync(r => r.ProjectId == id, autoSave: true);

        // 尽力删除项目关联的知识库，失败不阻断项目删除
        await TryDeleteLinkedKnowledgeBaseAsync(entity);

        await _repository.DeleteAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    private async Task TryDeleteLinkedKnowledgeBaseAsync(ProjectEntity entity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entity.KnowledgeBaseId))
            {
                return;
            }

            if (Guid.TryParse(entity.KnowledgeBaseId, out var knowledgeBaseId))
            {
                await _knowledgeBaseService.DeleteAsync(knowledgeBaseId);
            }
        }
        catch
        {
            // 尽力删除，忽略异常
        }
    }

    public async Task<BaseOutput<List<ProjectEnvDto>>> GetProjectEnvironmentsAsync(long projectId)
    {
        return await _environmentService.GetByProjectIdAsync(projectId);
    }
}
