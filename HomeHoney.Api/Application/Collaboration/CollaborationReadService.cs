using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Collaboration;

public sealed class CollaborationReadService
{
    private const string FridgeNoteCollection = "fridge_notes";
    private const string MemoCollection = "memos";

    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public CollaborationReadService(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<IReadOnlyList<FridgeNote>> GetFridgeNotesAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<FridgeNote>(FridgeNoteCollection)
                .Find(FilterDefinition<FridgeNote>.Empty)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items.OrderByDescending(note => note.IsPinned).ThenBy(note => note.DueAt).ToList();
            }
        }

        return await _seedDataService.GetFridgeNotesAsync();
    }

    public async Task<IReadOnlyList<Memo>> GetMemosAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<Memo>(MemoCollection)
                .Find(FilterDefinition<Memo>.Empty)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items.OrderBy(memo => memo.DueAt).ToList();
            }
        }

        return await _seedDataService.GetMemosAsync();
    }
}