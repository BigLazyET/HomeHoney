namespace HomeHoney.Api.Contracts.Search;

public sealed class SearchResultItemDto
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Href { get; set; } = "/";
}

public sealed class SearchGroupDto
{
    public string Title { get; set; } = string.Empty;

    public IReadOnlyList<SearchResultItemDto> Items { get; set; } = [];
}
