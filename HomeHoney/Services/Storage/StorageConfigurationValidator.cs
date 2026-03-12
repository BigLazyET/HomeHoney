using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed class StorageConfigurationValidator
{
    private readonly HttpClient _httpClient;

    public StorageConfigurationValidator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StorageValidationResult> ValidateAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(profile.ApiBaseUrl, UriKind.Absolute, out var backendUri))
        {
            return new(false, "后端 API 地址不是有效的绝对地址。");
        }

        if (string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            return new(false, "配置名称不能为空。");
        }

        if (string.IsNullOrWhiteSpace(profile.MongoDatabaseName))
        {
            return new(false, "Mongo 数据库名不能为空。");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildHealthUri(backendUri));
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"后端服务返回 {(int)response.StatusCode}，请检查地址或服务状态。");
            }
        }
        catch (Exception ex)
        {
            return new(false, $"无法连接后端服务：{ex.Message}");
        }

        return new(true, "后端连接验证通过。");
    }

    private static Uri BuildHealthUri(Uri backendUri)
        => new(new Uri(backendUri.ToString().TrimEnd('/') + "/"), "health");
}
