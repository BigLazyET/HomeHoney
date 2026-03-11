using System.Net.Http.Headers;
using System.Text.Json;
using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class FileBrowserFileStorageGateway : IFileStorageGateway
{
    private readonly HttpClient _httpClient;
    private readonly IStorageConnectionProfileService _profileService;

    public FileBrowserFileStorageGateway(HttpClient httpClient, IStorageConnectionProfileService profileService)
    {
        _httpClient = httpClient;
        _profileService = profileService;
    }

    public async Task<IReadOnlyList<FileResource>> ListFilesAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiBase = await GetApiBaseAsync(cancellationToken);
            using var response = await _httpClient.GetAsync(BuildResourceUri(apiBase, relativePath), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!json.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var results = new List<FileResource>();
            foreach (var item in items.EnumerateArray())
            {
                var isDir = item.TryGetProperty("isDir", out var isDirValue) && isDirValue.GetBoolean();
                if (isDir)
                {
                    continue;
                }

                var path = item.TryGetProperty("path", out var pathValue) ? pathValue.GetString() : null;
                results.Add(new FileResource
                {
                    ExternalFileId = path,
                    ExternalPath = path,
                    FileName = item.TryGetProperty("name", out var nameValue) ? nameValue.GetString() ?? string.Empty : string.Empty,
                    SizeBytes = item.TryGetProperty("size", out var sizeValue) ? sizeValue.GetInt64() : 0,
                    AvailabilityStatus = FileAvailabilityStatus.Available,
                    LastSyncedAt = DateTime.UtcNow,
                });
            }

            return results;
        }
        catch
        {
            return [];
        }
    }

    public async Task<FileStorageResult> UploadFileAsync(string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiBase = await GetApiBaseAsync(cancellationToken);
            using var form = new MultipartFormDataContent();
            using var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
            form.Add(streamContent, "files", fileName);

            using var response = await _httpClient.PostAsync(BuildResourceUri(apiBase, folderPath), form, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"上传失败：{(int)response.StatusCode} {response.ReasonPhrase}", AvailabilityStatus: FileAvailabilityStatus.SyncError);
            }

            var remotePath = CombinePath(folderPath, fileName);
            return new(true, "上传成功。", remotePath, remotePath, contentType, content.Length, FileAvailabilityStatus.Available);
        }
        catch (Exception ex)
        {
            return new(false, $"上传失败：{ex.Message}", AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }
    }

    public async Task<FileDownloadResult> DownloadFileAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiBase = await GetApiBaseAsync(cancellationToken);
            using var response = await _httpClient.GetAsync(BuildRawUri(apiBase, remotePath), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"下载失败：{(int)response.StatusCode} {response.ReasonPhrase}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var fileName = Path.GetFileName(remotePath);
            return new(true, "下载成功。", bytes, fileName, response.Content.Headers.ContentType?.MediaType, remotePath);
        }
        catch (Exception ex)
        {
            return new(false, $"下载失败：{ex.Message}");
        }
    }

    public async Task<FileStorageResult> DeleteFileAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiBase = await GetApiBaseAsync(cancellationToken);
            using var response = await _httpClient.DeleteAsync(BuildResourceUri(apiBase, remotePath), cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, "文件已删除。", remotePath, remotePath, AvailabilityStatus: FileAvailabilityStatus.Missing)
                : new(false, $"删除失败：{(int)response.StatusCode} {response.ReasonPhrase}", AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }
        catch (Exception ex)
        {
            return new(false, $"删除失败：{ex.Message}", AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }
    }

    public async Task<FileStorageResult> GetMetadataAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiBase = await GetApiBaseAsync(cancellationToken);
            using var response = await _httpClient.GetAsync(BuildResourceUri(apiBase, remotePath), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"文件不可访问：{(int)response.StatusCode}", remotePath, remotePath, AvailabilityStatus: FileAvailabilityStatus.Missing);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = json.RootElement;
            var size = root.TryGetProperty("size", out var sizeElement) ? sizeElement.GetInt64() : (long?)null;
            return new(true, "文件可访问。", remotePath, remotePath, SizeBytes: size, AvailabilityStatus: FileAvailabilityStatus.Available);
        }
        catch (Exception ex)
        {
            return new(false, $"文件不可访问：{ex.Message}", remotePath, remotePath, AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }
    }

    private async Task<Uri> GetApiBaseAsync(CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetActiveProfileAsync(cancellationToken);
        var normalized = string.IsNullOrWhiteSpace(profile.FileServiceApiPath) ? "/api" : profile.FileServiceApiPath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        return new Uri(new Uri(profile.FileServiceBaseUrl.TrimEnd('/') + "/"), normalized.TrimStart('/'));
    }

    private static Uri BuildResourceUri(Uri apiBase, string relativePath)
        => new(apiBase, $"resources/{EscapeRelativePath(relativePath)}");

    private static Uri BuildRawUri(Uri apiBase, string relativePath)
        => new(apiBase, $"raw/{EscapeRelativePath(relativePath)}");

    private static string EscapeRelativePath(string relativePath)
    {
        var segments = (relativePath ?? string.Empty)
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Uri.EscapeDataString);
        return string.Join('/', segments);
    }

    private static string CombinePath(string left, string right)
    {
        var normalizedLeft = (left ?? string.Empty).Trim('/');
        var normalizedRight = (right ?? string.Empty).Trim('/');
        return string.IsNullOrWhiteSpace(normalizedLeft) ? normalizedRight : $"{normalizedLeft}/{normalizedRight}";
    }
}
