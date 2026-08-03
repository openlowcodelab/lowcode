using H.File.Application.Services;
using H.File.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace H.File.Application.Controllers;

/// <summary>
/// 文件下载控制器：服务端转发 MinIO 文件并以附件形式返回，
/// 保证图片/PDF 等可在浏览器内预览的文件类型下载时也按附件保存，而非浏览器内打开
/// </summary>
[Route("api/file")]
public class FileDownloadController : Controller
{
    private readonly IRepository<FileProjectEntity, Guid> _repository;
    private readonly MinioStorageService _storage;

    public FileDownloadController(
        IRepository<FileProjectEntity, Guid> repository,
        MinioStorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    /// <summary>
    /// 下载文件（GET /api/file/download?projectId=xxx&amp;fileKey=xxx）
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadAsync(Guid projectId, string fileKey)
    {
        var entity = await _repository.GetAsync(projectId);
        var data = await _storage.GetObjectAsync(entity.BucketName, fileKey);
        var fileName = GetFileName(fileKey);
        var contentType = await _storage.GetObjectContentTypeAsync(entity.BucketName, fileKey) ?? "application/octet-stream";
        return File(data, contentType, fileName);
    }

    private static string GetFileName(string key)
    {
        var trimmed = key.TrimEnd('/');
        var idx = trimmed.LastIndexOf('/');
        return idx >= 0 ? trimmed[(idx + 1)..] : trimmed;
    }
}
