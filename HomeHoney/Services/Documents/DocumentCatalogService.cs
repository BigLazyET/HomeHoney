using HomeHoney.Models;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Storage;

namespace HomeHoney.Services.Documents;

public sealed class DocumentCatalogService
{
    private readonly List<InsuranceRecord> _insuranceRecords;
    private readonly List<ManualRecord> _manualRecords;
    private readonly List<HouseholdMember> _members;
    private readonly List<Space> _spaces;
    private readonly IStorageConnectionProfileService? _storageConnectionProfileService;
    private readonly IDocumentMetadataRepository? _documentMetadataRepository;
    private readonly IBusinessAggregateRepository? _businessAggregateRepository;
    private readonly DocumentFileOrchestrator? _documentFileOrchestrator;
    private readonly IDocumentApiClient? _documentApiClient;

    public DocumentCatalogService()
    {
        _insuranceRecords = [];
        _manualRecords = [];
        _members = [];
        _spaces = [];
        SeedLocalData();
    }

    public DocumentCatalogService(
        IStorageConnectionProfileService storageConnectionProfileService,
        IDocumentMetadataRepository documentMetadataRepository,
        IBusinessAggregateRepository businessAggregateRepository,
        DocumentFileOrchestrator documentFileOrchestrator)
    {
        _storageConnectionProfileService = storageConnectionProfileService;
        _documentMetadataRepository = documentMetadataRepository;
        _businessAggregateRepository = businessAggregateRepository;
        _documentFileOrchestrator = documentFileOrchestrator;
        _insuranceRecords = [];
        _manualRecords = [];
        _members = [];
        _spaces = [];
        SeedLocalData();
    }

    public DocumentCatalogService(
        IStorageConnectionProfileService storageConnectionProfileService,
        IDocumentMetadataRepository documentMetadataRepository,
        IBusinessAggregateRepository businessAggregateRepository,
        DocumentFileOrchestrator documentFileOrchestrator,
        IDocumentApiClient documentApiClient)
        : this(storageConnectionProfileService, documentMetadataRepository, businessAggregateRepository, documentFileOrchestrator)
    {
        _documentApiClient = documentApiClient;
    }

    private void SeedLocalData()
    {
        _members.Clear();
        _members.AddRange(
        [
            new() { Id = Guid.Parse("f4aab8d3-c8db-4f45-8f45-6e0eb50a1001"), DisplayName = "ET", Role = HouseholdRole.Self, AvatarColor = "#007aff", IsPrimary = true },
            new() { Id = Guid.Parse("f4aab8d3-c8db-4f45-8f45-6e0eb50a1002"), DisplayName = "妈妈", Role = HouseholdRole.Parent, AvatarColor = "#ff9500" },
        ]);

        _spaces.Clear();
        _spaces.AddRange(
        [
            new() { Id = Guid.Parse("6dbc1d83-67ab-4456-b62b-15f4c88c1001"), Name = "厨房", Icon = "🍳", SortOrder = 1 },
            new() { Id = Guid.Parse("6dbc1d83-67ab-4456-b62b-15f4c88c1002"), Name = "客厅", Icon = "🛋️", SortOrder = 2 },
        ]);

        _insuranceRecords.Clear();
        _insuranceRecords.AddRange(
        [
            new()
            {
                Id = Guid.Parse("9a7eb129-a0a3-4f2c-bae0-4dc8f56d1001"),
                PolicyName = "家庭综合医疗险",
                InsuredMemberId = _members[0].Id,
                InsuranceCategory = InsuranceCategory.Medical,
                Status = InsuranceStatus.Active,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(5)),
                ProviderName = "平安保险",
                ContactName = "专属顾问 王女士",
                ContactPhone = "400-800-0001",
                Summary = "覆盖住院、门诊与意外医疗，适合作为家庭基础保障。",
                AttachmentCount = 1,
                PrimaryFile = new FileResource
                {
                    FileName = "family-medical-policy.pdf",
                    ExternalPath = "seed/insurance/family-medical-policy.pdf",
                    AvailabilityStatus = FileAvailabilityStatus.Available,
                    ContentType = "application/pdf",
                    SizeBytes = 512000,
                },
                Tags = ["医疗", "年度续保", "家庭"]
            },
            new()
            {
                Id = Guid.Parse("9a7eb129-a0a3-4f2c-bae0-4dc8f56d1002"),
                PolicyName = "儿童意外险",
                InsuredMemberId = _members[1].Id,
                InsuranceCategory = InsuranceCategory.Accident,
                Status = InsuranceStatus.PendingRenewal,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(18)),
                ProviderName = "中国人保",
                ContactName = "理赔热线",
                ContactPhone = "95518",
                Summary = "儿童日常意外、骨折与门急诊意外保障。",
                AttachmentCount = 1,
                PrimaryFile = new FileResource
                {
                    FileName = "kids-accident-policy.pdf",
                    ExternalPath = "seed/insurance/kids-accident-policy.pdf",
                    AvailabilityStatus = FileAvailabilityStatus.Available,
                    ContentType = "application/pdf",
                    SizeBytes = 384000,
                },
                Tags = ["儿童", "意外", "即将到期"]
            },
        ]);

        _manualRecords.Clear();
        _manualRecords.AddRange(
        [
            new()
            {
                Id = Guid.Parse("00ab2f67-1f54-4080-b8ae-9c70426f1001"),
                DeviceName = "海尔冰箱",
                Brand = "Haier",
                Model = "BCD-501WGHFD14S8U1",
                SpaceId = _spaces[0].Id,
                ManualCategory = ManualCategory.Appliance,
                PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                WarrantyExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddYears(2)),
                Summary = "冰箱说明书、保养建议与故障代码索引。",
                AttachmentCount = 1,
                PrimaryFile = new FileResource
                {
                    FileName = "haier-fridge-manual.pdf",
                    ExternalPath = "seed/manuals/haier-fridge-manual.pdf",
                    AvailabilityStatus = FileAvailabilityStatus.Available,
                    ContentType = "application/pdf",
                    SizeBytes = 256000,
                },
                Tags = ["厨房", "冰箱", "保修中"]
            },
            new()
            {
                Id = Guid.Parse("00ab2f67-1f54-4080-b8ae-9c70426f1002"),
                DeviceName = "米家空气净化器",
                Brand = "Xiaomi",
                Model = "AC-M14-SC",
                SpaceId = _spaces[1].Id,
                ManualCategory = ManualCategory.Electronics,
                PurchaseDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-8)),
                WarrantyExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(16)),
                Summary = "滤芯更换周期、联网说明与常见故障处理。",
                AttachmentCount = 1,
                PrimaryFile = new FileResource
                {
                    FileName = "mijia-air-purifier-manual.pdf",
                    ExternalPath = "seed/manuals/mijia-air-purifier-manual.pdf",
                    AvailabilityStatus = FileAvailabilityStatus.Available,
                    ContentType = "application/pdf",
                    SizeBytes = 192000,
                },
                Tags = ["客厅", "净化器", "滤芯提醒"]
            },
        ]);
    }

    public IReadOnlyList<HouseholdMember> GetMembers() => _members;

    public IReadOnlyList<Space> GetSpaces() => _spaces;

    public async Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync()
    {
        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var remote = (await _documentApiClient.GetInsuranceRecordsAsync()).OrderBy(record => record.ExpiryDate).ToList();
                ReplaceLocal(_insuranceRecords, remote);
                return remote;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        if (await UseRemoteStorageAsync() && _documentMetadataRepository is not null)
        {
            try
            {
                var remote = (await _documentMetadataRepository.GetInsuranceRecordsAsync()).OrderBy(record => record.ExpiryDate).ToList();
                ReplaceLocal(_insuranceRecords, remote);
                await SyncReferenceDataAsync();
                return remote;
            }
            catch
            {
                // Fall back to the current in-memory view when the remote service is unavailable.
            }
        }

        return _insuranceRecords.OrderBy(record => record.ExpiryDate).ToList();
    }

    public async Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync()
    {
        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var remote = (await _documentApiClient.GetManualRecordsAsync()).OrderBy(record => record.SpaceId).ToList();
                ReplaceLocal(_manualRecords, remote);
                return remote;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        if (await UseRemoteStorageAsync() && _documentMetadataRepository is not null)
        {
            try
            {
                var remote = (await _documentMetadataRepository.GetManualRecordsAsync()).OrderBy(record => record.SpaceId).ToList();
                ReplaceLocal(_manualRecords, remote);
                await SyncReferenceDataAsync();
                return remote;
            }
            catch
            {
                // Fall back to the current in-memory view when the remote service is unavailable.
            }
        }

        return _manualRecords.OrderBy(record => record.SpaceId).ToList();
    }

    public async Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id)
        => (await GetInsuranceRecordsAsync()).FirstOrDefault(record => record.Id == id);

    public async Task<ManualRecord?> GetManualRecordAsync(Guid id)
        => (await GetManualRecordsAsync()).FirstOrDefault(record => record.Id == id);

    public async Task SaveInsuranceRecordAsync(InsuranceRecord record)
    {
        record.LastUpdatedAt = DateTime.Now;
        record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var saved = await _documentApiClient.SaveInsuranceRecordAsync(record);
                UpsertLocal(_insuranceRecords, saved, item => item.Id == saved.Id);
                return;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        UpsertLocal(_insuranceRecords, record, item => item.Id == record.Id);

        if (await UseRemoteStorageAsync() && _documentMetadataRepository is not null)
        {
            await _documentMetadataRepository.SaveInsuranceRecordAsync(record);
        }
    }

    public async Task SaveManualRecordAsync(ManualRecord record)
    {
        record.LastUpdatedAt = DateTime.Now;
        record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var saved = await _documentApiClient.SaveManualRecordAsync(record);
                UpsertLocal(_manualRecords, saved, item => item.Id == saved.Id);
                return;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        UpsertLocal(_manualRecords, record, item => item.Id == record.Id);

        if (await UseRemoteStorageAsync() && _documentMetadataRepository is not null)
        {
            await _documentMetadataRepository.SaveManualRecordAsync(record);
        }
    }

    public async Task<bool> DeleteInsuranceRecordAsync(Guid id)
    {
        var existing = _insuranceRecords.FirstOrDefault(item => item.Id == id);
        if (existing is null)
        {
            return false;
        }

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var deleted = await _documentApiClient.DeleteInsuranceRecordAsync(id);
                if (deleted)
                {
                    _insuranceRecords.Remove(existing);
                }

                return deleted;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        _insuranceRecords.Remove(existing);
        if (await UseRemoteStorageAsync() && _documentMetadataRepository is not null)
        {
            await _documentMetadataRepository.DeleteInsuranceRecordAsync(id);
        }

        return true;
    }

    public async Task<bool> DeleteManualRecordAsync(Guid id)
    {
        var existing = _manualRecords.FirstOrDefault(item => item.Id == id);
        if (existing is null)
        {
            return false;
        }

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var deleted = await _documentApiClient.DeleteManualRecordAsync(id);
                if (deleted)
                {
                    _manualRecords.Remove(existing);
                }

                return deleted;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        _manualRecords.Remove(existing);
        if (await UseRemoteStorageAsync() && _documentMetadataRepository is not null)
        {
            await _documentMetadataRepository.DeleteManualRecordAsync(id);
        }

        return true;
    }

    public InsuranceRecord CreateInsuranceTemplate() => new()
    {
        InsuredMemberId = _members[0].Id,
        EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
        ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
    };

    public ManualRecord CreateManualTemplate() => new()
    {
        SpaceId = _spaces[0].Id,
        PurchaseDate = DateOnly.FromDateTime(DateTime.Today),
        WarrantyExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddYears(2)),
    };

    public IEnumerable<DocumentSummary> GetRecentDocuments(int maxItems = 4)
    {
        var insurance = _insuranceRecords.Select(record => new DocumentSummary(record.Id, record.PolicyName, "保险", record.Summary, AppRoutes.InsuranceDetail(record.Id), record.Tags, record.LastUpdatedAt));
        var manuals = _manualRecords.Select(record => new DocumentSummary(record.Id, record.DeviceName, "说明书", record.Summary, AppRoutes.ManualDetail(record.Id), record.Tags, record.LastUpdatedAt));
        return insurance.Concat(manuals).OrderByDescending(item => item.UpdatedAt).Take(maxItems).ToList();
    }

    public async Task<FileStorageResult> UploadInsuranceFileAsync(Guid id, Stream content, string fileName, string? contentType)
    {
        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var uploadResult = await _documentApiClient.UploadInsuranceFileAsync(id, content, fileName, contentType);
                var refreshed = await _documentApiClient.GetInsuranceRecordAsync(id);
                if (refreshed is not null)
                {
                    UpsertLocal(_insuranceRecords, refreshed, item => item.Id == refreshed.Id);
                }

                return uploadResult;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        var record = await GetInsuranceRecordAsync(id);
        if (record is null || _documentFileOrchestrator is null || !await UseRemoteStorageAsync())
        {
            return new(false, "当前未启用远端文件服务，无法上传文件。", AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }

        var result = await _documentFileOrchestrator.UploadInsuranceFileAsync(record, content, fileName, contentType);
        await SaveInsuranceRecordAsync(record);
        return result;
    }

    public async Task<FileStorageResult> UploadManualFileAsync(Guid id, Stream content, string fileName, string? contentType)
    {
        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var uploadResult = await _documentApiClient.UploadManualFileAsync(id, content, fileName, contentType);
                var refreshed = await _documentApiClient.GetManualRecordAsync(id);
                if (refreshed is not null)
                {
                    UpsertLocal(_manualRecords, refreshed, item => item.Id == refreshed.Id);
                }

                return uploadResult;
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        var record = await GetManualRecordAsync(id);
        if (record is null || _documentFileOrchestrator is null || !await UseRemoteStorageAsync())
        {
            return new(false, "当前未启用远端文件服务，无法上传文件。", AvailabilityStatus: FileAvailabilityStatus.SyncError);
        }

        var result = await _documentFileOrchestrator.UploadManualFileAsync(record, content, fileName, contentType);
        await SaveManualRecordAsync(record);
        return result;
    }

    public async Task<FileDownloadResult> DownloadInsuranceFileAsync(Guid id)
    {
        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                return await _documentApiClient.DownloadInsuranceFileAsync(id);
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        var record = await GetInsuranceRecordAsync(id);
        if (record?.PrimaryFile is null || _documentFileOrchestrator is null)
        {
            return new(false, "当前记录还没有可下载的附件。");
        }

        return await _documentFileOrchestrator.DownloadAsync(record.PrimaryFile);
    }

    public async Task<FileDownloadResult> DownloadManualFileAsync(Guid id)
    {
        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                return await _documentApiClient.DownloadManualFileAsync(id);
            }
            catch
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
            }
        }

        var record = await GetManualRecordAsync(id);
        if (record?.PrimaryFile is null || _documentFileOrchestrator is null)
        {
            return new(false, "当前记录还没有可下载的附件。");
        }

        return await _documentFileOrchestrator.DownloadAsync(record.PrimaryFile);
    }

    private async Task<bool> UseRemoteStorageAsync()
    {
        if (_storageConnectionProfileService is null)
        {
            return false;
        }

        var profile = await _storageConnectionProfileService.GetActiveProfileAsync();
        var connectionString = await _storageConnectionProfileService.GetMongoConnectionStringAsync();
        return profile.IsActive && profile.ValidationStatus == StorageValidationStatus.Valid && !string.IsNullOrWhiteSpace(connectionString);
    }

    private async Task<bool> UseBackendApiAsync()
    {
        if (_storageConnectionProfileService is null)
        {
            return false;
        }

        var profile = await _storageConnectionProfileService.GetActiveProfileAsync();
        return profile.IsActive
            && profile.ValidationStatus == StorageValidationStatus.Valid
            && Uri.TryCreate(profile.ApiBaseUrl, UriKind.Absolute, out _);
    }

    private async Task SyncReferenceDataAsync()
    {
        if (_businessAggregateRepository is null || !await UseRemoteStorageAsync())
        {
            return;
        }

        var members = await _businessAggregateRepository.GetMembersAsync();
        if (members.Count > 0)
        {
            ReplaceLocal(_members, members.ToList());
        }

        var spaces = await _businessAggregateRepository.GetSpacesAsync();
        if (spaces.Count > 0)
        {
            ReplaceLocal(_spaces, spaces.ToList());
        }
    }

    private static void ReplaceLocal<T>(List<T> target, List<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    private static void UpsertLocal<T>(List<T> target, T value, Func<T, bool> predicate)
    {
        var existing = target.FirstOrDefault(predicate);
        if (existing is null)
        {
            target.Add(value);
            return;
        }

        var index = target.IndexOf(existing);
        target[index] = value;
    }
}

public sealed record DocumentSummary(Guid Id, string Title, string Category, string Summary, string Href, IReadOnlyList<string> Tags, DateTime UpdatedAt);
