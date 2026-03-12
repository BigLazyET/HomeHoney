using System.Text.Json;

namespace HomeHoney.Api.Infrastructure.FileBrowser;

public sealed class FileBrowserGateway : IFileBrowserGateway
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FileBrowserGateway(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<FileBrowserValidationResult> ValidateConnectionAsync(string baseUrl, string apiPath, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            return new(false, "文件服务地址不是有效的绝对地址。");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            var normalized = string.IsNullOrWhiteSpace(apiPath) ? "/api" : apiPath.Trim();
            if (!normalized.StartsWith('/'))
            {
                normalized = $"/{normalized}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUri.ToString().TrimEnd('/') + "/"), normalized.TrimStart('/')));
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, "文件服务连接正常。")
                : new(false, $"文件服务返回 {(int)response.StatusCode} {response.ReasonPhrase}。");
        }
        catch (Exception ex)
        {
            return new(false, $"无法连接文件服务：{ex.Message}");
        }
    }

    public async Task<FileBrowserUploadResult> UploadFileAsync(string baseUrl, string apiPath, string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(baseUrl, apiPath, out var apiBase, out var message))
        {
            return new(false, message);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            using var form = new MultipartFormDataContent();
            using var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType ?? "application/octet-stream");
            form.Add(streamContent, "files", fileName);

            using var response = await client.PostAsync(BuildResourceUri(apiBase!, folderPath), form, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"文件服务上传失败：{(int)response.StatusCode} {response.ReasonPhrase}。");
            }

            var remotePath = CombinePath(folderPath, fileName);
            return new(true, "上传成功。", new FileBrowserFileDescriptor
            {
                Path = remotePath,
                Name = fileName,
                SizeBytes = content.CanSeek ? content.Length : 0,
                ContentType = contentType ?? "application/octet-stream",
            });
        }
        catch (Exception ex)
        {
            return new(false, $"文件服务上传失败：{ex.Message}");
        }
    }

    public async Task<FileBrowserDownloadResult> DownloadFileAsync(string baseUrl, string apiPath, string remotePath, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(baseUrl, apiPath, out var apiBase, out var message))
        {
            return new(false, message, RemotePath: remotePath);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            using var response = await client.GetAsync(BuildRawUri(apiBase!, remotePath), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"文件服务下载失败：{(int)response.StatusCode} {response.ReasonPhrase}。", RemotePath: remotePath);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return new(true, "下载成功。", bytes, Path.GetFileName(remotePath), response.Content.Headers.ContentType?.MediaType, remotePath);
        }
        catch (Exception ex)
        {
            return new(false, $"文件服务下载失败：{ex.Message}", RemotePath: remotePath);
        }
    }

    public async Task<FileBrowserDeleteResult> DeleteFileAsync(string baseUrl, string apiPath, string remotePath, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(baseUrl, apiPath, out var apiBase, out var message))
        {
            return new(false, message);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            using var response = await client.DeleteAsync(BuildResourceUri(apiBase!, remotePath), cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, "文件已删除。")
                : new(false, $"文件服务删除失败：{(int)response.StatusCode} {response.ReasonPhrase}。");
        }
        catch (Exception ex)
        {
            return new(false, $"文件服务删除失败：{ex.Message}");
        }
    }

    private static bool TryBuildApiBase(string baseUrl, string apiPath, out Uri? apiBase, out string message)
    {
        apiBase = null;
        message = string.Empty;
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            message = "文件服务地址不是有效的绝对地址。";
            return false;
        }

        var normalized = string.IsNullOrWhiteSpace(apiPath) ? "/api" : apiPath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        apiBase = new Uri(new Uri(baseUri.ToString().TrimEnd('/') + "/"), normalized.TrimStart('/'));
        return true;
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
