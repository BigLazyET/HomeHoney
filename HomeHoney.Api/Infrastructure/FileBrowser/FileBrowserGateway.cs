using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HomeHoney.Api.Infrastructure.FileBrowser;

public sealed class FileBrowserGateway : IFileBrowserGateway
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FileBrowserGateway(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<FileBrowserValidationResult> ValidateConnectionAsync(FileBrowserConnectionOptions connectionOptions, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(connectionOptions, out var apiBase, out var message))
        {
            return new(false, message);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            var auth = await AuthenticateAsync(client, apiBase!, connectionOptions, cancellationToken);
            if (!auth.IsSuccess)
            {
                return new(false, auth.Message);
            }

            using var request = CreateRequest(HttpMethod.Get, BuildResourceUri(apiBase!, string.Empty), auth.Token, connectionOptions);
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, "文件服务连接正常。")
                : new(false, await BuildFailureMessageAsync("文件服务连接失败", response, cancellationToken));
        }
        catch (Exception ex)
        {
            return new(false, $"无法连接文件服务：{ex.Message}");
        }
    }

    public async Task<FileBrowserUploadResult> UploadFileAsync(FileBrowserConnectionOptions connectionOptions, string folderPath, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(connectionOptions, out var apiBase, out var message))
        {
            return new(false, message);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            var auth = await AuthenticateAsync(client, apiBase!, connectionOptions, cancellationToken);
            if (!auth.IsSuccess)
            {
                return new(false, auth.Message);
            }

            using var form = new MultipartFormDataContent();
            using var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType ?? "application/octet-stream");
            form.Add(streamContent, "files", fileName);

            using var request = CreateRequest(HttpMethod.Post, BuildResourceUri(apiBase!, folderPath), auth.Token, connectionOptions);
            request.Content = form;
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, await BuildFailureMessageAsync("文件服务上传失败", response, cancellationToken));
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

    public async Task<FileBrowserDownloadResult> DownloadFileAsync(FileBrowserConnectionOptions connectionOptions, string remotePath, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(connectionOptions, out var apiBase, out var message))
        {
            return new(false, message, RemotePath: remotePath);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            var auth = await AuthenticateAsync(client, apiBase!, connectionOptions, cancellationToken);
            if (!auth.IsSuccess)
            {
                return new(false, auth.Message, RemotePath: remotePath);
            }

            using var request = CreateRequest(HttpMethod.Get, BuildRawUri(apiBase!, remotePath), auth.Token, connectionOptions);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, await BuildFailureMessageAsync("文件服务下载失败", response, cancellationToken), RemotePath: remotePath);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return new(true, "下载成功。", bytes, Path.GetFileName(remotePath), response.Content.Headers.ContentType?.MediaType, remotePath);
        }
        catch (Exception ex)
        {
            return new(false, $"文件服务下载失败：{ex.Message}", RemotePath: remotePath);
        }
    }

    public async Task<FileBrowserDeleteResult> DeleteFileAsync(FileBrowserConnectionOptions connectionOptions, string remotePath, CancellationToken cancellationToken = default)
    {
        if (!TryBuildApiBase(connectionOptions, out var apiBase, out var message))
        {
            return new(false, message);
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(FileBrowserGateway));
            var auth = await AuthenticateAsync(client, apiBase!, connectionOptions, cancellationToken);
            if (!auth.IsSuccess)
            {
                return new(false, auth.Message);
            }

            using var request = CreateRequest(HttpMethod.Delete, BuildResourceUri(apiBase!, remotePath), auth.Token, connectionOptions);
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, "文件已删除。")
                : new(false, await BuildFailureMessageAsync("文件服务删除失败", response, cancellationToken));
        }
        catch (Exception ex)
        {
            return new(false, $"文件服务删除失败：{ex.Message}");
        }
    }

    private static bool TryBuildApiBase(FileBrowserConnectionOptions connectionOptions, out Uri? apiBase, out string message)
    {
        apiBase = null;
        message = string.Empty;
        if (!Uri.TryCreate(connectionOptions.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            message = "文件服务地址不是有效的绝对地址。";
            return false;
        }

        var normalized = string.IsNullOrWhiteSpace(connectionOptions.ApiPath) ? "/api" : connectionOptions.ApiPath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        if (!normalized.EndsWith('/'))
        {
            normalized = $"{normalized}/";
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

    private static HttpRequestMessage CreateRequest(HttpMethod method, Uri uri, string? token, FileBrowserConnectionOptions connectionOptions)
    {
        var request = new HttpRequestMessage(method, uri);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation("X-Auth", token);
        }

        if (!string.IsNullOrWhiteSpace(connectionOptions.FileServiceAuthHeaderName) && !string.IsNullOrWhiteSpace(connectionOptions.FileServiceAuthHeaderValue))
        {
            request.Headers.TryAddWithoutValidation(connectionOptions.FileServiceAuthHeaderName, connectionOptions.FileServiceAuthHeaderValue);
        }

        return request;
    }

    private static async Task<string> BuildFailureMessageAsync(string prefix, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if ((response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden) && string.IsNullOrWhiteSpace(body))
        {
            return $"{prefix}：FileBrowser 拒绝了当前认证信息（{(int)response.StatusCode} {response.ReasonPhrase}）。";
        }

        return string.IsNullOrWhiteSpace(body)
            ? $"{prefix}：{(int)response.StatusCode} {response.ReasonPhrase}。"
            : $"{prefix}：{body.Trim()}";
    }

    private static Uri BuildLoginUri(Uri apiBase)
        => new(apiBase, "login");

    private static async Task<FileBrowserAuthResult> AuthenticateAsync(HttpClient client, Uri apiBase, FileBrowserConnectionOptions connectionOptions, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionOptions.FileServiceAuthHeaderName) || !string.IsNullOrWhiteSpace(connectionOptions.FileServiceAuthHeaderValue))
        {
            if (string.IsNullOrWhiteSpace(connectionOptions.FileServiceAuthHeaderName) || string.IsNullOrWhiteSpace(connectionOptions.FileServiceAuthHeaderValue))
            {
                return new(false, null, "FileBrowser 代理头认证未完整配置。", UsedAuthentication: true);
            }

            return new(true, null, "FileBrowser 代理头认证已附加。", UsedAuthentication: true);
        }

        if (string.IsNullOrWhiteSpace(connectionOptions.FileServiceUsername) && string.IsNullOrWhiteSpace(connectionOptions.FileServicePassword))
        {
            return new(true, null, "未配置 FileBrowser 认证，按匿名方式访问。", UsedAuthentication: false);
        }

        if (string.IsNullOrWhiteSpace(connectionOptions.FileServiceUsername) || string.IsNullOrWhiteSpace(connectionOptions.FileServicePassword))
        {
            return new(false, null, "FileBrowser JSON 认证需要同时提供用户名和密码。", UsedAuthentication: true);
        }

        using var response = await client.PostAsJsonAsync(BuildLoginUri(apiBase), new
        {
            username = connectionOptions.FileServiceUsername,
            password = connectionOptions.FileServicePassword,
            recaptcha = string.Empty,
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new(false, null, await BuildFailureMessageAsync("FileBrowser 登录失败", response, cancellationToken), UsedAuthentication: true);
        }

        var token = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new(false, null, "FileBrowser 登录成功，但未返回访问令牌。", UsedAuthentication: true);
        }

        return new(true, token, "FileBrowser 登录成功。", UsedAuthentication: true);
    }

    private sealed record FileBrowserAuthResult(bool IsSuccess, string? Token, string Message, bool UsedAuthentication);
}
