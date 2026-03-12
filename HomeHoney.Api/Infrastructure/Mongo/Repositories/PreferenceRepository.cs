using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Infrastructure.Mongo.Repositories;

public sealed class PreferenceRepository
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public PreferenceRepository(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<UserPreference> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var existing = await database.GetCollection<UserPreference>(BusinessCollections.UserPreferences)
                .Find(FilterDefinition<UserPreference>.Empty)
                .SortByDescending(item => item.StoragePreference.LastValidatedAt)
                .Limit(1)
                .FirstOrDefaultAsync(cancellationToken);
            if (existing is not null)
            {
                return existing;
            }
        }

        return _seedDataService.GetDefaultPreference();
    }

    public async Task SaveAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var collection = database.GetCollection<UserPreference>(BusinessCollections.UserPreferences);
            await collection.DeleteManyAsync(FilterDefinition<UserPreference>.Empty, cancellationToken);
            await collection.InsertOneAsync(preference, cancellationToken: cancellationToken);
            return;
        }

        await _seedDataService.SavePreferenceAsync(preference);
    }
}
