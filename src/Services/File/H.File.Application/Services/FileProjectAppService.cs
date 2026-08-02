using H.File.Application.Contracts;
using H.File.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace H.File.Application.Services;

/// <summary>
/// 文件项目应用服务（管理 MinIO Bucket）
/// </summary>
public class FileProjectAppService : ApplicationService, IFileProjectAppService
{
    private readonly IRepository<FileProjectEntity, Guid> _repository;
    private readonly MinioStorageService _storage;
    private readonly ICurrentTenant _currentTenant;

    public FileProjectAppService(
        IRepository<FileProjectEntity, Guid> repository,
        MinioStorageService storage,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _storage = storage;
        _currentTenant = currentTenant;
    }

    public async Task<List<FileProjectDto>> GetListAsync()
    {
        var queryable = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(x => x.CreationTime));

        // 直接从数据库读取统计信息，不再调用 minio
        var dtos = entities.Select(e => new FileProjectDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Icon = e.Icon,
            FileCount = e.FileCount,
            TotalSize = e.TotalSize,
            CreationTime = e.CreationTime
        }).ToList();

        return dtos;
    }

    public async Task<FileProjectDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<FileProjectDto> CreateAsync(CreateFileProjectDto input)
    {
        // 生成 bucket 名称：租户前缀 + 项目名称（小写、去特殊字符），最长 63 字符（MinIO 限制）
        var tenantPrefix = _currentTenant.Id?.ToString("N")[..8] ?? "default";
        var safeName = new string(input.Name.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray()).ToLowerInvariant();
        var rawBucketName = $"{tenantPrefix}-{safeName}-{Guid.NewGuid():N}";
        var bucketName = (rawBucketName.Length > 63 ? rawBucketName[..63] : rawBucketName).TrimEnd('-');

        // 创建 MinIO Bucket
        await _storage.CreateBucketAsync(bucketName);

        var entity = new FileProjectEntity
        {
            Name = input.Name,
            Description = input.Description,
            Icon = input.Icon ?? "📁",
            BucketName = bucketName
        };

        await _repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<FileProjectDto> UpdateAsync(Guid id, UpdateFileProjectDto input)
    {
        var entity = await _repository.GetAsync(id);

        if (!string.IsNullOrWhiteSpace(input.Name))
            entity.Name = input.Name;
        if (input.Description is not null)
            entity.Description = input.Description;
        if (input.Icon is not null)
            entity.Icon = input.Icon;

        await _repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);

        // 删除 MinIO Bucket 及所有文件
        await _storage.DeleteBucketAsync(entity.BucketName);

        await _repository.DeleteAsync(entity);
    }

    private static FileProjectDto MapToDto(FileProjectEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Icon = e.Icon,
        CreationTime = e.CreationTime
    };
}
