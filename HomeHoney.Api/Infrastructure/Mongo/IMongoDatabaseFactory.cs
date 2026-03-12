using MongoDB.Driver;

namespace HomeHoney.Api.Infrastructure.Mongo;

public sealed record MongoValidationResult(bool IsSuccess, string Message);

public interface IMongoDatabaseFactory
{
    Task<IMongoDatabase?> GetDatabaseAsync(CancellationToken cancellationToken = default);

    Task<MongoValidationResult> ValidateAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default);
}
