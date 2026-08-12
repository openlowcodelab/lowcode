using H.File.Application.Contracts;
using H.File.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace H.File.Application.Services;

/// <summary>
/// 文件项目应用服务（管理 MinIO Bucket）
/// </summary>
public class FileProjectAppService : ApplicationService, IFileProjectAppService
{
    private const string ProjectCodeChars = "abcdefghijklmnopqrstuvwxyz";
    private static readonly Regex ProjectCodeRegex = new("^[a-z]{3,20}$", RegexOptions.Compiled);

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
            Code = e.Code,
            Description = e.Description,
            Icon = e.Icon,
            BucketName = e.BucketName,
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
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new UserFriendlyException("项目名称不能为空");

        // 项目编号：留空自动生成 8 位小写字母，否则校验 3-20 位小写字母
        var code = string.IsNullOrWhiteSpace(input.Code)
            ? await GenerateUniqueCodeAsync()
            : input.Code.Trim().ToLowerInvariant();
        if (!ProjectCodeRegex.IsMatch(code))
            throw new UserFriendlyException("项目编号需为 3-20 位小写字母");

        // 生成 bucket 名称：租户前缀 + 项目编号（全局唯一，含租户前缀）
        var tenantPrefix = _currentTenant.Id?.ToString("N")[..8] ?? "default";
        var bucketName = $"{tenantPrefix}-{code}";

        if (await _repository.AnyAsync(x => x.BucketName == bucketName))
            throw new UserFriendlyException($"项目编号「{code}」已存在");

        // 创建 MinIO Bucket
        await _storage.CreateBucketAsync(bucketName);

        var entity = new FileProjectEntity
        {
            Name = input.Name,
            Code = code,
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
        Code = e.Code,
        Description = e.Description,
        Icon = e.Icon,
        BucketName = e.BucketName,
        CreationTime = e.CreationTime
    };

    private static string GenerateRandomCode()
    {
        var chars = new char[8];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = ProjectCodeChars[Random.Shared.Next(ProjectCodeChars.Length)];
        return new string(chars);
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        var bucketPrefix = $"{_currentTenant.Id?.ToString("N")[..8] ?? "default"}-";
        for (int i = 0; i < 10; i++)
        {
            var code = GenerateRandomCode();
            if (!await _repository.AnyAsync(x => x.BucketName == bucketPrefix + code))
                return code;
        }
        throw new UserFriendlyException("无法生成唯一的项目编号，请重试");
    }
}
