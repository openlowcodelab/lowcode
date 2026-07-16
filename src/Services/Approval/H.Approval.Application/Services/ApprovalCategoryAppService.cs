using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Approval.Application.Services;

/// <summary>
/// 审批分类(分组)应用服务
/// </summary>
public class ApprovalCategoryAppService : ApplicationService, IApprovalCategoryAppService
{
    private readonly ILogger<ApprovalCategoryAppService> _logger;
    private readonly IApprovalCategoryRepository _categoryRepository;
    private readonly IApprovalDefinitionRepository _definitionRepository;

    public ApprovalCategoryAppService(
        ILogger<ApprovalCategoryAppService> logger,
        IApprovalCategoryRepository categoryRepository,
        IApprovalDefinitionRepository definitionRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _definitionRepository = definitionRepository;
    }

    public async Task<List<ApprovalCategoryDto>> GetAllAsync()
    {
        var entities = await _categoryRepository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ApprovalCategoryDto> CreateAsync(CreateApprovalCategoryDto input)
    {
        var name = (input.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new UserFriendlyException("分类名称不能为空");
        }

        var existing = await _categoryRepository.GetByNameAsync(name);
        if (existing != null)
        {
            throw new UserFriendlyException($"分类已存在: {name}");
        }

        var all = await _categoryRepository.GetAllAsync();
        var maxSort = all.Count == 0 ? 0 : all.Max(c => c.Sort);

        var entity = new ApprovalCategory(Guid.NewGuid().ToString())
        {
            Name = name,
            Sort = maxSort + 1,
            CreationTime = DateTime.Now
        };

        await _categoryRepository.InsertAsync(entity);
        _logger.LogInformation("审批分类已创建: Id={Id}, Name={Name}", entity.Id, name);

        return MapToDto(entity);
    }

    public async Task<ApprovalCategoryDto> RenameAsync(RenameApprovalCategoryDto input)
    {
        var newName = (input.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(newName))
        {
            throw new UserFriendlyException("分类名称不能为空");
        }

        var entity = await _categoryRepository.GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new UserFriendlyException($"分类不存在: {input.Id}");
        }

        var duplicate = await _categoryRepository.GetByNameAsync(newName);
        if (duplicate != null && duplicate.Id != entity.Id)
        {
            throw new UserFriendlyException($"分类已存在: {newName}");
        }

        var oldName = entity.Name;
        if (oldName == newName)
        {
            return MapToDto(entity);
        }

        entity.Name = newName;
        await _categoryRepository.UpdateAsync(entity);

        // 同步更新引用该分类名的审批定义
        var definitions = await _definitionRepository.GetAllAsync();
        foreach (var def in definitions.Where(d => d.CategoryName == oldName))
        {
            def.CategoryName = newName;
            def.LastModificationTime = DateTime.Now;
            await _definitionRepository.UpdateAsync(def);
        }

        _logger.LogInformation("审批分类已重命名: Id={Id}, {OldName} -> {NewName}", entity.Id, oldName, newName);

        return MapToDto(entity);
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _categoryRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return;
        }

        // 将引用该分类的审批定义归入未分类
        var definitions = await _definitionRepository.GetAllAsync();
        foreach (var def in definitions.Where(d => d.CategoryName == entity.Name))
        {
            def.CategoryId = null;
            def.CategoryName = null;
            def.LastModificationTime = DateTime.Now;
            await _definitionRepository.UpdateAsync(def);
        }

        await _categoryRepository.DeleteAsync(id);
        _logger.LogInformation("审批分类已删除: Id={Id}, Name={Name}", id, entity.Name);
    }

    private static ApprovalCategoryDto MapToDto(ApprovalCategory entity)
    {
        return new ApprovalCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Sort = entity.Sort,
            CreationTime = entity.CreationTime
        };
    }
}

