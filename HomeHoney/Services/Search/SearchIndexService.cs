using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace HomeHoney.Services.Search;

public sealed class SearchIndexService
{
    public string? LastErrorMessage { get; private set; }

    private readonly DocumentCatalogService _documentCatalogService;
    private readonly FamilyCollaborationService _familyCollaborationService;
    private readonly IStorageConnectionProfileService? _storageConnectionProfileService;
    private readonly ISearchApiClient? _searchApiClient;

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

    [ActivatorUtilitiesConstructor]
    public SearchIndexService(
        DocumentCatalogService documentCatalogService,
        FamilyCollaborationService familyCollaborationService,
        IStorageConnectionProfileService storageConnectionProfileService,
        ISearchApiClient searchApiClient)
        : this(documentCatalogService, familyCollaborationService, storageConnectionProfileService)
    {
        _searchApiClient = searchApiClient;
    }

    public async Task<IReadOnlyList<SearchGroupResult>> SearchAsync(string? keyword)
    {
        LastErrorMessage = null;
        keyword ??= string.Empty;
        var term = keyword.Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        var useBackendApi = await UseBackendApiAsync();

        if (useBackendApi && _searchApiClient is not null)
        {
            try
            {
                return await _searchApiClient.SearchAsync(term);
            }
            catch (Exception ex)
            {
                // Fall back to local aggregation when the backend service is unavailable.
                LastErrorMessage = "后端搜索失败，当前展示的是应用内现有内容。";
                Debug.WriteLine($"SearchAsync Error: {ex}");
            }
        }

        var insurance = (await _documentCatalogService.GetInsuranceRecordsAsync())
            .Where(record => ContainsTerm(record.PolicyName, term) || ContainsTerm(record.ProviderName, term) || ContainsTerm(record.Summary, term) || ContainsTag(record.Tags, term))
            .Select(record => new SearchResultItem(FallbackText(record.PolicyName, "未命名保单"), DecorateSummary(record.Summary, record.SyncMessage, useBackendApi), "保险", AppRoutes.InsuranceDetail(record.Id)));

        var manuals = (await _documentCatalogService.GetManualRecordsAsync())
            .Where(record => ContainsTerm(record.DeviceName, term) || ContainsTerm(record.Summary, term) || ContainsTerm(record.Brand, term) || ContainsTag(record.Tags, term))
            .Select(record => new SearchResultItem(FallbackText(record.DeviceName, "未命名设备"), DecorateSummary(record.Summary, record.SyncMessage, useBackendApi), "说明书", AppRoutes.ManualDetail(record.Id)));

        var fridgeNotes = (await _familyCollaborationService.GetFridgeNotesAsync())
            .Where(note => ContainsTerm(note.Title, term) || ContainsTerm(note.Content, term))
            .Select(note => new SearchResultItem(FallbackText(note.Title, "未命名冰箱贴"), FallbackText(note.Content, "暂未填写内容"), "冰箱贴", AppRoutes.FridgeNoteEdit(note.Id)));

        var memos = (await _familyCollaborationService.GetMemosAsync())
            .Where(memo => ContainsTerm(memo.Title, term) || ContainsTerm(memo.Content, term))
            .Select(memo => new SearchResultItem(FallbackText(memo.Title, "未命名备忘录"), FallbackText(memo.Content, "暂未填写内容"), "备忘录", AppRoutes.MemoDetail(memo.Id)));

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

    private static bool ContainsTerm(string? source, string term)
        => !string.IsNullOrWhiteSpace(source) && source.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsTag(IReadOnlyList<string>? tags, string term)
        => tags?.Any(tag => ContainsTerm(tag, term)) == true;

    private static string FallbackText(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string DecorateSummary(string summary, string? syncMessage, bool usingRemoteStorage)
    {
        var safeSummary = FallbackText(summary, "暂无摘要");
        return !usingRemoteStorage || string.IsNullOrWhiteSpace(syncMessage) ? safeSummary : $"{safeSummary} · {syncMessage}";
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

        return profile.IsActive && Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out _);
    }
}

public sealed record SearchResultItem(string Title, string Summary, string Category, string Href);

public sealed record SearchGroupResult(string Title, IReadOnlyList<SearchResultItem> Items);
