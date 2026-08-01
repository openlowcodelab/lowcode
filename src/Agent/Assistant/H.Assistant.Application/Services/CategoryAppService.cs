using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Application;

/// <summary>
/// 任务分类管理应用服务
/// </summary>
public class CategoryAppService : ApplicationService, ICategoryAppService
{
    private readonly IRepository<CategoryEntity, Guid> _categoryRepository;
    private readonly IMapper _objectMapper;

    public CategoryAppService(
        IRepository<CategoryEntity, Guid> categoryRepository,
        IMapper objectMapper)
    {
        _categoryRepository = categoryRepository;
        _objectMapper = objectMapper;
    }

    public async Task<List<CategoryDto>> GetListAsync()
    {
        var queryable = await _categoryRepository.GetQueryableAsync();
        var list = await queryable.OrderBy(x => x.Sort).ThenBy(x => x.Name).ToListAsync();
        return list.Select(x => _objectMapper.Map<CategoryEntity, CategoryDto>(x)).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto input)
    {
        var entity = new CategoryEntity
        {
            Name = input.Name.Trim(),
            Sort = input.Sort
        };

        await _categoryRepository.InsertAsync(entity);
        return _objectMapper.Map<CategoryEntity, CategoryDto>(entity);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input)
    {
        var entity = await _categoryRepository.GetAsync(id);
        entity.Name = input.Name.Trim();
        entity.Sort = input.Sort;

        await _categoryRepository.UpdateAsync(entity);
        return _objectMapper.Map<CategoryEntity, CategoryDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _categoryRepository.DeleteAsync(id);
    }
}
