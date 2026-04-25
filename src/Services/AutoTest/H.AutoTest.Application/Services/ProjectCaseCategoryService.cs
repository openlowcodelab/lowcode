using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

/// <summary>
/// 测试用例分类服务
/// </summary>
public class ProjectCaseCategoryService : ApplicationService, IProjectCaseCategoryAppService
{
    private readonly string _dataPath;

    public ProjectCaseCategoryService(IConfiguration configuration)
    {
        _dataPath = configuration["DataPath"] ?? "data";
    }

    /// <summary>
    /// 根据项目 ID 获取分类列表
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetByProjectIdAsync(string projectId)
    {
        var filePath = Path.Combine(_dataPath, projectId, "project-case-categories.json");
        if (!File.Exists(filePath))
        {
            return new List<ProjectCaseCategory>();
        }

        var json = await File.ReadAllTextAsync(filePath);
        var categories = System.Text.Json.JsonSerializer.Deserialize<List<ProjectCaseCategory>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProjectCaseCategory>();
        return categories.OrderBy(c => c.Order).ToList();
    }

    /// <summary>
    /// 创建新的测试用例分类
    /// </summary>
    public async Task<ProjectCaseCategory> CreateAsync(ProjectCaseCategory category)
    {
        category.Id = Guid.NewGuid().ToString();
        category.CreatedAt = DateTime.Now;

        var categories = await GetByProjectIdAsync(category.ProjectId);
        categories.Add(category);

        await SaveAsync(category.ProjectId, categories);
        return category;
    }

    /// <summary>
    /// 更新测试用例分类
    /// </summary>
    public async Task<bool> UpdateAsync(string id, ProjectCaseCategory category)
    {
        var categories = await GetByProjectIdAsync(category.ProjectId);
        var index = categories.FindIndex(c => c.Id == id);

        if (index >= 0)
        {
            category.Id = id;
            categories[index] = category;
            await SaveAsync(category.ProjectId, categories);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 删除测试用例分类
    /// </summary>
    public async Task<bool> DeleteAsync(string projectId, string id)
    {
        var categories = await GetByProjectIdAsync(projectId);
        var category = categories.FirstOrDefault(c => c.Id == id);

        if (category != null)
        {
            categories.Remove(category);
            await SaveAsync(projectId, categories);
            return true;
        }

        return false;
    }

    private async Task SaveAsync(string projectId, List<ProjectCaseCategory> categories)
    {
        var dirPath = Path.Combine(_dataPath, projectId);
        Directory.CreateDirectory(dirPath);

        var filePath = Path.Combine(dirPath, "project-case-categories.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = System.Text.Json.JsonSerializer.Serialize(categories, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 获取树形结构的分类列表
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetTreeStructureAsync(string projectId)
    {
        var allCategories = await GetByProjectIdAsync(projectId);

        // 构建树形结构
        var rootCategories = allCategories.Where(c => string.IsNullOrEmpty(c.ParentId)).ToList();

        foreach (var category in rootCategories)
        {
            BuildCategoryTree(category, allCategories);
        }

        return rootCategories;
    }

    private void BuildCategoryTree(ProjectCaseCategory parent, List<ProjectCaseCategory> allCategories)
    {
        // 这里可以根据需要扩展子分类逻辑
        // 由于当前模型中没有 Children 属性，暂时保持简单实现
    }
}