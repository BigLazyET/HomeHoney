using System.Net.Http.Json;
using HomeHoney.Services.Storage;

namespace HomeHoney.Services.Search;

public sealed class SearchApiClient : ISearchApiClient
{
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public SearchApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<SearchGroupResult>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync($"api/v1/search?q={Uri.EscapeDataString(keyword)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SearchGroupResult>>(cancellationToken: cancellationToken) ?? [];
    }
}