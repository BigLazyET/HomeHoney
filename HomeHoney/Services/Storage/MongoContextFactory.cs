using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public sealed class MongoContextFactory : IMongoContextFactory
{
    private readonly IStorageConnectionProfileService _profileService;

    public MongoContextFactory(IStorageConnectionProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<IMongoDatabase?> GetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetActiveProfileAsync(cancellationToken);
        var connectionString = await _profileService.GetMongoConnectionStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(profile.MongoDatabaseName))
        {
            return null;
        }

        try
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(profile.MongoDatabaseName);
        }
        catch
        {
            return null;
        }
    }
}
