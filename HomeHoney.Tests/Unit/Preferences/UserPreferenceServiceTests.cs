using HomeHoney.Models;
using HomeHoney.Services.Preferences;
using Moq;

namespace HomeHoney.Tests.Unit.Preferences;

public sealed class UserPreferenceServiceTests
{
    [Fact]
    public void Constructor_loads_legacy_string_enum_payload_and_normalizes_home_modules()
    {
        PlatformPreferenceStore.Remove("homehoney.user-preferences");
        PlatformPreferenceStore.SetString("homehoney.user-preferences", """
        {
          "themeMode": "Dark",
          "notificationSettings": {
            "insuranceEnabled": true,
            "manualEnabled": true,
            "memoEnabled": true,
            "fridgeNoteEnabled": true
          },
          "homeModuleOrder": ["RecentDocuments", "QuickActions", "UpcomingReminders"],
          "hiddenHomeModules": ["FamilyMessages"],
          "privacyMode": "Standard",
          "onboardingCompleted": true,
          "storagePreference": {
            "displayName": "默认后端",
            "apiBaseUrl": "http://localhost:7080",
            "fileServiceBaseUrl": "http://localhost:8999",
            "fileServiceApiPath": "/api",
            "mongoConnectionStringSecretKey": "storage.mongo.connection",
            "mongoConnectionStringPreview": "",
            "mongoDatabaseName": "homehoney",
            "isActive": true,
            "validationStatus": "Valid",
            "validationMessage": "ok"
          }
        }
        """);

        var sut = new UserPreferenceService();
        var preferences = sut.GetPreferences();

        Assert.Equal(ThemeMode.Dark, preferences.ThemeMode);
        Assert.Equal([HomeModuleType.QuickActions, HomeModuleType.UpcomingReminders], preferences.HomeModuleOrder);
        Assert.Empty(preferences.HiddenHomeModules);
        Assert.Equal(StorageValidationStatus.Valid, preferences.StoragePreference.ValidationStatus);

        PlatformPreferenceStore.Remove("homehoney.user-preferences");
    }

    [Fact]
    public async Task MoveModuleDownAsync_keeps_local_change_when_remote_sync_throws()
    {
        PlatformPreferenceStore.Remove("homehoney.user-preferences");

        var preferenceApiClient = new Mock<IPreferenceApiClient>();
        preferenceApiClient.Setup(x => x.UpdateAsync(It.IsAny<UserPreference>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("sync failed"));

        var serviceProvider = new TestServiceProvider(preferenceApiClient.Object);
        var sut = new UserPreferenceService(serviceProvider);

        await sut.MoveModuleDownAsync(HomeModuleType.QuickActions);

        Assert.Equal([HomeModuleType.UpcomingReminders, HomeModuleType.QuickActions], sut.GetPreferences().HomeModuleOrder);
        Assert.Equal("设置已保存到本地，但同步后端失败，请稍后重试。", sut.LastRemoteSyncErrorMessage);

        PlatformPreferenceStore.Remove("homehoney.user-preferences");
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly IPreferenceApiClient _preferenceApiClient;

        public TestServiceProvider(IPreferenceApiClient preferenceApiClient)
        {
            _preferenceApiClient = preferenceApiClient;
        }

        public object? GetService(Type serviceType)
            => serviceType == typeof(IPreferenceApiClient) ? _preferenceApiClient : null;
    }
}