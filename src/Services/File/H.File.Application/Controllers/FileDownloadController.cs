using H.File.Application.Services;
using H.File.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace H.File.Application.Controllers;

/// <summary>
/// 文件下载控制器：服务端转发 MinIO 文件并以附件形式返回，
/// 保证图片/PDF 等可在浏览器内预览的文件类型下载时也按附件保存，而非浏览器内打开。
/// MinIO 对象完整路径 = 文件实体的 FolderPath + Id，文件名等元数据取自数据库。
/// </summary>
[Route("api/file")]
public class FileDownloadController : Controller
{
    private readonly IRepository<FileProjectEntity, Guid> _repository;
    private readonly IRepository<FileObjectEntity, Guid> _objectRepository;
    private readonly MinioStorageService _storage;

    public FileDownloadController(
        IRepository<FileProjectEntity, Guid> repository,
        IRepository<FileObjectEntity, Guid> objectRepository,
        MinioStorageService storage)
    {
        _repository = repository;
        _objectRepository = objectRepository;
        _storage = storage;
    }

    /// <summary>
    /// 下载文件（GET /api/file/download?projectId=xxx&amp;fileId=xxx）
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadAsync(Guid projectId, Guid fileId)
    {
        var (bucketName, fileObject) = await GetStorageInfoAsync(projectId, fileId);
        var objectKey = GetObjectKey(fileObject);
        var data = await _storage.GetObjectAsync(bucketName, objectKey);
        var contentType = string.IsNullOrEmpty(fileObject.ContentType) ? "application/octet-stream" : fileObject.ContentType;
        return File(data, contentType, fileObject.FileName);
    }

    /// <summary>
    /// 预览文件（GET /api/file/preview?projectId=xxx&amp;fileId=xxx），内联返回内容，供 img/iframe 直接渲染
    /// </summary>
    [HttpGet("preview")]
    public async Task<IActionResult> PreviewAsync(Guid projectId, Guid fileId)
    {
        var (bucketName, fileObject) = await GetStorageInfoAsync(projectId, fileId);
        var objectKey = GetObjectKey(fileObject);
        var data = await _storage.GetObjectAsync(bucketName, objectKey);
        var contentType = string.IsNullOrEmpty(fileObject.ContentType) ? "application/octet-stream" : fileObject.ContentType;
        return File(data, contentType, enableRangeProcessing: true);
    }

    private static string GetObjectKey(FileObjectEntity fileObject)
        => fileObject.FolderPath + fileObject.Id.ToString("N");

    private async Task<(string BucketName, FileObjectEntity FileObject)> GetStorageInfoAsync(Guid projectId, Guid fileId)
    {
        var project = await _repository.GetAsync(projectId);
        var fileObject = await _objectRepository.FindAsync(o => o.Id == fileId && o.ProjectId == projectId)
            ?? throw new UserFriendlyException("文件不存在");
        return (project.BucketName, fileObject);
    }
}
