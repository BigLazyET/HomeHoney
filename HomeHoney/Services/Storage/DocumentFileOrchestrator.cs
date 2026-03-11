using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class DocumentFileOrchestrator
{
    private readonly IFileStorageGateway _fileStorageGateway;

    public DocumentFileOrchestrator(IFileStorageGateway fileStorageGateway)
    {
        _fileStorageGateway = fileStorageGateway;
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
        if (remoteResult.IsSuccess)
        {
            fileResource.AvailabilityStatus = FileAvailabilityStatus.Available;
            fileResource.LastSyncedAt = DateTime.UtcNow;
        }

        return remoteResult;
    }
}
