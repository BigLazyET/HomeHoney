using HomeHoney.Api.Contracts.Search;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;
using HomeHoney.Services.Navigation;
using HomeHoney.Services.Search;

namespace HomeHoney.Api.Application.Search;

public sealed class SearchAggregationService
{
    private readonly DocumentRepository _documentRepository;
    private readonly CollaborationRepository _collaborationRepository;

    public SearchAggregationService(DocumentRepository documentRepository, CollaborationRepository collaborationRepository)
    {
        _documentRepository = documentRepository;
        _collaborationRepository = collaborationRepository;
    }

    public async Task<IReadOnlyList<SearchGroupDto>> SearchAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        keyword ??= string.Empty;
        var term = keyword.Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        var insurance = (await _documentRepository.GetInsuranceRecordsAsync(cancellationToken))
            .Where(record => ContainsTerm(record.PolicyName, term) || ContainsTerm(record.ProviderName, term) || ContainsTerm(record.Summary, term) || ContainsTag(record.Tags, term))
            .Select(record => new SearchResultItem(FallbackText(record.PolicyName, "未命名保单"), DecorateSummary(record.Summary, record.SyncMessage), "保险", AppRoutes.InsuranceDetail(record.Id)));

        var manuals = (await _documentRepository.GetManualRecordsAsync(cancellationToken))
            .Where(record => ContainsTerm(record.DeviceName, term) || ContainsTerm(record.Summary, term) || ContainsTerm(record.Brand, term) || ContainsTag(record.Tags, term))
            .Select(record => new SearchResultItem(FallbackText(record.DeviceName, "未命名设备"), DecorateSummary(record.Summary, record.SyncMessage), "说明书", AppRoutes.ManualDetail(record.Id)));

        var fridgeNotes = (await _collaborationRepository.GetFridgeNotesAsync(cancellationToken))
            .Where(note => ContainsTerm(note.Title, term) || ContainsTerm(note.Content, term))
            .Select(note => new SearchResultItem(FallbackText(note.Title, "未命名冰箱贴"), FallbackText(note.Content, "暂未填写内容"), "冰箱贴", AppRoutes.FridgeNoteEdit(note.Id)));

        var memos = (await _collaborationRepository.GetMemosAsync(cancellationToken))
            .Where(memo => ContainsTerm(memo.Title, term) || ContainsTerm(memo.Content, term))
            .Select(memo => new SearchResultItem(FallbackText(memo.Title, "未命名备忘录"), FallbackText(memo.Content, "暂未填写内容"), "备忘录", AppRoutes.MemoDetail(memo.Id)));

        var groups = new List<SearchGroupResult>();
        AddGroup(groups, "保险", insurance);
        AddGroup(groups, "说明书", manuals);
        AddGroup(groups, "冰箱贴", fridgeNotes);
        AddGroup(groups, "备忘录", memos);
        return groups.Select(SearchDtoMapper.ToDto).ToList();
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

    private static string DecorateSummary(string summary, string? syncMessage)
    {
        var safeSummary = FallbackText(summary, "暂无摘要");
        return string.IsNullOrWhiteSpace(syncMessage) ? safeSummary : $"{safeSummary} · {syncMessage}";
    }
}
