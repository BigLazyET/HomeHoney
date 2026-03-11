using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public sealed class DocumentMetadataRepository : IDocumentMetadataRepository
{
    private readonly IMongoContextFactory _mongoContextFactory;

    public DocumentMetadataRepository(IMongoContextFactory mongoContextFactory)
    {
        _mongoContextFactory = mongoContextFactory;
    }

    public async Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync(CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<InsuranceRecord>("insurance_records", cancellationToken);
        return collection is null ? [] : await collection.Find(FilterDefinition<InsuranceRecord>.Empty).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync(CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<ManualRecord>("manual_records", cancellationToken);
        return collection is null ? [] : await collection.Find(FilterDefinition<ManualRecord>.Empty).ToListAsync(cancellationToken);
    }

    public async Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<InsuranceRecord>("insurance_records", cancellationToken);
        return collection is null ? null : await collection.Find(item => item.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ManualRecord?> GetManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<ManualRecord>("manual_records", cancellationToken);
        return collection is null ? null : await collection.Find(item => item.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveInsuranceRecordAsync(InsuranceRecord record, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<InsuranceRecord>("insurance_records", cancellationToken);
        if (collection is null)
        {
            return;
        }

        await collection.ReplaceOneAsync(item => item.Id == record.Id, record, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task SaveManualRecordAsync(ManualRecord record, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<ManualRecord>("manual_records", cancellationToken);
        if (collection is null)
        {
            return;
        }

        await collection.ReplaceOneAsync(item => item.Id == record.Id, record, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<bool> DeleteInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<InsuranceRecord>("insurance_records", cancellationToken);
        if (collection is null)
        {
            return false;
        }

        var result = await collection.DeleteOneAsync(item => item.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<bool> DeleteManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync<ManualRecord>("manual_records", cancellationToken);
        if (collection is null)
        {
            return false;
        }

        var result = await collection.DeleteOneAsync(item => item.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    private async Task<IMongoCollection<T>?> GetCollectionAsync<T>(string name, CancellationToken cancellationToken)
    {
        var database = await _mongoContextFactory.GetDatabaseAsync(cancellationToken);
        return database?.GetCollection<T>(name);
    }
}
