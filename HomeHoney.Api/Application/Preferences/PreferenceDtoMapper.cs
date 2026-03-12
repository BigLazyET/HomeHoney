using HomeHoney.Api.Contracts.Preferences;
using HomeHoney.Models;

namespace HomeHoney.Api.Application.Preferences;

public static class PreferenceDtoMapper
{
    public static UserPreferenceDto ToDto(UserPreference preference)
        => new()
        {
            ThemeMode = preference.ThemeMode,
            NotificationSettings = new NotificationPreferenceDto
            {
                InsuranceEnabled = preference.NotificationSettings.InsuranceEnabled,
                ManualEnabled = preference.NotificationSettings.ManualEnabled,
                MemoEnabled = preference.NotificationSettings.MemoEnabled,
                FridgeNoteEnabled = preference.NotificationSettings.FridgeNoteEnabled,
            },
            HomeModuleOrder = [.. preference.HomeModuleOrder],
            HiddenHomeModules = [.. preference.HiddenHomeModules],
            PrivacyMode = preference.PrivacyMode,
            OnboardingCompleted = preference.OnboardingCompleted,
            StoragePreference = new StoragePreferenceDto
            {
                DisplayName = preference.StoragePreference.DisplayName,
                ApiBaseUrl = preference.StoragePreference.ApiBaseUrl,
                FileServiceBaseUrl = preference.StoragePreference.FileServiceBaseUrl,
                FileServiceApiPath = preference.StoragePreference.FileServiceApiPath,
                MongoConnectionStringSecretKey = preference.StoragePreference.MongoConnectionStringSecretKey,
                MongoConnectionStringPreview = preference.StoragePreference.MongoConnectionStringPreview,
                MongoDatabaseName = preference.StoragePreference.MongoDatabaseName,
                IsActive = preference.StoragePreference.IsActive,
                LastValidatedAt = preference.StoragePreference.LastValidatedAt,
                ValidationStatus = preference.StoragePreference.ValidationStatus,
                ValidationMessage = preference.StoragePreference.ValidationMessage,
            },
        };

    public static UserPreference ToModel(UserPreferenceDto preference)
        => new()
        {
            ThemeMode = preference.ThemeMode,
            NotificationSettings = new NotificationPreference
            {
                InsuranceEnabled = preference.NotificationSettings.InsuranceEnabled,
                ManualEnabled = preference.NotificationSettings.ManualEnabled,
                MemoEnabled = preference.NotificationSettings.MemoEnabled,
                FridgeNoteEnabled = preference.NotificationSettings.FridgeNoteEnabled,
            },
            HomeModuleOrder = [.. preference.HomeModuleOrder],
            HiddenHomeModules = [.. preference.HiddenHomeModules],
            PrivacyMode = preference.PrivacyMode,
            OnboardingCompleted = preference.OnboardingCompleted,
            StoragePreference = new StoragePreference
            {
                DisplayName = preference.StoragePreference.DisplayName,
                ApiBaseUrl = preference.StoragePreference.ApiBaseUrl,
                FileServiceBaseUrl = preference.StoragePreference.FileServiceBaseUrl,
                FileServiceApiPath = preference.StoragePreference.FileServiceApiPath,
                MongoConnectionStringSecretKey = preference.StoragePreference.MongoConnectionStringSecretKey,
                MongoConnectionStringPreview = preference.StoragePreference.MongoConnectionStringPreview,
                MongoDatabaseName = preference.StoragePreference.MongoDatabaseName,
                IsActive = preference.StoragePreference.IsActive,
                LastValidatedAt = preference.StoragePreference.LastValidatedAt,
                ValidationStatus = preference.StoragePreference.ValidationStatus,
                ValidationMessage = preference.StoragePreference.ValidationMessage,
            },
        };
}
