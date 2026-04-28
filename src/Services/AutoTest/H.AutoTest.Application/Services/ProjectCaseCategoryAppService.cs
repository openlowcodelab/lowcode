using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

/// <summary>
/// 测试用例分类服务
/// </summary>
public class ProjectCaseCategoryAppService : ApplicationService, IProjectCaseCategoryAppService
{
    private readonly string _dataPath;

    public ProjectCaseCategoryAppService(IConfiguration configuration)
    {
        _dataPath = configuration["DataPath"]!;
    }

    /// <summary>
    /// 获取所有分类列表（所有项目）
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetAllAsync()
    {
        var allCategories = new List<ProjectCaseCategory>();
        
        if (!Directory.Exists(_dataPath))
            return allCategories;
            
        var projectDirs = Directory.GetDirectories(_dataPath)
            .Where(d => Directory.Exists(d) && !Path.GetFileName(d).StartsWith("."))
            .ToList();
            
        foreach (var projectDir in projectDirs)
        {
            var projectId = Path.GetFileName(projectDir);
            var projectCategories = await GetByProjectIdAsync(projectId);
            allCategories.AddRange(projectCategories);
        }
        
        return allCategories;
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
        var flatCategories = json.FromJson<List<ProjectCaseCategory>>();
        
        // 将平铺的数据构建树形结构
        return BuildCategoryTree(flatCategories);
    }

    /// <summary>
    /// 创建新的测试用例分类
    /// </summary>
    public async Task<ProjectCaseCategory> CreateAsync(ProjectCaseCategory category)
    {
        category.Id = Guid.NewGuid().ToString();

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
    /// 将平铺的分类数据构建为树形结构
    /// </summary>
    private List<ProjectCaseCategory> BuildCategoryTree(List<ProjectCaseCategory> flatCategories)
    {
        if (flatCategories == null || flatCategories.Count == 0)
            return new List<ProjectCaseCategory>();

        // 创建字典以便快速查找
        var categoryMap = new Dictionary<string, ProjectCaseCategory>();
        foreach (var category in flatCategories)
        {
            category.Childrens = new ProjectCaseCategory[0];
            categoryMap[category.Id] = category;
        }

        var rootCategories = new List<ProjectCaseCategory>();

        // 构建树形结构
        foreach (var category in flatCategories)
        {
            if (string.IsNullOrEmpty(category.ParentId) || category.ParentId == "0")
            {
                // 根节点
                rootCategories.Add(category);
            }
            else if (categoryMap.ContainsKey(category.ParentId))
            {
                // 子节点，添加到父节点的 Childrens 中
                var parent = categoryMap[category.ParentId];
                if (parent.Childrens == null)
                {
                    parent.Childrens = new ProjectCaseCategory[0];
                }
                var childrenList = parent.Childrens.ToList();
                childrenList.Add(category);
                parent.Childrens = childrenList.OrderBy(c => c.Order).ToArray();
            }
        }

        return rootCategories.OrderBy(c => c.Order).ToList();
    }

    /// <summary>
    /// 获取树形结构的分类列表
    /// </summary>
    public async Task<List<ProjectCaseCategory>> GetTreeStructureAsync(string projectId)
    {
        // GetByProjectIdAsync 已经返回树形结构
        return await GetByProjectIdAsync(projectId);
    }
}