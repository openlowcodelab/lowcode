using System.Collections.ObjectModel;
using H.Assistant.Application.Contracts;

namespace H.AppLab.Desktop.Services;

/// <summary>
/// 任务分类管理服务（通过 API 持久化到数据库）
/// </summary>
public class CategoryService
{
    private readonly ICategoryAppService _categoryAppService;

    public ObservableCollection<CategoryDto> Categories { get; } = [];

    public CategoryService(ICategoryAppService categoryAppService)
    {
        _categoryAppService = categoryAppService;
    }

    public async Task LoadAsync()
    {
        var list = await _categoryAppService.GetListAsync();
        Categories.Clear();
        foreach (var item in list)
        {
            Categories.Add(item);
        }
    }

    public async Task<CategoryDto?> AddAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        try
        {
            var dto = await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = name.Trim() });
            Categories.Add(dto);
            return dto;
        }
        catch
        {
            return null;
        }
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        try
        {
            var dto = await _categoryAppService.UpdateAsync(id, new UpdateCategoryDto { Name = name.Trim() });
            var index = Categories.ToList().FindIndex(c => c.Id == id);
            if (index >= 0)
            {
                Categories[index] = dto;
            }
            return dto;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await _categoryAppService.DeleteAsync(id);
            var item = Categories.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                Categories.Remove(item);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
