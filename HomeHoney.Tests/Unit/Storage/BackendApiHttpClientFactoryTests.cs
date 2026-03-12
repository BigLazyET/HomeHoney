using HomeHoney.Models;
using HomeHoney.Services.Preferences;
using HomeHoney.Services.Storage;

namespace HomeHoney.Tests.Unit.Storage;

public sealed class BackendApiHttpClientFactoryTests
{
    [Fact]
    public async Task CreateAsync_uses_explicit_backend_base_url_when_provided()
    {
        var preferences = new UserPreferenceService();
        await preferences.UpdateStoragePreferenceAsync(new StoragePreference { ApiBaseUrl = "https://localhost:7080" });
        var sut = new BackendApiHttpClientFactory(new HttpClient(), preferences, new BackendApiOptions());

        var client = await sut.CreateAsync("https://api.example.com");

        Assert.Equal("https://api.example.com/", client.BaseAddress?.ToString());
    }
}