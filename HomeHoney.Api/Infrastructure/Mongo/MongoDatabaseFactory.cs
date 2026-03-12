using HomeHoney.Api.Infrastructure.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HomeHoney.Api.Infrastructure.Mongo;

public sealed class MongoDatabaseFactory : IMongoDatabaseFactory
{
    private readonly SecureSettingsStore _secureSettingsStore;

    public MongoDatabaseFactory(SecureSettingsStore secureSettingsStore)
    {
        _secureSettingsStore = secureSettingsStore;
    }

    public Task<IMongoDatabase?> GetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var options = _secureSettingsStore.GetEffectiveStorageOptions();
        if (string.IsNullOrWhiteSpace(options.MongoConnectionString) || string.IsNullOrWhiteSpace(options.MongoDatabaseName))
        {
            return Task.FromResult<IMongoDatabase?>(null);
        }

        var client = new MongoClient(options.MongoConnectionString);
        return Task.FromResult<IMongoDatabase?>(client.GetDatabase(options.MongoDatabaseName));
    }

    public async Task<MongoValidationResult> ValidateAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ConnectTimeout = TimeSpan.FromSeconds(3);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(3);
            var client = new MongoClient(settings);
            await client.GetDatabase(databaseName).RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
            return new(true, "MongoDB 连接正常。");
        }
        catch (Exception ex)
        {
            return new(false, $"无法连接 MongoDB：{ex.Message}");
        }
    }
}
