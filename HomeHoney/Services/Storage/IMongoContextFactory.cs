using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public interface IMongoContextFactory
{
    Task<IMongoDatabase?> GetDatabaseAsync(CancellationToken cancellationToken = default);
}
