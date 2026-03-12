using HomeHoney.Models;
using HomeHoney.Services.Preferences;
using HomeHoney.Services.Storage;
using Moq;
using System.Net;

namespace HomeHoney.Tests.Unit.Storage;

public sealed class StorageConnectionProfileServiceTests
{
    [Fact]
    public async Task SaveProfileAsync_persists_backend_profile_after_remote_success()
    {
        var preferences = new UserPreferenceService();
        var httpClient = new HttpClient(new SuccessHandler());
        var validator = new StorageConfigurationValidator(httpClient);

        var adminClient = new Mock<IAdminStorageApiClient>();
        adminClient.Setup(x => x.SaveProfileAsync(It.IsAny<StorageConnectionProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorageConnectionProfile profile, CancellationToken _) => new StorageProfileSaveResult(true, profile, "ok"));

        var sut = new StorageConnectionProfileService(preferences, validator, adminClient.Object);
        var profile = new StorageConnectionProfile { DisplayName = "家庭后端", ApiBaseUrl = "http://localhost:7080" };

        var result = await sut.SaveProfileAsync(profile);

        Assert.True(result.IsSuccess);
        Assert.Equal("家庭后端", preferences.GetPreferences().StoragePreference.DisplayName);
        Assert.Equal("http://localhost:7080", preferences.GetPreferences().StoragePreference.ApiBaseUrl);
    }

    [Fact]
    public async Task SaveProfileAsync_uses_hidden_default_backend_address_when_value_is_blank()
    {
        var preferences = new UserPreferenceService();
        var httpClient = new HttpClient(new SuccessHandler());
        var validator = new StorageConfigurationValidator(httpClient);

        var adminClient = new Mock<IAdminStorageApiClient>();
        adminClient.Setup(x => x.SaveProfileAsync(It.IsAny<StorageConnectionProfile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorageConnectionProfile profile, CancellationToken _) => new StorageProfileSaveResult(true, profile, "ok"));

        var sut = new StorageConnectionProfileService(preferences, validator, adminClient.Object);

        var result = await sut.SaveProfileAsync(new StorageConnectionProfile { ApiBaseUrl = string.Empty });

        Assert.True(result.IsSuccess);
        Assert.Equal(StorageConnectionProfile.DefaultBackendApiBaseUrl, result.Profile.ApiBaseUrl);
        Assert.Equal(StorageConnectionProfile.DefaultBackendApiBaseUrl, preferences.GetPreferences().StoragePreference.ApiBaseUrl);
    }

    private sealed class SuccessHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}