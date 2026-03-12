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

    public StorageValidationService(IFileBrowserGateway fileBrowserGateway, IMongoDatabaseFactory mongoDatabaseFactory)
    {
        _fileBrowserGateway = fileBrowserGateway;
        _mongoDatabaseFactory = mongoDatabaseFactory;
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

        try
        {
            MongoClientSettings.FromConnectionString(request.MongoConnectionString);
        }
        catch (Exception ex)
        {
            return $"Mongo 连接字符串无效：{ex.Message}";
        }

        var fileResult = await _fileBrowserGateway.ValidateConnectionAsync(request.FileServiceBaseUrl, request.FileServiceApiPath, cancellationToken);
        if (!fileResult.IsSuccess)
        {
            return fileResult.Message;
        }

        var mongoResult = await _mongoDatabaseFactory.ValidateAsync(request.MongoConnectionString, request.MongoDatabaseName, cancellationToken);
        return mongoResult.IsSuccess ? null : mongoResult.Message;
    }
}
