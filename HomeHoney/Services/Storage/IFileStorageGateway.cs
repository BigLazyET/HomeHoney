using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public interface IFileStorageGateway
{
    Task<IReadOnlyList<FileResource>> ListFilesAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<FileStorageResult> UploadFileAsync(string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default);

    Task<FileDownloadResult> DownloadFileAsync(string remotePath, CancellationToken cancellationToken = default);

    Task<FileStorageResult> DeleteFileAsync(string remotePath, CancellationToken cancellationToken = default);

    Task<FileStorageResult> GetMetadataAsync(string remotePath, CancellationToken cancellationToken = default);
}
