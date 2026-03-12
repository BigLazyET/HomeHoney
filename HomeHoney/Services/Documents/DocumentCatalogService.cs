using HomeHoney.Models;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace HomeHoney.Services.Documents;

public sealed class DocumentCatalogService
{
    public string? LastErrorMessage { get; private set; }

    private const string RequiredFieldCode = "required";

    private readonly List<InsuranceRecord> _insuranceRecords;
    private readonly List<ManualRecord> _manualRecords;
    private readonly List<HouseholdMember> _members;
    private readonly List<Space> _spaces;
    private readonly IStorageConnectionProfileService? _storageConnectionProfileService;
    private readonly IDocumentApiClient? _documentApiClient;

    public DocumentCatalogService()
    {
        _insuranceRecords = [];
        _manualRecords = [];
        _members = [];
        _spaces = [];
        SeedLocalData();
    }

    [ActivatorUtilitiesConstructor]
    public DocumentCatalogService(
        IStorageConnectionProfileService storageConnectionProfileService,
        IDocumentApiClient documentApiClient)
    {
        _storageConnectionProfileService = storageConnectionProfileService;
        _documentApiClient = documentApiClient;
        _insuranceRecords = [];
        _manualRecords = [];
        _members = [];
        _spaces = [];
        SeedLocalData();
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

        _manualRecords.Clear();
    }

    public IReadOnlyList<HouseholdMember> GetMembers() => _members;

    public IReadOnlyList<Space> GetSpaces() => _spaces;

    public async Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync()
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var remote = (await _documentApiClient.GetInsuranceRecordsAsync()).OrderBy(record => record.ExpiryDate).ToList();
                ReplaceLocal(_insuranceRecords, remote);
                return remote;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "读取保险资料失败，当前展示的是应用内现有内容。";
                Debug.WriteLine($"GetInsuranceRecordsAsync Error: {ex}");
            }
        }

        return _insuranceRecords.OrderBy(record => record.ExpiryDate).ToList();
    }

    public async Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync()
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                var remote = (await _documentApiClient.GetManualRecordsAsync()).OrderBy(record => record.SpaceId).ToList();
                ReplaceLocal(_manualRecords, remote);
                return remote;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "读取说明书资料失败，当前展示的是应用内现有内容。";
                Debug.WriteLine($"GetManualRecordsAsync Error: {ex}");
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
        LastErrorMessage = null;
        NormalizeInsuranceRecord(record);
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
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "保存保险资料失败，未能同步到后端，请稍后重试。";
                Debug.WriteLine($"SaveInsuranceRecordAsync Error: {ex}");
            }
        }

        UpsertLocal(_insuranceRecords, record, item => item.Id == record.Id);

    }

    public async Task SaveManualRecordAsync(ManualRecord record)
    {
        LastErrorMessage = null;
        NormalizeManualRecord(record);
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
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "保存说明书资料失败，未能同步到后端，请稍后重试。";
                Debug.WriteLine($"SaveManualRecordAsync Error: {ex}");
            }
        }

        UpsertLocal(_manualRecords, record, item => item.Id == record.Id);

    }

    public async Task<bool> DeleteInsuranceRecordAsync(Guid id)
    {
        LastErrorMessage = null;
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
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "删除保险资料失败，后端未确认本次删除操作。";
                Debug.WriteLine($"DeleteInsuranceRecordAsync Error: {ex}");
            }
        }

        _insuranceRecords.Remove(existing);
        return true;
    }

    public async Task<bool> DeleteManualRecordAsync(Guid id)
    {
        LastErrorMessage = null;
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
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "删除说明书资料失败，后端未确认本次删除操作。";
                Debug.WriteLine($"DeleteManualRecordAsync Error: {ex}");
            }
        }

        _manualRecords.Remove(existing);
        return true;
    }

    public InsuranceRecord CreateInsuranceTemplate() => new()
    {
        Id = Guid.NewGuid(),
        InsuredMemberId = Guid.Empty,
        EffectiveDate = default,
        ExpiryDate = default,
        IntegrityStatus = string.Empty,
        Tags = [],
    };

    public ManualRecord CreateManualTemplate() => new()
    {
        Id = Guid.NewGuid(),
        SpaceId = Guid.Empty,
        PurchaseDate = default,
        WarrantyExpiryDate = default,
        IntegrityStatus = string.Empty,
        Tags = [],
    };

    public IReadOnlyList<ValidationIssue> ValidateInsuranceRecord(
        InsuranceRecord record,
        bool hasMemberSelection = true,
        bool hasCategorySelection = true,
        bool hasStatusSelection = true,
        bool hasEffectiveDate = true,
        bool hasExpiryDate = true)
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(record.PolicyName))
        {
            issues.Add(new("PolicyName", "请填写保单名称。", Code: RequiredFieldCode));
        }

        if (!hasMemberSelection || record.InsuredMemberId == Guid.Empty)
        {
            issues.Add(new("InsuredMemberId", "请选择保障对象。", Code: RequiredFieldCode));
        }

        if (!hasCategorySelection)
        {
            issues.Add(new("InsuranceCategory", "请选择保障类别。", Code: RequiredFieldCode));
        }

        if (!hasStatusSelection)
        {
            issues.Add(new("Status", "请选择当前状态。", Code: RequiredFieldCode));
        }

        if (string.IsNullOrWhiteSpace(record.ProviderName))
        {
            issues.Add(new("ProviderName", "请填写保险公司名称。", Code: RequiredFieldCode));
        }

        if (!hasEffectiveDate || record.EffectiveDate == default)
        {
            issues.Add(new("EffectiveDate", "请选择生效日期。", Code: RequiredFieldCode));
        }

        if (!hasExpiryDate || record.ExpiryDate == default)
        {
            issues.Add(new("ExpiryDate", "请选择到期日期。", Code: RequiredFieldCode));
        }
        else if (hasEffectiveDate && record.EffectiveDate != default && record.ExpiryDate < record.EffectiveDate)
        {
            issues.Add(new("ExpiryDate", "到期日期不能早于生效日期。", Code: "range"));
        }

        return issues;
    }

    public IReadOnlyList<ValidationIssue> ValidateManualRecord(
        ManualRecord record,
        bool hasSpaceSelection = true,
        bool hasCategorySelection = true,
        bool hasPurchaseDate = true,
        bool hasWarrantyDate = true)
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(record.DeviceName))
        {
            issues.Add(new("DeviceName", "请填写设备名称。", Code: RequiredFieldCode));
        }

        if (string.IsNullOrWhiteSpace(record.Brand))
        {
            issues.Add(new("Brand", "请填写品牌名称。", Code: RequiredFieldCode));
        }

        if (!hasSpaceSelection || record.SpaceId == Guid.Empty)
        {
            issues.Add(new("SpaceId", "请选择所属空间。", Code: RequiredFieldCode));
        }

        if (!hasCategorySelection)
        {
            issues.Add(new("ManualCategory", "请选择说明书类别。", Code: RequiredFieldCode));
        }

        if (!hasPurchaseDate || record.PurchaseDate == default)
        {
            issues.Add(new("PurchaseDate", "请选择购买日期。", Code: RequiredFieldCode));
        }

        if (!hasWarrantyDate || record.WarrantyExpiryDate == default)
        {
            issues.Add(new("WarrantyExpiryDate", "请选择保修截止日期。", Code: RequiredFieldCode));
        }
        else if (hasPurchaseDate && record.PurchaseDate != default && record.WarrantyExpiryDate < record.PurchaseDate)
        {
            issues.Add(new("WarrantyExpiryDate", "保修截止日期不能早于购买日期。", Code: "range"));
        }

        return issues;
    }

    public string GetMemberDisplayName(Guid memberId)
        => _members.FirstOrDefault(member => member.Id == memberId)?.DisplayName ?? "未选择";

    public string GetSpaceDisplayName(Guid spaceId)
        => _spaces.FirstOrDefault(space => space.Id == spaceId)?.Name ?? "未选择";

    public static string NormalizeUserInput(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    public IEnumerable<DocumentSummary> GetRecentDocuments(int maxItems = 4)
    {
        var insurance = _insuranceRecords.Select(record => new DocumentSummary(record.Id, PresentationFallbacks.TextOrFallback(record.PolicyName, "未命名保单"), "保险", PresentationFallbacks.TextOrFallback(record.Summary, "暂未填写摘要"), AppRoutes.InsuranceDetail(record.Id), record.Tags, record.LastUpdatedAt));
        var manuals = _manualRecords.Select(record => new DocumentSummary(record.Id, PresentationFallbacks.TextOrFallback(record.DeviceName, "未命名设备"), "说明书", PresentationFallbacks.TextOrFallback(record.Summary, "暂未填写摘要"), AppRoutes.ManualDetail(record.Id), record.Tags, record.LastUpdatedAt));
        return insurance.Concat(manuals).OrderByDescending(item => item.UpdatedAt).Take(maxItems).ToList();
    }

    public async Task<FileStorageResult> UploadInsuranceFileAsync(Guid id, Stream content, string fileName, string? contentType)
    {
        LastErrorMessage = null;

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
                else
                {
                    ApplyUploadMetadata(_insuranceRecords, id, uploadResult);
                }

                return uploadResult;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "上传保险附件失败，未能同步到后端，请稍后重试。";
                Debug.WriteLine($"UploadInsuranceFileAsync Error: {ex}");
            }
        }

        return new(false, "当前未启用后端 API 文件上传能力。", AvailabilityStatus: FileAvailabilityStatus.SyncError);
    }

    public async Task<FileStorageResult> UploadManualFileAsync(Guid id, Stream content, string fileName, string? contentType)
    {
        LastErrorMessage = null;

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
                else
                {
                    ApplyUploadMetadata(_manualRecords, id, uploadResult);
                }

                return uploadResult;
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "上传说明书附件失败，未能同步到后端，请稍后重试。";
                Debug.WriteLine($"UploadManualFileAsync Error: {ex}");
            }
        }

        return new(false, "当前未启用后端 API 文件上传能力。", AvailabilityStatus: FileAvailabilityStatus.SyncError);
    }

    public async Task<FileDownloadResult> DownloadInsuranceFileAsync(Guid id)
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                return await _documentApiClient.DownloadInsuranceFileAsync(id);
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "下载保险附件失败，请检查后端连接后重试。";
                Debug.WriteLine($"DownloadInsuranceFileAsync Error: {ex}");
            }
        }

        return new(false, "当前未启用后端 API 文件下载能力。");
    }

    public async Task<FileDownloadResult> DownloadManualFileAsync(Guid id)
    {
        LastErrorMessage = null;

        if (await UseBackendApiAsync() && _documentApiClient is not null)
        {
            try
            {
                return await _documentApiClient.DownloadManualFileAsync(id);
            }
            catch (Exception ex)
            {
                // Fall back to the current in-memory view when the backend service is unavailable.
                LastErrorMessage = "下载说明书附件失败，请检查后端连接后重试。";
                Debug.WriteLine($"DownloadManualFileAsync Error: {ex}");
            }
        }

        return new(false, "当前未启用后端 API 文件下载能力。");
    }

    private async Task<bool> UseBackendApiAsync()
    {
        if (_storageConnectionProfileService is null)
        {
            return false;
        }

        var profile = await _storageConnectionProfileService.GetActiveProfileAsync();
        var apiBaseUrl = string.IsNullOrWhiteSpace(profile.ApiBaseUrl)
            ? StorageConnectionProfile.DefaultBackendApiBaseUrl
            : profile.ApiBaseUrl;

        return profile.IsActive
            && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out _);
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

    private static void ApplyUploadMetadata(List<InsuranceRecord> records, Guid id, FileStorageResult result)
    {
        var record = records.FirstOrDefault(item => item.Id == id);
        if (record is null)
        {
            return;
        }

        record.AttachmentCount = result.IsSuccess ? 1 : record.AttachmentCount;
        record.SyncMessage = result.Message;
        record.PrimaryFile ??= new FileResource();
        record.PrimaryFile.FileName = result.FileName ?? record.PrimaryFile.FileName;
        record.PrimaryFile.ExternalPath = result.RemotePath ?? record.PrimaryFile.ExternalPath;
        record.PrimaryFile.ExternalFileId = result.RemoteId ?? record.PrimaryFile.ExternalFileId;
        record.PrimaryFile.ContentType = result.ContentType ?? record.PrimaryFile.ContentType;
        record.PrimaryFile.SizeBytes = result.SizeBytes ?? record.PrimaryFile.SizeBytes;
        record.PrimaryFile.LastSyncedAt = result.LastSyncedAt ?? record.PrimaryFile.LastSyncedAt ?? DateTime.UtcNow;
        record.PrimaryFile.AvailabilityStatus = result.AvailabilityStatus;
    }

    private static void ApplyUploadMetadata(List<ManualRecord> records, Guid id, FileStorageResult result)
    {
        var record = records.FirstOrDefault(item => item.Id == id);
        if (record is null)
        {
            return;
        }

        record.AttachmentCount = result.IsSuccess ? 1 : record.AttachmentCount;
        record.SyncMessage = result.Message;
        record.PrimaryFile ??= new FileResource();
        record.PrimaryFile.FileName = result.FileName ?? record.PrimaryFile.FileName;
        record.PrimaryFile.ExternalPath = result.RemotePath ?? record.PrimaryFile.ExternalPath;
        record.PrimaryFile.ExternalFileId = result.RemoteId ?? record.PrimaryFile.ExternalFileId;
        record.PrimaryFile.ContentType = result.ContentType ?? record.PrimaryFile.ContentType;
        record.PrimaryFile.SizeBytes = result.SizeBytes ?? record.PrimaryFile.SizeBytes;
        record.PrimaryFile.LastSyncedAt = result.LastSyncedAt ?? record.PrimaryFile.LastSyncedAt ?? DateTime.UtcNow;
        record.PrimaryFile.AvailabilityStatus = result.AvailabilityStatus;
    }

    private static void NormalizeInsuranceRecord(InsuranceRecord record)
    {
        record.PolicyName = NormalizeUserInput(record.PolicyName);
        record.ProviderName = NormalizeUserInput(record.ProviderName);
        record.ContactName = NormalizeUserInput(record.ContactName);
        record.ContactPhone = NormalizeUserInput(record.ContactPhone);
        record.Summary = NormalizeUserInput(record.Summary);
        record.Tags = [.. record.Tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim())];
    }

    private static void NormalizeManualRecord(ManualRecord record)
    {
        record.DeviceName = NormalizeUserInput(record.DeviceName);
        record.Brand = NormalizeUserInput(record.Brand);
        record.Model = NormalizeUserInput(record.Model);
        record.Summary = NormalizeUserInput(record.Summary);
        record.Tags = [.. record.Tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim())];
    }
}

public sealed record DocumentSummary(Guid Id, string Title, string Category, string Summary, string Href, IReadOnlyList<string> Tags, DateTime UpdatedAt);
