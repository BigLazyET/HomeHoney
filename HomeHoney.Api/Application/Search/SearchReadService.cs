using HomeHoney.Api.Application.Collaboration;
using HomeHoney.Api.Application.Documents;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Search;

namespace HomeHoney.Api.Application.Search;

public sealed class SearchReadService
{
    private readonly DocumentReadService _documentReadService;
    private readonly CollaborationReadService _collaborationReadService;

    public SearchReadService(DocumentReadService documentReadService, CollaborationReadService collaborationReadService)
    {
        _documentReadService = documentReadService;
        _collaborationReadService = collaborationReadService;
    }

    public async Task<IReadOnlyList<SearchGroupResult>> SearchAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        keyword ??= string.Empty;
        var term = keyword.Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        var insurance = (await _documentReadService.GetInsuranceRecordsAsync(cancellationToken))
            .Where(record => Contains(record.PolicyName, term) || Contains(record.Summary, term) || record.Tags.Any(tag => Contains(tag, term)))
            .Select(record => new SearchResultItem(record.PolicyName, DecorateSummary(record.Summary, record.SyncMessage), "保险", AppRoutes.InsuranceDetail(record.Id)));

        var manuals = (await _documentReadService.GetManualRecordsAsync(cancellationToken))
            .Where(record => Contains(record.DeviceName, term) || Contains(record.Summary, term) || Contains(record.Brand, term) || record.Tags.Any(tag => Contains(tag, term)))
            .Select(record => new SearchResultItem(record.DeviceName, DecorateSummary(record.Summary, record.SyncMessage), "说明书", AppRoutes.ManualDetail(record.Id)));

        var fridgeNotes = (await _collaborationReadService.GetFridgeNotesAsync(cancellationToken))
            .Where(note => Contains(note.Title, term) || Contains(note.Content, term))
            .Select(note => new SearchResultItem(note.Title, note.Content, "冰箱贴", AppRoutes.FridgeNoteEdit(note.Id)));

        var memos = (await _collaborationReadService.GetMemosAsync(cancellationToken))
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

    private static string DecorateSummary(string summary, string? syncMessage)
        => string.IsNullOrWhiteSpace(syncMessage) ? summary : $"{summary} · {syncMessage}";
}