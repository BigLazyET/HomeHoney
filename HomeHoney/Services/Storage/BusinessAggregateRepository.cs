using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public sealed class BusinessAggregateRepository : IBusinessAggregateRepository
{
    private readonly IMongoContextFactory _mongoContextFactory;

    public BusinessAggregateRepository(IMongoContextFactory mongoContextFactory)
    {
        _mongoContextFactory = mongoContextFactory;
    }

    public async Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default)
        => await FindAllAsync<FridgeNote>("fridge_notes", cancellationToken);

    public async Task<FridgeNote?> GetFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
        => await FindOneAsync<FridgeNote>("fridge_notes", id, cancellationToken);

    public async Task SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default)
        => await ReplaceOneAsync("fridge_notes", fridgeNote.Id, fridgeNote, cancellationToken);

    public async Task<bool> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
        => await DeleteOneAsync<FridgeNote>("fridge_notes", id, cancellationToken);

    public async Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default)
        => await FindAllAsync<Memo>("memos", cancellationToken);

    public async Task<Memo?> GetMemoAsync(Guid id, CancellationToken cancellationToken = default)
        => await FindOneAsync<Memo>("memos", id, cancellationToken);

    public async Task SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default)
        => await ReplaceOneAsync("memos", memo.Id, memo, cancellationToken);

    public async Task<IReadOnlyList<HouseholdMember>> GetMembersAsync(CancellationToken cancellationToken = default)
        => await FindAllAsync<HouseholdMember>("household_members", cancellationToken);

    public async Task SaveMembersAsync(IReadOnlyList<HouseholdMember> members, CancellationToken cancellationToken = default)
        => await ReplaceWholeCollectionAsync("household_members", members, cancellationToken);

    public async Task<IReadOnlyList<Space>> GetSpacesAsync(CancellationToken cancellationToken = default)
        => await FindAllAsync<Space>("spaces", cancellationToken);

    public async Task SaveSpacesAsync(IReadOnlyList<Space> spaces, CancellationToken cancellationToken = default)
        => await ReplaceWholeCollectionAsync("spaces", spaces, cancellationToken);

    public async Task<UserPreference?> GetUserPreferenceAsync(CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<UserPreference>("user_preferences", cancellationToken);
        return collection is null ? null : await collection.Find(FilterDefinition<UserPreference>.Empty).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveUserPreferenceAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<UserPreference>("user_preferences", cancellationToken);
        if (collection is null)
        {
            return;
        }

        await collection.DeleteManyAsync(FilterDefinition<UserPreference>.Empty, cancellationToken);
        await collection.InsertOneAsync(preference, cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<T>> FindAllAsync<T>(string collectionName, CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync<T>(collectionName, cancellationToken);
        return collection is null ? [] : await collection.Find(FilterDefinition<T>.Empty).ToListAsync(cancellationToken);
    }

    private async Task<T?> FindOneAsync<T>(string collectionName, Guid id, CancellationToken cancellationToken) where T : class
    {
        var collection = await GetCollectionAsync<T>(collectionName, cancellationToken);
        if (collection is null)
        {
            return null;
        }

        return await collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ReplaceOneAsync<T>(string collectionName, Guid id, T entity, CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync<T>(collectionName, cancellationToken);
        if (collection is null)
        {
            return;
        }

        await collection.ReplaceOneAsync(Builders<T>.Filter.Eq("Id", id), entity, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    private async Task<bool> DeleteOneAsync<T>(string collectionName, Guid id, CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync<T>(collectionName, cancellationToken);
        if (collection is null)
        {
            return false;
        }

        var result = await collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id), cancellationToken);
        return result.DeletedCount > 0;
    }

    private async Task ReplaceWholeCollectionAsync<T>(string collectionName, IReadOnlyList<T> items, CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync<T>(collectionName, cancellationToken);
        if (collection is null)
        {
            return;
        }

        await collection.DeleteManyAsync(FilterDefinition<T>.Empty, cancellationToken);
        if (items.Count > 0)
        {
            await collection.InsertManyAsync(items, cancellationToken: cancellationToken);
        }
    }

    private async Task<IMongoCollection<T>?> GetCollectionAsync<T>(string name, CancellationToken cancellationToken)
    {
        var database = await _mongoContextFactory.GetDatabaseAsync(cancellationToken);
        return database?.GetCollection<T>(name);
    }
}
