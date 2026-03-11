using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Storage;

namespace HomeHoney.Services.Search;

public sealed class SearchIndexService
{
    private readonly DocumentCatalogService _documentCatalogService;
    private readonly FamilyCollaborationService _familyCollaborationService;
    private readonly IStorageConnectionProfileService? _storageConnectionProfileService;

    public SearchIndexService(DocumentCatalogService documentCatalogService, FamilyCollaborationService familyCollaborationService)
    {
        _documentCatalogService = documentCatalogService;
        _familyCollaborationService = familyCollaborationService;
    }

    public SearchIndexService(DocumentCatalogService documentCatalogService, FamilyCollaborationService familyCollaborationService, IStorageConnectionProfileService storageConnectionProfileService)
    {
        _documentCatalogService = documentCatalogService;
        _familyCollaborationService = familyCollaborationService;
        _storageConnectionProfileService = storageConnectionProfileService;
    }

    public async Task<IReadOnlyList<SearchGroupResult>> SearchAsync(string? keyword)
    {
        keyword ??= string.Empty;
        var term = keyword.Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        var usingRemoteStorage = await UsesRemoteStorageAsync();

        var insurance = (await _documentCatalogService.GetInsuranceRecordsAsync())
            .Where(record => Contains(record.PolicyName, term) || Contains(record.Summary, term) || record.Tags.Any(tag => Contains(tag, term)))
            .Select(record => new SearchResultItem(record.PolicyName, DecorateSummary(record.Summary, record.SyncMessage, usingRemoteStorage), "保险", AppRoutes.InsuranceDetail(record.Id)));

        var manuals = (await _documentCatalogService.GetManualRecordsAsync())
            .Where(record => Contains(record.DeviceName, term) || Contains(record.Summary, term) || Contains(record.Brand, term) || record.Tags.Any(tag => Contains(tag, term)))
            .Select(record => new SearchResultItem(record.DeviceName, DecorateSummary(record.Summary, record.SyncMessage, usingRemoteStorage), "说明书", AppRoutes.ManualDetail(record.Id)));

        var fridgeNotes = (await _familyCollaborationService.GetFridgeNotesAsync())
            .Where(note => Contains(note.Title, term) || Contains(note.Content, term))
            .Select(note => new SearchResultItem(note.Title, note.Content, "冰箱贴", AppRoutes.FridgeNoteEdit(note.Id)));

        var memos = (await _familyCollaborationService.GetMemosAsync())
            .Where(memo => Contains(memo.Title, term) || Contains(memo.Content, term))
            .Select(memo => new SearchResultItem(memo.Title, memo.Content, "备忘录", AppRoutes.MemoDetail(memo.Id)));

        var groups = new List<SearchGroupResult>();
        AddGroup(groups, "保险", insurance);
        AddGroup(groups, "说明书", manuals);
        AddGroup(groups, "冰箱贴", fridgeNotes);
        AddGroup(groups, "备忘录", memos);
        return groups;
    }

    private static void AddGroup(List<SearchGroupResult> groups, string title, IEnumerable<SearchResultItem> items)
    {
        var materialized = items.ToList();
        if (materialized.Count > 0)
        {
            groups.Add(new SearchGroupResult(title, materialized));
        }
    }

    private static bool Contains(string source, string term) => source.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string DecorateSummary(string summary, string? syncMessage, bool usingRemoteStorage)
        => !usingRemoteStorage || string.IsNullOrWhiteSpace(syncMessage) ? summary : $"{summary} · {syncMessage}";

    private async Task<bool> UsesRemoteStorageAsync()
    {
        if (_storageConnectionProfileService is null)
        {
            return false;
        }

        var profile = await _storageConnectionProfileService.GetActiveProfileAsync();
        return profile.ValidationStatus == StorageValidationStatus.Valid && profile.IsActive;
    }
}

public sealed record SearchResultItem(string Title, string Summary, string Category, string Href);

public sealed record SearchGroupResult(string Title, IReadOnlyList<SearchResultItem> Items);
