using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class FileBrowserFileStorageGateway : IFileStorageGateway
{
    public FileBrowserFileStorageGateway(HttpClient httpClient, IStorageConnectionProfileService profileService)
    {
    }

    public Task<IReadOnlyList<FileResource>> ListFilesAsync(string relativePath, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<FileResource>>([]);

    public Task<FileStorageResult> UploadFileAsync(string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
        => Task.FromResult(new FileStorageResult(false, "文件上传已迁移为后端职责，移动端不再直连文件服务。", AvailabilityStatus: FileAvailabilityStatus.SyncError));

    public Task<FileDownloadResult> DownloadFileAsync(string remotePath, CancellationToken cancellationToken = default)
        => Task.FromResult(new FileDownloadResult(false, "文件下载已迁移为后端职责，移动端不再直连文件服务。"));

    public Task<FileStorageResult> DeleteFileAsync(string remotePath, CancellationToken cancellationToken = default)
        => Task.FromResult(new FileStorageResult(false, "文件删除已迁移为后端职责，移动端不再直连文件服务。", AvailabilityStatus: FileAvailabilityStatus.SyncError));

    public Task<FileStorageResult> GetMetadataAsync(string remotePath, CancellationToken cancellationToken = default)
        => Task.FromResult(new FileStorageResult(false, "文件元数据探测已迁移为后端职责，移动端不再直连文件服务。", remotePath, remotePath, AvailabilityStatus: FileAvailabilityStatus.SyncError));
}
