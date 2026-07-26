using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试用例分类服务（数据库存储）
/// </summary>
public class ProjectCaseCategoryAppService : ApplicationService, IProjectCaseCategoryAppService
{
    private readonly IRepository<TestingProjectCaseCategory, long> _repository;

    public ProjectCaseCategoryAppService(IRepository<TestingProjectCaseCategory, long> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取指定项目分类（树形结构）
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(c => c.ProjectId == projectId));
        var flat = list.Select(e => e.ToDto()).ToList();
        return BuildCategoryTree(flat);
    }

    public async Task<ProjectCaseCategory> CreateAsync(ProjectCaseCategory category)
    {
        var entity = new TestingProjectCaseCategory();
        category.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return entity.ToDto();
    }

    public async Task<bool> UpdateAsync(long id, ProjectCaseCategory category)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        category.Apply(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return true;
    }

    public async Task<bool> DeleteAsync(long projectId, long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return false;
        }

        await _repository.DeleteAsync(entity, autoSave: true);
        return true;
    }

    public async Task<List<ProjectCaseCategory>> GetTreeStructureAsync(long projectId)
    {
        return await GetByProjectIdAsync(projectId);
    }

    /// <summary>
    /// 将平铺的分类数据构建为树形结构
    /// </summary>
    private static List<ProjectCaseCategory> BuildCategoryTree(List<ProjectCaseCategory> flatCategories)
    {
        if (flatCategories == null || flatCategories.Count == 0)
        {
            return new List<ProjectCaseCategory>();
        }

        var categoryMap = new Dictionary<long, ProjectCaseCategory>();
        foreach (var category in flatCategories)
        {
            category.Childrens = System.Array.Empty<ProjectCaseCategory>();
            categoryMap[category.Id] = category;
        }

        var rootCategories = new List<ProjectCaseCategory>();
        foreach (var category in flatCategories)
        {
            if (category.ParentId == null || !categoryMap.ContainsKey(category.ParentId.Value))
            {
                rootCategories.Add(category);
            }
            else
            {
                var parent = categoryMap[category.ParentId.Value];
                var childrenList = parent.Childrens.ToList();
                childrenList.Add(category);
                parent.Childrens = childrenList.OrderBy(c => c.Order).ToArray();
            }
        }

        return rootCategories.OrderBy(c => c.Order).ToList();
    }
}
