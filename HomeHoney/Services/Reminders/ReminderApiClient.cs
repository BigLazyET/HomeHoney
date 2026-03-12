using System.Net.Http.Json;
using HomeHoney.Models;
using HomeHoney.Services.Storage;

namespace HomeHoney.Services.Reminders;

public sealed class ReminderApiClient : IReminderApiClient
{
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public ReminderApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ReminderItem>> GetUpcomingAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync($"api/v1/reminders?days={days}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ReminderItem>>(cancellationToken: cancellationToken) ?? [];
    }
}