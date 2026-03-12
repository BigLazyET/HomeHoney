using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Infrastructure.Mongo.Repositories;

public sealed class DocumentRepository
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public DocumentRepository(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<InsuranceRecord>(DocumentCollections.InsuranceRecords)
                .Find(FilterDefinition<InsuranceRecord>.Empty)
                .SortBy(record => record.ExpiryDate)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items;
            }
        }

        return await _seedDataService.GetInsuranceRecordsAsync();
    }

    public async Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            return await database.GetCollection<InsuranceRecord>(DocumentCollections.InsuranceRecords)
                .Find(Builders<InsuranceRecord>.Filter.Eq(item => item.Id, id))
                .FirstOrDefaultAsync(cancellationToken);
        }

        return (await GetInsuranceRecordsAsync(cancellationToken)).FirstOrDefault(record => record.Id == id);
    }

    public async Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<ManualRecord>(DocumentCollections.ManualRecords)
                .Find(FilterDefinition<ManualRecord>.Empty)
                .SortBy(record => record.SpaceId)
                .ThenBy(record => record.Brand)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items;
            }
        }

        return await _seedDataService.GetManualRecordsAsync();
    }

    public async Task<ManualRecord?> GetManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            return await database.GetCollection<ManualRecord>(DocumentCollections.ManualRecords)
                .Find(Builders<ManualRecord>.Filter.Eq(item => item.Id, id))
                .FirstOrDefaultAsync(cancellationToken);
        }

        return (await GetManualRecordsAsync(cancellationToken)).FirstOrDefault(record => record.Id == id);
    }

    public async Task SaveInsuranceRecordAsync(InsuranceRecord record, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            await database.GetCollection<InsuranceRecord>(DocumentCollections.InsuranceRecords)
                .ReplaceOneAsync(Builders<InsuranceRecord>.Filter.Eq(item => item.Id, record.Id), record, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        await _seedDataService.SaveInsuranceRecordAsync(record);
    }

    public async Task SaveManualRecordAsync(ManualRecord record, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            await database.GetCollection<ManualRecord>(DocumentCollections.ManualRecords)
                .ReplaceOneAsync(Builders<ManualRecord>.Filter.Eq(item => item.Id, record.Id), record, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        await _seedDataService.SaveManualRecordAsync(record);
    }

    public async Task<bool> DeleteInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var result = await database.GetCollection<InsuranceRecord>(DocumentCollections.InsuranceRecords)
                .DeleteOneAsync(Builders<InsuranceRecord>.Filter.Eq(item => item.Id, id), cancellationToken);
            return result.DeletedCount > 0;
        }

        return await _seedDataService.DeleteInsuranceRecordAsync(id);
    }

    public async Task<bool> DeleteManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var result = await database.GetCollection<ManualRecord>(DocumentCollections.ManualRecords)
                .DeleteOneAsync(Builders<ManualRecord>.Filter.Eq(item => item.Id, id), cancellationToken);
            return result.DeletedCount > 0;
        }

        return await _seedDataService.DeleteManualRecordAsync(id);
    }
}
