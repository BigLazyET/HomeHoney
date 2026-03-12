namespace HomeHoney.Services.Search;

public interface ISearchApiClient
{
	Task<IReadOnlyList<SearchGroupResult>> SearchAsync(string keyword, CancellationToken cancellationToken = default);
}
