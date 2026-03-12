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
        var secretStore = new Mock<ISecretStore>();
        var httpClient = new HttpClient(new SuccessHandler());
        var validator = new StorageConfigurationValidator(httpClient);

        var adminClient = new Mock<IAdminStorageApiClient>();
        adminClient.Setup(x => x.SaveProfileAsync(It.IsAny<StorageConnectionProfile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorageConnectionProfile profile, string _, CancellationToken _) => new StorageProfileSaveResult(true, profile, "ok"));

        var sut = new StorageConnectionProfileService(preferences, secretStore.Object, validator, adminClient.Object);
        var profile = new StorageConnectionProfile { DisplayName = "家庭后端", ApiBaseUrl = "https://localhost:7080" };

        var result = await sut.SaveProfileAsync(profile, "mongodb://localhost:27017/homehoney");

        Assert.True(result.IsSuccess);
        Assert.Equal("家庭后端", preferences.GetPreferences().StoragePreference.DisplayName);
        Assert.Equal("https://localhost:7080", preferences.GetPreferences().StoragePreference.ApiBaseUrl);
        secretStore.Verify(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class SuccessHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}