using HomeHoney.Models;
using HomeHoney.Services.Preferences;
using System.Diagnostics;

namespace HomeHoney.Services.Storage;

public sealed class StorageConnectionProfileService : IStorageConnectionProfileService
{
    private readonly UserPreferenceService _userPreferenceService;
    private readonly StorageConfigurationValidator _validator;
    private readonly IAdminStorageApiClient _adminStorageApiClient;

    public StorageConnectionProfileService(UserPreferenceService userPreferenceService, StorageConfigurationValidator validator, IAdminStorageApiClient adminStorageApiClient)
    {
        _userPreferenceService = userPreferenceService;
        _validator = validator;
        _adminStorageApiClient = adminStorageApiClient;
    }

    public async Task<StorageConnectionProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        var preference = _userPreferenceService.GetPreferences().StoragePreference;
        var profile = new StorageConnectionProfile
        {
            DisplayName = preference.DisplayName,
            ApiBaseUrl = NormalizeApiBaseUrl(preference.ApiBaseUrl),
            FileServiceBaseUrl = preference.FileServiceBaseUrl,
            FileServiceApiPath = preference.FileServiceApiPath,
            MongoConnectionStringSecretKey = preference.MongoConnectionStringSecretKey,
            MongoConnectionStringPreview = preference.MongoConnectionStringPreview,
            MongoDatabaseName = preference.MongoDatabaseName,
            IsActive = preference.IsActive,
            LastValidatedAt = preference.LastValidatedAt,
            ValidationStatus = preference.ValidationStatus,
            ValidationMessage = preference.ValidationMessage,
        };

        if (!Uri.TryCreate(profile.ApiBaseUrl, UriKind.Absolute, out _))
        {
            return profile;
        }

        try
        {
            return await _adminStorageApiClient.GetProfileAsync(profile.ApiBaseUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetActiveProfileAsync Error: {ex}");
            return profile;
        }
    }

    public Task<string?> GetBackendBaseUrlAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(NormalizeApiBaseUrl(_userPreferenceService.GetPreferences().StoragePreference.ApiBaseUrl));

    public Task<StorageValidationResult> ValidateProfileAsync(StorageConnectionProfile profile, CancellationToken cancellationToken = default)
        => _validator.ValidateAsync(NormalizeProfile(profile), cancellationToken);

    public async Task<StorageProfileSaveResult> SaveProfileAsync(StorageConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        profile = NormalizeProfile(profile);
        var validation = await ValidateProfileAsync(profile, cancellationToken);
        profile.LastValidatedAt = DateTime.UtcNow;
        profile.ValidationStatus = validation.IsValid ? StorageValidationStatus.Valid : StorageValidationStatus.Invalid;
        profile.ValidationMessage = validation.Message;

        if (!validation.IsValid)
        {
            return new(false, profile, validation.Message);
        }

        var remoteSave = await _adminStorageApiClient.SaveProfileAsync(profile, cancellationToken);
        profile = remoteSave.Profile;
        profile.LastValidatedAt ??= DateTime.UtcNow;

        if (!remoteSave.IsSuccess)
        {
            return remoteSave;
        }

        var current = _userPreferenceService.GetPreferences().StoragePreference;
        await _userPreferenceService.UpdateStoragePreferenceAsync(new StoragePreference
        {
            DisplayName = profile.DisplayName,
            ApiBaseUrl = profile.ApiBaseUrl,
            FileServiceBaseUrl = current.FileServiceBaseUrl,
            FileServiceApiPath = current.FileServiceApiPath,
            MongoConnectionStringSecretKey = current.MongoConnectionStringSecretKey,
            MongoConnectionStringPreview = current.MongoConnectionStringPreview,
            MongoDatabaseName = current.MongoDatabaseName,
            IsActive = true,
            LastValidatedAt = profile.LastValidatedAt,
            ValidationStatus = profile.ValidationStatus,
            ValidationMessage = profile.ValidationMessage,
        });

        return new(true, profile, remoteSave.Message);
    }

    private static StorageConnectionProfile NormalizeProfile(StorageConnectionProfile profile)
    {
        profile.DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? "默认后端" : profile.DisplayName.Trim();
        profile.ApiBaseUrl = NormalizeApiBaseUrl(profile.ApiBaseUrl);
        return profile;
    }

    private static string NormalizeApiBaseUrl(string? apiBaseUrl)
        => string.IsNullOrWhiteSpace(apiBaseUrl)
            ? StorageConnectionProfile.DefaultBackendApiBaseUrl
            : apiBaseUrl.Trim();
}
