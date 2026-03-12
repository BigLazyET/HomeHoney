using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public sealed class MongoContextFactory : IMongoContextFactory
{
    public MongoContextFactory(IStorageConnectionProfileService profileService)
    {
    }

    public Task<IMongoDatabase?> GetDatabaseAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IMongoDatabase?>(null);
}
