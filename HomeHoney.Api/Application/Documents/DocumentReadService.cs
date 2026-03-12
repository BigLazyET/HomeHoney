using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Documents;

public sealed class DocumentReadService
{
    private const string InsuranceCollection = "insurance_records";
    private const string ManualCollection = "manual_records";

    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;

    public DocumentReadService(IMongoDatabaseFactory mongoDatabaseFactory, SeedDataService seedDataService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
    }

    public async Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<InsuranceRecord>(InsuranceCollection)
                .Find(FilterDefinition<InsuranceRecord>.Empty)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items.OrderBy(record => record.ExpiryDate).ToList();
            }
        }

        return await _seedDataService.GetInsuranceRecordsAsync();
    }

    public async Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => (await GetInsuranceRecordsAsync(cancellationToken)).FirstOrDefault(record => record.Id == id);

    public async Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var items = await database.GetCollection<ManualRecord>(ManualCollection)
                .Find(FilterDefinition<ManualRecord>.Empty)
                .ToListAsync(cancellationToken);
            if (items.Count > 0)
            {
                return items.OrderBy(record => record.SpaceId).ToList();
            }
        }

        return await _seedDataService.GetManualRecordsAsync();
    }

    public async Task<ManualRecord?> GetManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
        => (await GetManualRecordsAsync(cancellationToken)).FirstOrDefault(record => record.Id == id);

    public IReadOnlyList<HouseholdMember> GetMembers() => _seedDataService.GetMembers();

    public IReadOnlyList<Space> GetSpaces() => _seedDataService.GetSpaces();
}