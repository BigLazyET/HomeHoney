using HomeHoney.Models;
using HomeHoney.Services.Navigation;

namespace HomeHoney.Services.Documents;

public sealed class DocumentCatalogService
{
    private readonly List<InsuranceRecord> _insuranceRecords;
    private readonly List<ManualRecord> _manualRecords;
    private readonly List<HouseholdMember> _members;
    private readonly List<Space> _spaces;

    public DocumentCatalogService()
    {
        _members =
        [
            new() { Id = Guid.Parse("f4aab8d3-c8db-4f45-8f45-6e0eb50a1001"), DisplayName = "ET", Role = HouseholdRole.Self, AvatarColor = "#007aff", IsPrimary = true },
            new() { Id = Guid.Parse("f4aab8d3-c8db-4f45-8f45-6e0eb50a1002"), DisplayName = "妈妈", Role = HouseholdRole.Parent, AvatarColor = "#ff9500" },
        ];

        _spaces =
        [
            new() { Id = Guid.Parse("6dbc1d83-67ab-4456-b62b-15f4c88c1001"), Name = "厨房", Icon = "🍳", SortOrder = 1 },
            new() { Id = Guid.Parse("6dbc1d83-67ab-4456-b62b-15f4c88c1002"), Name = "客厅", Icon = "🛋️", SortOrder = 2 },
        ];

        _insuranceRecords =
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
                AttachmentCount = 3,
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
                AttachmentCount = 2,
                Tags = ["儿童", "意外", "即将到期"]
            },
        ];

        _manualRecords =
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
                AttachmentCount = 2,
                Tags = ["客厅", "净化器", "滤芯提醒"]
            },
        ];
    }

    public IReadOnlyList<HouseholdMember> GetMembers() => _members;

    public IReadOnlyList<Space> GetSpaces() => _spaces;

    public Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync() => Task.FromResult<IReadOnlyList<InsuranceRecord>>(_insuranceRecords.OrderBy(record => record.ExpiryDate).ToList());

    public Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync() => Task.FromResult<IReadOnlyList<ManualRecord>>(_manualRecords.OrderBy(record => record.SpaceId).ToList());

    public Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id) => Task.FromResult(_insuranceRecords.FirstOrDefault(record => record.Id == id));

    public Task<ManualRecord?> GetManualRecordAsync(Guid id) => Task.FromResult(_manualRecords.FirstOrDefault(record => record.Id == id));

    public Task SaveInsuranceRecordAsync(InsuranceRecord record)
    {
        record.LastUpdatedAt = DateTime.Now;
        var existing = _insuranceRecords.FirstOrDefault(item => item.Id == record.Id);
        if (existing is null)
        {
            record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
            _insuranceRecords.Add(record);
        }
        else
        {
            var index = _insuranceRecords.IndexOf(existing);
            _insuranceRecords[index] = record;
        }

        return Task.CompletedTask;
    }

    public Task SaveManualRecordAsync(ManualRecord record)
    {
        record.LastUpdatedAt = DateTime.Now;
        var existing = _manualRecords.FirstOrDefault(item => item.Id == record.Id);
        if (existing is null)
        {
            record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
            _manualRecords.Add(record);
        }
        else
        {
            var index = _manualRecords.IndexOf(existing);
            _manualRecords[index] = record;
        }

        return Task.CompletedTask;
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
}

public sealed record DocumentSummary(Guid Id, string Title, string Category, string Summary, string Href, IReadOnlyList<string> Tags, DateTime UpdatedAt);
