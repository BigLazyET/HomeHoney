using HomeHoney.Api.Contracts.Storage;
using HomeHoney.Api.Infrastructure.Configuration;
using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Api.Infrastructure.Mongo;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Storage;

public sealed class StorageValidationService
{
    private readonly IFileBrowserGateway _fileBrowserGateway;
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SecureSettingsStore _secureSettingsStore;

    public StorageValidationService(IFileBrowserGateway fileBrowserGateway, IMongoDatabaseFactory mongoDatabaseFactory, SecureSettingsStore secureSettingsStore)
    {
        _fileBrowserGateway = fileBrowserGateway;
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _secureSettingsStore = secureSettingsStore;
    }

    public async Task<string?> ValidateAsync(UpdateDownstreamStorageSettingsRequest request, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(request.FileServiceBaseUrl, UriKind.Absolute, out _))
        {
            return "文件服务地址不是有效的绝对地址。";
        }

        if (string.IsNullOrWhiteSpace(request.MongoDatabaseName))
        {
            return "Mongo 数据库名不能为空。";
        }

        var effectiveOptions = _secureSettingsStore.GetEffectiveStorageOptions(request);
        if (string.IsNullOrWhiteSpace(effectiveOptions.FileServiceUsername) ^ string.IsNullOrWhiteSpace(effectiveOptions.FileServicePassword))
        {
            return "FileBrowser JSON 认证需要同时提供用户名和密码。";
        }

        if (string.IsNullOrWhiteSpace(effectiveOptions.FileServiceAuthHeaderName) ^ string.IsNullOrWhiteSpace(effectiveOptions.FileServiceAuthHeaderValue))
        {
            return "FileBrowser 代理头认证需要同时提供请求头名称和请求头值。";
        }

        if ((!string.IsNullOrWhiteSpace(effectiveOptions.FileServiceUsername) || !string.IsNullOrWhiteSpace(effectiveOptions.FileServicePassword)) &&
            (!string.IsNullOrWhiteSpace(effectiveOptions.FileServiceAuthHeaderName) || !string.IsNullOrWhiteSpace(effectiveOptions.FileServiceAuthHeaderValue)))
        {
            return "FileBrowser 认证请在 JSON 登录 和 代理头 两种方式中二选一。";
        }

        try
        {
            MongoClientSettings.FromConnectionString(request.MongoConnectionString);
        }
        catch (Exception ex)
        {
            return $"Mongo 连接字符串无效：{ex.Message}";
        }

        var fileResult = await _fileBrowserGateway.ValidateConnectionAsync(new FileBrowserConnectionOptions
        {
            BaseUrl = effectiveOptions.FileServiceBaseUrl,
            ApiPath = effectiveOptions.FileServiceApiPath,
            FileServiceUsername = effectiveOptions.FileServiceUsername,
            FileServicePassword = effectiveOptions.FileServicePassword,
            FileServiceAuthHeaderName = effectiveOptions.FileServiceAuthHeaderName,
            FileServiceAuthHeaderValue = effectiveOptions.FileServiceAuthHeaderValue,
        }, cancellationToken);
        if (!fileResult.IsSuccess)
        {
            return fileResult.Message;
        }

        var mongoResult = await _mongoDatabaseFactory.ValidateAsync(request.MongoConnectionString, request.MongoDatabaseName, cancellationToken);
        return mongoResult.IsSuccess ? null : mongoResult.Message;
    }
}
