using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using MongoDB.Driver;

namespace HomeHoney.Api.Infrastructure.Mongo;

public sealed class MongoIndexInitializer
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(IMongoDatabaseFactory mongoDatabaseFactory, ILogger<MongoIndexInitializer> logger)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _logger = logger;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is null)
        {
            _logger.LogInformation("Mongo 索引初始化已跳过：当前未配置可用数据库。");
            return;
        }

        await EnsureInsuranceIndexesAsync(database, cancellationToken);
        await EnsureManualIndexesAsync(database, cancellationToken);
        await EnsureFridgeNoteIndexesAsync(database, cancellationToken);
        await EnsureMemoIndexesAsync(database, cancellationToken);
        await EnsurePreferenceIndexesAsync(database, cancellationToken);
        await EnsureHouseholdMemberIndexesAsync(database, cancellationToken);
        await EnsureSpaceIndexesAsync(database, cancellationToken);

        _logger.LogInformation("Mongo 索引初始化完成。");
    }

    private static Task EnsureInsuranceIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<InsuranceRecord>(DocumentCollections.InsuranceRecords).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<InsuranceRecord>(Builders<InsuranceRecord>.IndexKeys.Ascending(record => record.ExpiryDate), new CreateIndexOptions { Name = "insurance_expiry_date" }),
                new CreateIndexModel<InsuranceRecord>(Builders<InsuranceRecord>.IndexKeys.Ascending(record => record.Status).Ascending(record => record.ExpiryDate), new CreateIndexOptions { Name = "insurance_status_expiry" }),
                new CreateIndexModel<InsuranceRecord>(Builders<InsuranceRecord>.IndexKeys.Ascending(record => record.InsuredMemberId).Ascending(record => record.ExpiryDate), new CreateIndexOptions { Name = "insurance_member_expiry" }),
                new CreateIndexModel<InsuranceRecord>(Builders<InsuranceRecord>.IndexKeys.Descending(record => record.LastUpdatedAt), new CreateIndexOptions { Name = "insurance_updated_at" }),
                new CreateIndexModel<InsuranceRecord>(Builders<InsuranceRecord>.IndexKeys.Ascending(record => record.PolicyName).Ascending(record => record.ProviderName), new CreateIndexOptions { Name = "insurance_name_provider" }),
            ],
            cancellationToken);

    private static Task EnsureManualIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<ManualRecord>(DocumentCollections.ManualRecords).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<ManualRecord>(Builders<ManualRecord>.IndexKeys.Ascending(record => record.SpaceId).Ascending(record => record.Brand), new CreateIndexOptions { Name = "manual_space_brand" }),
                new CreateIndexModel<ManualRecord>(Builders<ManualRecord>.IndexKeys.Ascending(record => record.WarrantyExpiryDate), new CreateIndexOptions { Name = "manual_warranty_expiry" }),
                new CreateIndexModel<ManualRecord>(Builders<ManualRecord>.IndexKeys.Descending(record => record.LastUpdatedAt), new CreateIndexOptions { Name = "manual_updated_at" }),
                new CreateIndexModel<ManualRecord>(Builders<ManualRecord>.IndexKeys.Ascending(record => record.DeviceName).Ascending(record => record.Brand), new CreateIndexOptions { Name = "manual_device_brand" }),
            ],
            cancellationToken);

    private static Task EnsureFridgeNoteIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<FridgeNote>(BusinessCollections.FridgeNotes).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<FridgeNote>(Builders<FridgeNote>.IndexKeys.Descending(note => note.IsPinned).Ascending(note => note.DueAt), new CreateIndexOptions { Name = "fridge_pinned_due" }),
                new CreateIndexModel<FridgeNote>(Builders<FridgeNote>.IndexKeys.Ascending(note => note.IsCompleted).Ascending(note => note.DueAt), new CreateIndexOptions { Name = "fridge_completed_due" }),
                new CreateIndexModel<FridgeNote>(Builders<FridgeNote>.IndexKeys.Descending(note => note.UpdatedAt), new CreateIndexOptions { Name = "fridge_updated_at" }),
                new CreateIndexModel<FridgeNote>(Builders<FridgeNote>.IndexKeys.Ascending(note => note.Title), new CreateIndexOptions { Name = "fridge_title" }),
            ],
            cancellationToken);

    private static Task EnsureMemoIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<Memo>(BusinessCollections.Memos).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<Memo>(Builders<Memo>.IndexKeys.Ascending(memo => memo.Status).Ascending(memo => memo.DueAt), new CreateIndexOptions { Name = "memo_status_due" }),
                new CreateIndexModel<Memo>(Builders<Memo>.IndexKeys.Descending(memo => memo.UpdatedAt), new CreateIndexOptions { Name = "memo_updated_at" }),
                new CreateIndexModel<Memo>(Builders<Memo>.IndexKeys.Ascending(memo => memo.Title), new CreateIndexOptions { Name = "memo_title" }),
            ],
            cancellationToken);

    private static Task EnsurePreferenceIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<UserPreference>(BusinessCollections.UserPreferences).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<UserPreference>(Builders<UserPreference>.IndexKeys.Ascending(preference => preference.StoragePreference.IsActive), new CreateIndexOptions { Name = "preference_active_profile" }),
                new CreateIndexModel<UserPreference>(Builders<UserPreference>.IndexKeys.Descending(preference => preference.StoragePreference.LastValidatedAt), new CreateIndexOptions { Name = "preference_validated_at" }),
            ],
            cancellationToken);

    private static Task EnsureHouseholdMemberIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<HouseholdMember>(BusinessCollections.HouseholdMembers).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<HouseholdMember>(Builders<HouseholdMember>.IndexKeys.Ascending(member => member.IsPrimary), new CreateIndexOptions { Name = "member_primary" }),
                new CreateIndexModel<HouseholdMember>(Builders<HouseholdMember>.IndexKeys.Ascending(member => member.DisplayName), new CreateIndexOptions { Name = "member_display_name" }),
            ],
            cancellationToken);

    private static Task EnsureSpaceIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
        => database.GetCollection<Space>(BusinessCollections.Spaces).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<Space>(Builders<Space>.IndexKeys.Ascending(space => space.SortOrder), new CreateIndexOptions { Name = "space_sort_order" }),
                new CreateIndexModel<Space>(Builders<Space>.IndexKeys.Ascending(space => space.Name), new CreateIndexOptions { Name = "space_name" }),
            ],
            cancellationToken);
}
