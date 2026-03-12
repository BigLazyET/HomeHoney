using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Infrastructure.Mongo.Repositories;

public sealed class CollaborationRepository
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public CollaborationRepository(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<FridgeNote>(BusinessCollections.FridgeNotes)
                .Find(FilterDefinition<FridgeNote>.Empty)
                .SortByDescending(note => note.IsPinned)
                .ThenBy(note => note.DueAt)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items;
            }
        }

        return await _seedDataService.GetFridgeNotesAsync();
    }

    public async Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<Memo>(BusinessCollections.Memos)
                .Find(FilterDefinition<Memo>.Empty)
                .SortBy(memo => memo.DueAt)
                .ThenByDescending(memo => memo.UpdatedAt)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items;
            }
        }

        return await _seedDataService.GetMemosAsync();
    }

    public async Task SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            await database.GetCollection<FridgeNote>(BusinessCollections.FridgeNotes)
                .ReplaceOneAsync(Builders<FridgeNote>.Filter.Eq(item => item.Id, fridgeNote.Id), fridgeNote, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        await _seedDataService.SaveFridgeNoteAsync(fridgeNote);
    }

    public async Task SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            await database.GetCollection<Memo>(BusinessCollections.Memos)
                .ReplaceOneAsync(Builders<Memo>.Filter.Eq(item => item.Id, memo.Id), memo, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        await _seedDataService.SaveMemoAsync(memo);
    }

    public async Task<bool> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var result = await database.GetCollection<FridgeNote>(BusinessCollections.FridgeNotes)
                .DeleteOneAsync(Builders<FridgeNote>.Filter.Eq(item => item.Id, id), cancellationToken);
            return result.DeletedCount > 0;
        }

        return await _seedDataService.DeleteFridgeNoteAsync(id);
    }

    public async Task<bool> DeleteMemoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var result = await database.GetCollection<Memo>(BusinessCollections.Memos)
                .DeleteOneAsync(Builders<Memo>.Filter.Eq(item => item.Id, id), cancellationToken);
            return result.DeletedCount > 0;
        }

        return await _seedDataService.DeleteMemoAsync(id);
    }
}
