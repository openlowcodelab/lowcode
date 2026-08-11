using System.Text.Json;
using H.Assistant.Application.Contracts;
using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 项目服务（数据库存储）
/// </summary>
public class ProjectAppService : ApplicationService, IProjectAppService
{
    private readonly IRepository<TestingProject, long> _repository;
    private readonly IRepository<TestingProjectService, long> _serviceRepository;
    private readonly IRepository<TestingProjectEnvironment, long> _environmentRepository;
    private readonly IRepository<TestingEnvironmentServiceConfig, long> _serviceConfigRepository;
    private readonly IRepository<TestingProjectCaseCategory, long> _categoryRepository;
    private readonly IRepository<TestingProjectCase, long> _caseRepository;
    private readonly IRepository<TestingExecutionRecord, long> _executionRepository;
    private readonly IProjectEnvironmentAppService _environmentService;
    private readonly IKnowledgeBaseAppService _knowledgeBaseService;

    public ProjectAppService(
        IRepository<TestingProject, long> repository,
        IRepository<TestingProjectService, long> serviceRepository,
        IRepository<TestingProjectEnvironment, long> environmentRepository,
        IRepository<TestingEnvironmentServiceConfig, long> serviceConfigRepository,
        IRepository<TestingProjectCaseCategory, long> categoryRepository,
        IRepository<TestingProjectCase, long> caseRepository,
        IRepository<TestingExecutionRecord, long> executionRepository,
        IProjectEnvironmentAppService environmentService,
        IKnowledgeBaseAppService knowledgeBaseService)
    {
        _repository = repository;
        _serviceRepository = serviceRepository;
        _environmentRepository = environmentRepository;
        _serviceConfigRepository = serviceConfigRepository;
        _categoryRepository = categoryRepository;
        _caseRepository = caseRepository;
        _executionRepository = executionRepository;
        _environmentService = environmentService;
        _knowledgeBaseService = knowledgeBaseService;
    }

    public async Task<List<ProjectDto>> GetAllAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(p => p.Id));
        return list.Select(e => e.ToDto()).ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        return entity?.ToDto();
    }

    public async Task<long> CreateAsync(ProjectDto project)
    {
        var entity = new TestingProject();
        project.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return entity.Id;
    }

    public Task<long> CreateProjectAsync(ProjectDto project) => CreateAsync(project);

    public async Task<bool> UpdateAsync(long id, ProjectDto project)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        project.Apply(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        // 级联删除项目下的所有相关数据
        var envQuery = await _environmentRepository.GetQueryableAsync();
        var envIds = await AsyncExecuter.ToListAsync(envQuery.Where(e => e.ProjectId == id).Select(e => e.Id));
        if (envIds.Count > 0)
        {
            await _serviceConfigRepository.DeleteAsync(c => envIds.Contains(c.EnvironmentId), autoSave: true);
        }

        await _caseRepository.DeleteAsync(c => c.ProjectId == id, autoSave: true);
        await _categoryRepository.DeleteAsync(c => c.ProjectId == id, autoSave: true);
        await _serviceRepository.DeleteAsync(s => s.ProjectId == id, autoSave: true);
        await _environmentRepository.DeleteAsync(e => e.ProjectId == id, autoSave: true);
        await _executionRepository.DeleteAsync(r => r.ProjectId == id, autoSave: true);

        // 尽力删除项目关联的知识库，失败不阻断项目删除
        await TryDeleteLinkedKnowledgeBaseAsync(entity);

        await _repository.DeleteAsync(entity, autoSave: true);
        return true;
    }

    private async Task TryDeleteLinkedKnowledgeBaseAsync(TestingProject entity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entity.MetadataJson))
            {
                return;
            }

            using var document = JsonDocument.Parse(entity.MetadataJson);
            if (document.RootElement.TryGetProperty("knowledgeBaseId", out var property)
                && Guid.TryParse(property.GetString()?.Trim().Trim('"'), out var knowledgeBaseId))
            {
                await _knowledgeBaseService.DeleteAsync(knowledgeBaseId);
            }
        }
        catch
        {
            // 尽力删除，忽略异常
        }
    }

    public async Task<List<ProjectEnvironmentDto>> GetProjectEnvironmentsAsync(long projectId)
    {
        return await _environmentService.GetByProjectIdAsync(projectId);
    }
}
