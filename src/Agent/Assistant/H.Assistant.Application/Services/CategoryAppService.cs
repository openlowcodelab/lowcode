using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using H.Util.Base;
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

    public async Task<BaseOutput<List<CategoryDto>>> GetListAsync()
    {
        var queryable = await _categoryRepository.GetQueryableAsync();
        var list = await queryable.OrderBy(x => x.Sort).ThenBy(x => x.Name).ToListAsync();
        return new(list.Select(x => _objectMapper.Map<CategoryEntity, CategoryDto>(x)).ToList());
    }

    public async Task<BaseOutput<CategoryDto>> CreateAsync(CreateCategoryDto input)
    {
        var entity = new CategoryEntity
        {
            Name = input.Name.Trim(),
            Sort = input.Sort
        };

        entity = await _categoryRepository.InsertAsync(entity);
        return new(_objectMapper.Map<CategoryEntity, CategoryDto>(entity));
    }

    public async Task<BaseOutput<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto input)
    {
        var entity = await _categoryRepository.GetAsync(id);
        entity.Name = input.Name.Trim();
        entity.Sort = input.Sort;

        entity = await _categoryRepository.UpdateAsync(entity);
        return new(_objectMapper.Map<CategoryEntity, CategoryDto>(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _categoryRepository.DeleteAsync(id);
        return new();
    }
}
