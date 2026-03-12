using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Collaboration;

public sealed class CollaborationWriteService
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public CollaborationWriteService(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<ApiOperationResult<FridgeNote>> SaveFridgeNoteAsync(FridgeNote fridgeNote, CancellationToken cancellationToken = default)
    {
        fridgeNote.Id = fridgeNote.Id == Guid.Empty ? Guid.NewGuid() : fridgeNote.Id;
        fridgeNote.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(BusinessCollections.FridgeNotes, fridgeNote, cancellationToken);
        return new(ApiOperationResult.Success("upsert-fridge-note", "冰箱贴已保存。", targetType: "fridge-note", targetId: fridgeNote.Id), fridgeNote);
    }

    public async Task<ApiOperationResult<Memo>> SaveMemoAsync(Memo memo, CancellationToken cancellationToken = default)
    {
        memo.Id = memo.Id == Guid.Empty ? Guid.NewGuid() : memo.Id;
        memo.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(BusinessCollections.Memos, memo, cancellationToken);
        return new(ApiOperationResult.Success("upsert-memo", "备忘录已保存。", targetType: "memo", targetId: memo.Id), memo);
    }

    public async Task<ApiOperationResult> DeleteFridgeNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = await DeleteAsync<FridgeNote>(BusinessCollections.FridgeNotes, id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-fridge-note", "冰箱贴已删除。", targetType: "fridge-note", targetId: id)
            : ApiOperationResult.Failure("delete-fridge-note", "未找到要删除的冰箱贴。", "not_found", targetType: "fridge-note", targetId: id);
    }

    public async Task<ApiOperationResult> DeleteMemoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = await DeleteAsync<Memo>(BusinessCollections.Memos, id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-memo", "备忘录已删除。", targetType: "memo", targetId: id)
            : ApiOperationResult.Failure("delete-memo", "未找到要删除的备忘录。", "not_found", targetType: "memo", targetId: id);
    }

    private async Task SaveAsync<T>(string collectionName, T entity, CancellationToken cancellationToken)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            await database.GetCollection<T>(collectionName)
                .ReplaceOneAsync(Builders<T>.Filter.Eq("Id", entity?.GetType().GetProperty("Id")?.GetValue(entity)), entity, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        switch (entity)
        {
            case FridgeNote note:
                await _seedDataService.SaveFridgeNoteAsync(note);
                break;
            case Memo memo:
                await _seedDataService.SaveMemoAsync(memo);
                break;
        }
    }

    private async Task<bool> DeleteAsync<T>(string collectionName, Guid id, CancellationToken cancellationToken)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var result = await database.GetCollection<T>(collectionName).DeleteOneAsync(Builders<T>.Filter.Eq("Id", id), cancellationToken);
            return result.DeletedCount > 0;
        }

        return typeof(T) == typeof(FridgeNote)
            ? await _seedDataService.DeleteFridgeNoteAsync(id)
            : await _seedDataService.DeleteMemoAsync(id);
    }
}