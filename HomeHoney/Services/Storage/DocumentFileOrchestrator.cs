using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class DocumentFileOrchestrator
{
    private readonly IFileStorageGateway _fileStorageGateway;
    private readonly ILocalFileCache _localFileCache;

    public DocumentFileOrchestrator(IFileStorageGateway fileStorageGateway, ILocalFileCache localFileCache)
    {
        _fileStorageGateway = fileStorageGateway;
        _localFileCache = localFileCache;
    }

    public async Task<FileStorageResult> UploadInsuranceFileAsync(InsuranceRecord record, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var result = await _fileStorageGateway.UploadFileAsync($"insurance/{record.Id}", fileName, content, contentType, cancellationToken);
        FileSyncStateMapper.ApplyUploadResult(record, result);
        return result;
    }

    public async Task<FileStorageResult> UploadManualFileAsync(ManualRecord record, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var result = await _fileStorageGateway.UploadFileAsync($"manuals/{record.Id}", fileName, content, contentType, cancellationToken);
        FileSyncStateMapper.ApplyUploadResult(record, result);
        return result;
    }

    public async Task<FileDownloadResult> DownloadAsync(FileResource fileResource, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileResource.ExternalPath))
        {
            return new(false, "当前记录还没有可下载的文件。");
        }

        var remoteResult = await _fileStorageGateway.DownloadFileAsync(fileResource.ExternalPath, cancellationToken);
        if (!remoteResult.IsSuccess || remoteResult.Content is null)
        {
            var cached = await _localFileCache.GetAsync(fileResource.ExternalPath, cancellationToken);
            return cached is null
                ? remoteResult
                : new(true, "已使用本地缓存文件。", cached, remoteResult.FileName ?? fileResource.FileName, remoteResult.ContentType ?? fileResource.ContentType, fileResource.ExternalPath);
        }

        fileResource.CachedLocalPath = await _localFileCache.SaveAsync(fileResource.ExternalPath, remoteResult.FileName ?? fileResource.FileName, remoteResult.Content, cancellationToken);
        fileResource.AvailabilityStatus = FileAvailabilityStatus.Cached;
        fileResource.LastSyncedAt = DateTime.UtcNow;
        return remoteResult;
    }
}
