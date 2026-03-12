using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Preferences;

public sealed class PreferenceWriteService
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public PreferenceWriteService(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<ApiOperationResult<UserPreference>> SaveAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var collection = database.GetCollection<UserPreference>(BusinessCollections.UserPreferences);
            await collection.DeleteManyAsync(FilterDefinition<UserPreference>.Empty, cancellationToken);
            await collection.InsertOneAsync(preference, cancellationToken: cancellationToken);
        }
        else
        {
            await _seedDataService.SavePreferenceAsync(preference);
        }

        return new(ApiOperationResult.Success("update-preferences", "用户偏好已更新。", targetType: "preference"), preference);
    }
}