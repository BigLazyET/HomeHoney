using HomeHoney.Models;
using HomeHoney.Services.Diagnostics;
using HomeHoney.Services.Storage;
using System.Text.Json;

namespace HomeHoney.Services.Preferences;

public sealed class UserPreferenceService
{
    private const string PreferenceStorageKey = "homehoney.user-preferences";
    private readonly UserPreference _preferences = new();
    private readonly IServiceProvider? _serviceProvider;

    public event Action? Changed;

    public UserPreferenceService(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        Load();
    }

    public UserPreference GetPreferences() => _preferences;

    public bool IsOnboardingCompleted => _preferences.OnboardingCompleted;

    public bool ShouldShowOnboardingOnStartup() => !_preferences.OnboardingCompleted;

    public Task CompleteOnboardingAsync()
    {
        _preferences.OnboardingCompleted = true;
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task SetThemeModeAsync(ThemeMode themeMode)
    {
        _preferences.ThemeMode = themeMode;
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task SetPrivacyModeAsync(PrivacyMode privacyMode)
    {
        _preferences.PrivacyMode = privacyMode;
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task UpdateNotificationSettingsAsync(NotificationPreference notificationPreference)
    {
        _preferences.NotificationSettings = notificationPreference ?? new NotificationPreference();
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task UpdateStoragePreferenceAsync(StoragePreference storagePreference)
    {
        _preferences.StoragePreference = storagePreference ?? new StoragePreference();
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task SetModuleVisibilityAsync(HomeModuleType moduleType, bool isVisible)
    {
        if (isVisible)
        {
            _preferences.HiddenHomeModules.Remove(moduleType);
        }
        else
        {
            _preferences.HiddenHomeModules.Add(moduleType);
        }

        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public Task MoveModuleUpAsync(HomeModuleType moduleType)
    {
        MoveModule(moduleType, -1);
        return Task.CompletedTask;
    }

    public Task MoveModuleDownAsync(HomeModuleType moduleType)
    {
        MoveModule(moduleType, 1);
        return Task.CompletedTask;
    }

    private void MoveModule(HomeModuleType moduleType, int delta)
    {
        var order = _preferences.HomeModuleOrder;
        var index = order.IndexOf(moduleType);
        if (index < 0)
        {
            return;
        }

        var nextIndex = index + delta;
        if (nextIndex < 0 || nextIndex >= order.Count)
        {
            return;
        }

        (order[index], order[nextIndex]) = (order[nextIndex], order[index]);
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        Save();
        SaveRemote();
        Changed?.Invoke();
    }

    private void Load()
    {
        try
        {
            var raw = PlatformPreferenceStore.GetString(PreferenceStorageKey);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var saved = JsonSerializer.Deserialize<UserPreference>(raw);
                if (saved is not null)
                {
                    Apply(saved);
                }
            }
        }
        catch (Exception ex)
        {
            RuntimeDiagnostics.Record(nameof(UserPreferenceService), ex);
        }
    }

    private void Save()
    {
        try
        {
            var raw = JsonSerializer.Serialize(_preferences);
            PlatformPreferenceStore.SetString(PreferenceStorageKey, raw);
        }
        catch (Exception ex)
        {
            RuntimeDiagnostics.Record(nameof(UserPreferenceService), ex);
        }
    }

    private void SaveRemote()
    {
        try
        {
            var preferenceRepository = _serviceProvider?.GetService(typeof(PreferenceRepository)) as PreferenceRepository;
            preferenceRepository?.SaveUserPreferenceAsync(_preferences).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            RuntimeDiagnostics.Record(nameof(UserPreferenceService), ex);
        }
    }

    private void Apply(UserPreference saved)
    {
        _preferences.ThemeMode = saved.ThemeMode;
        _preferences.NotificationSettings = saved.NotificationSettings ?? new NotificationPreference();
        _preferences.HomeModuleOrder = saved.HomeModuleOrder.Count > 0 ? saved.HomeModuleOrder : _preferences.HomeModuleOrder;
        _preferences.HiddenHomeModules = saved.HiddenHomeModules.Count > 0 ? saved.HiddenHomeModules : [];
        _preferences.PrivacyMode = saved.PrivacyMode;
        _preferences.OnboardingCompleted = saved.OnboardingCompleted;
        _preferences.StoragePreference = saved.StoragePreference ?? new StoragePreference();
    }
}
