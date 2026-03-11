using HomeHoney.Models;
using HomeHoney.Services.Storage;
using System.IO;
using System.Text.Json;

namespace HomeHoney.Services.Preferences;

public sealed class UserPreferenceService
{
    private static readonly string PreferenceStoragePath = Path.Combine(AppContext.BaseDirectory, "homehoney.user-preferences.json");
    private readonly UserPreference _preferences = new();
    private readonly PreferenceRepository? _preferenceRepository;

    public event Action? Changed;

    public UserPreferenceService()
    {
        Load();
    }

    public UserPreferenceService(PreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
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
            if (File.Exists(PreferenceStoragePath))
            {
                var raw = File.ReadAllText(PreferenceStoragePath);
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    var saved = JsonSerializer.Deserialize<UserPreference>(raw);
                    if (saved is not null)
                    {
                        Apply(saved);
                    }
                }
            }

            LoadRemote();
        }
        catch
        {
            // Ignore persistence failures and keep in-memory defaults.
        }
    }

    private void Save()
    {
        try
        {
            var raw = JsonSerializer.Serialize(_preferences);
            File.WriteAllText(PreferenceStoragePath, raw);
        }
        catch
        {
            // Ignore persistence failures and keep current session state.
        }
    }

    private void LoadRemote()
    {
        try
        {
            if (_preferenceRepository is null)
            {
                return;
            }

            var remote = _preferenceRepository.GetUserPreferenceAsync().GetAwaiter().GetResult();
            if (remote is not null)
            {
                Apply(remote);
            }
        }
        catch
        {
            // Keep local settings if remote load fails.
        }
    }

    private void SaveRemote()
    {
        try
        {
            _preferenceRepository?.SaveUserPreferenceAsync(_preferences).GetAwaiter().GetResult();
        }
        catch
        {
            // Remote persistence is best-effort and should not break local preferences.
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
