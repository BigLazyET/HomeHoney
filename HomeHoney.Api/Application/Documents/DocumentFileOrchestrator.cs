using HomeHoney.Api.Infrastructure.Configuration;
using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Models;
using HomeHoney.Services.Storage;

namespace HomeHoney.Api.Application.Documents;

public sealed class DocumentFileOrchestrator
{
    private readonly IFileBrowserGateway _fileBrowserGateway;
    private readonly SecureSettingsStore _secureSettingsStore;

    public DocumentFileOrchestrator(IFileBrowserGateway fileBrowserGateway, SecureSettingsStore secureSettingsStore)
    {
        _fileBrowserGateway = fileBrowserGateway;
        _secureSettingsStore = secureSettingsStore;
    }

    public async Task<FileStorageResult> UploadPrimaryFileAsync(string folderPath, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var options = _secureSettingsStore.GetEffectiveStorageOptions();
        var upload = await _fileBrowserGateway.UploadFileAsync(ToConnectionOptions(options), folderPath, fileName, content, contentType, cancellationToken);
        return new FileStorageResult(
            upload.IsSuccess,
            upload.Message,
            upload.File?.Path,
            upload.File?.Path,
            upload.File?.Name,
            upload.File?.ContentType,
            upload.File?.SizeBytes,
            DateTime.UtcNow,
            upload.IsSuccess ? FileAvailabilityStatus.Available : FileAvailabilityStatus.SyncError);
    }

    public async Task<FileBrowserDownloadResult> DownloadPrimaryFileAsync(string? remotePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(remotePath))
        {
            return new(false, "当前资料还没有可下载的文件。");
        }

        var options = _secureSettingsStore.GetEffectiveStorageOptions();
        return await _fileBrowserGateway.DownloadFileAsync(ToConnectionOptions(options), remotePath, cancellationToken);
    }

    public async Task<FileBrowserDeleteResult> DeletePrimaryFileAsync(string? remotePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(remotePath))
        {
            return new(true, "当前资料没有需要删除的文件。");
        }

        var options = _secureSettingsStore.GetEffectiveStorageOptions();
        return await _fileBrowserGateway.DeleteFileAsync(ToConnectionOptions(options), remotePath, cancellationToken);
    }

    private static FileBrowserConnectionOptions ToConnectionOptions(BackendStorageOptions options)
        => new()
        {
            BaseUrl = options.FileServiceBaseUrl,
            ApiPath = options.FileServiceApiPath,
            FileServiceUsername = options.FileServiceUsername,
            FileServicePassword = options.FileServicePassword,
            FileServiceAuthHeaderName = options.FileServiceAuthHeaderName,
            FileServiceAuthHeaderValue = options.FileServiceAuthHeaderValue,
        };
}