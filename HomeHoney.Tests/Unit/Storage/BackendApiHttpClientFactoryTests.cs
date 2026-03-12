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
        await preferences.UpdateStoragePreferenceAsync(new StoragePreference { ApiBaseUrl = "http://localhost:7080" });
        var sut = new BackendApiHttpClientFactory(new HttpClient(), preferences, new BackendApiOptions());

        var client = await sut.CreateAsync("https://api.example.com");

        Assert.Equal("https://api.example.com/", client.BaseAddress?.ToString());
    }

    [Fact]
    public async Task CreateAsync_falls_back_to_default_backend_base_url_when_preference_is_blank()
    {
        var preferences = new UserPreferenceService();
        await preferences.UpdateStoragePreferenceAsync(new StoragePreference { ApiBaseUrl = string.Empty });
        var sut = new BackendApiHttpClientFactory(new HttpClient(), preferences, new BackendApiOptions());

        var client = await sut.CreateAsync();

        Assert.Equal("http://localhost:7080/", client.BaseAddress?.ToString());
    }

    [Fact]
    public async Task CreateAsync_returns_a_new_client_instance_for_each_call()
    {
        var preferences = new UserPreferenceService();
        await preferences.UpdateStoragePreferenceAsync(new StoragePreference { ApiBaseUrl = "http://localhost:7080" });
        var sut = new BackendApiHttpClientFactory(new HttpClient(), preferences, new BackendApiOptions());

        var first = await sut.CreateAsync("http://localhost:7080");
        var second = await sut.CreateAsync("http://localhost:7081");

        Assert.NotSame(first, second);
        Assert.Equal("http://localhost:7080/", first.BaseAddress?.ToString());
        Assert.Equal("http://localhost:7081/", second.BaseAddress?.ToString());
    }
}