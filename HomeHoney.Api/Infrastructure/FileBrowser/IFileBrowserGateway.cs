namespace HomeHoney.Api.Infrastructure.FileBrowser;

public sealed record FileBrowserValidationResult(bool IsSuccess, string Message);

public interface IFileBrowserGateway
{
    Task<FileBrowserValidationResult> ValidateConnectionAsync(FileBrowserConnectionOptions connectionOptions, CancellationToken cancellationToken = default);

    Task<FileBrowserUploadResult> UploadFileAsync(FileBrowserConnectionOptions connectionOptions, string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default);

    Task<FileBrowserDownloadResult> DownloadFileAsync(FileBrowserConnectionOptions connectionOptions, string remotePath, CancellationToken cancellationToken = default);

    Task<FileBrowserDeleteResult> DeleteFileAsync(FileBrowserConnectionOptions connectionOptions, string remotePath, CancellationToken cancellationToken = default);
}
