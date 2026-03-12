using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Preferences;

public sealed class PreferenceReadService
{
    private const string PreferenceCollection = "user_preferences";

    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public PreferenceReadService(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<UserPreference> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var existing = await database.GetCollection<UserPreference>(PreferenceCollection)
                .Find(FilterDefinition<UserPreference>.Empty)
                .FirstOrDefaultAsync(cancellationToken);
            if (existing is not null)
            {
                return existing;
            }
        }

        return _seedDataService.GetDefaultPreference();
    }
}