using HomeHoney.Models;
using HomeHoney.Services.Diagnostics;
using HomeHoney.Services.Storage;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeHoney.Services.Preferences;

public sealed class UserPreferenceService
{
    private const string PreferenceStorageKey = "homehoney.user-preferences";
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private static readonly IReadOnlyList<HomeModuleType> SupportedHomeModules =
    [
        HomeModuleType.QuickActions,
        HomeModuleType.UpcomingReminders,
    ];

    private readonly UserPreference _preferences = new();
    private readonly IServiceProvider? _serviceProvider;

    public string? LastRemoteSyncErrorMessage { get; private set; }

    public event Action? Changed;

    public UserPreferenceService(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        Load();
    }

    public UserPreference GetPreferences() => _preferences;

    public bool IsOnboardingCompleted => _preferences.OnboardingCompleted;

    public bool ShouldShowOnboardingOnStartup() => !_preferences.OnboardingCompleted;

    public async Task CompleteOnboardingAsync()
    {
        _preferences.OnboardingCompleted = true;
        await NotifyStateChangedAsync();
    }

    public async Task SetThemeModeAsync(ThemeMode themeMode)
    {
        _preferences.ThemeMode = themeMode;
        await NotifyStateChangedAsync();
    }

    public async Task SetPrivacyModeAsync(PrivacyMode privacyMode)
    {
        _preferences.PrivacyMode = privacyMode;
        await NotifyStateChangedAsync();
    }

    public async Task UpdateNotificationSettingsAsync(NotificationPreference notificationPreference)
    {
        _preferences.NotificationSettings = notificationPreference ?? new NotificationPreference();
        await NotifyStateChangedAsync();
    }

    public async Task UpdateStoragePreferenceAsync(StoragePreference storagePreference)
    {
        _preferences.StoragePreference = storagePreference ?? new StoragePreference();
        await NotifyStateChangedAsync();
    }

    public async Task SetModuleVisibilityAsync(HomeModuleType moduleType, bool isVisible)
    {
        var changed = false;
        if (isVisible)
        {
            changed = _preferences.HiddenHomeModules.Remove(moduleType);
        }
        else
        {
            changed = _preferences.HiddenHomeModules.Add(moduleType);
        }

        if (changed)
        {
            await NotifyStateChangedAsync();
        }
    }

    public async Task MoveModuleUpAsync(HomeModuleType moduleType)
    {
        if (MoveModule(moduleType, -1))
        {
            await NotifyStateChangedAsync();
        }
    }

    public async Task MoveModuleDownAsync(HomeModuleType moduleType)
    {
        if (MoveModule(moduleType, 1))
        {
            await NotifyStateChangedAsync();
        }
    }

    private bool MoveModule(HomeModuleType moduleType, int delta)
    {
        var order = _preferences.HomeModuleOrder;
        var index = order.IndexOf(moduleType);
        if (index < 0)
        {
            return false;
        }

        var nextIndex = index + delta;
        if (nextIndex < 0 || nextIndex >= order.Count)
        {
            return false;
        }

        (order[index], order[nextIndex]) = (order[nextIndex], order[index]);
        return true;
    }

    private async Task NotifyStateChangedAsync()
    {
        Save();
        await SaveRemoteAsync();
        Changed?.Invoke();
    }

    private void Load()
    {
        try
        {
            var raw = PlatformPreferenceStore.GetString(PreferenceStorageKey);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var saved = JsonSerializer.Deserialize<UserPreference>(raw, JsonOptions);
                if (saved is not null)
                {
                    Apply(saved);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Load Error: {ex}");
            RuntimeDiagnostics.Record(nameof(UserPreferenceService), ex);
        }
    }

    private void Save()
    {
        try
        {
            var raw = JsonSerializer.Serialize(_preferences, JsonOptions);
            PlatformPreferenceStore.SetString(PreferenceStorageKey, raw);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Save Error: {ex}");
            RuntimeDiagnostics.Record(nameof(UserPreferenceService), ex);
        }
    }

    private async Task SaveRemoteAsync()
    {
        try
        {
            LastRemoteSyncErrorMessage = null;
            var preferenceApiClient = _serviceProvider?.GetService(typeof(IPreferenceApiClient)) as IPreferenceApiClient;
            if (preferenceApiClient is not null)
            {
                await preferenceApiClient.UpdateAsync(_preferences);
            }
        }
        catch (Exception ex)
        {
            LastRemoteSyncErrorMessage = "设置已保存到本地，但同步后端失败，请稍后重试。";
            Debug.WriteLine($"SaveRemote Error: {ex}");
            RuntimeDiagnostics.Record(nameof(UserPreferenceService), ex);
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private void Apply(UserPreference saved)
    {
        _preferences.ThemeMode = saved.ThemeMode;
        _preferences.NotificationSettings = saved.NotificationSettings ?? new NotificationPreference();
        _preferences.HomeModuleOrder = NormalizeModuleOrder(saved.HomeModuleOrder);
        _preferences.HiddenHomeModules = [.. saved.HiddenHomeModules.Where(SupportedHomeModules.Contains)];
        _preferences.PrivacyMode = saved.PrivacyMode;
        _preferences.OnboardingCompleted = saved.OnboardingCompleted;
        _preferences.StoragePreference = saved.StoragePreference ?? new StoragePreference();
    }

    private static List<HomeModuleType> NormalizeModuleOrder(IReadOnlyList<HomeModuleType>? order)
    {
        var normalized = (order ?? [])
            .Where(SupportedHomeModules.Contains)
            .Distinct()
            .ToList();

        foreach (var module in SupportedHomeModules)
        {
            if (!normalized.Contains(module))
            {
                normalized.Add(module);
            }
        }

        return normalized;
    }
}
