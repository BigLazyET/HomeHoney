namespace HomeHoney.Api.Infrastructure.FileBrowser;

public sealed record FileBrowserValidationResult(bool IsSuccess, string Message);

public interface IFileBrowserGateway
{
    Task<FileBrowserValidationResult> ValidateConnectionAsync(string baseUrl, string apiPath, CancellationToken cancellationToken = default);

    Task<FileBrowserUploadResult> UploadFileAsync(string baseUrl, string apiPath, string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default);

    Task<FileBrowserDownloadResult> DownloadFileAsync(string baseUrl, string apiPath, string remotePath, CancellationToken cancellationToken = default);

    Task<FileBrowserDeleteResult> DeleteFileAsync(string baseUrl, string apiPath, string remotePath, CancellationToken cancellationToken = default);
}
