using HomeHoney.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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
        if (!Uri.TryCreate(profile.FileServiceBaseUrl, UriKind.Absolute, out var fileServiceUri))
        {
            return new(false, "文件服务地址不是有效的绝对地址。");
        }

        try
        {
            MongoUrl.Create(mongoConnectionString);
        }
        catch (Exception ex)
        {
            return new(false, $"Mongo 连接信息无效：{ex.Message}");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiBaseUri(fileServiceUri, profile.FileServiceApiPath));
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, $"文件服务返回 {(int)response.StatusCode}，请检查地址或登录状态。");
            }
        }
        catch (Exception ex)
        {
            return new(false, $"无法连接文件服务：{ex.Message}");
        }

        try
        {
            var settings = MongoClientSettings.FromConnectionString(mongoConnectionString);
            settings.ConnectTimeout = TimeSpan.FromSeconds(3);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);
            var client = new MongoClient(settings);
            await client.GetDatabase(profile.MongoDatabaseName).RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            return new(false, $"无法连接 MongoDB：{ex.Message}");
        }

        return new(true, "连接验证通过。");
    }

    private static Uri BuildApiBaseUri(Uri fileServiceUri, string apiPath)
    {
        var normalized = string.IsNullOrWhiteSpace(apiPath) ? "/api" : apiPath.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        return new Uri(fileServiceUri, normalized);
    }
}
