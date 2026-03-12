using HomeHoney.Models;
using HomeHoney.Services.Preferences;
using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public sealed class StorageConnectionProfileService : IStorageConnectionProfileService
{
    private readonly UserPreferenceService _userPreferenceService;
    private readonly ISecretStore _secretStore;
    private readonly StorageConfigurationValidator _validator;
    private readonly IAdminStorageApiClient _adminStorageApiClient;

    public StorageConnectionProfileService(UserPreferenceService userPreferenceService, ISecretStore secretStore, StorageConfigurationValidator validator, IAdminStorageApiClient adminStorageApiClient)
    {
        _userPreferenceService = userPreferenceService;
        _secretStore = secretStore;
        _validator = validator;
        _adminStorageApiClient = adminStorageApiClient;
    }

    public async Task<StorageConnectionProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        var preference = _userPreferenceService.GetPreferences().StoragePreference;
        var profile = new StorageConnectionProfile
        {
            DisplayName = preference.DisplayName,
            ApiBaseUrl = preference.ApiBaseUrl,
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
        catch
        {
            return profile;
        }
    }

    public Task<string?> GetBackendBaseUrlAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(_userPreferenceService.GetPreferences().StoragePreference.ApiBaseUrl);

    public Task<string?> GetMongoConnectionStringAsync(CancellationToken cancellationToken = default)
        => _secretStore.GetSecretAsync(_userPreferenceService.GetPreferences().StoragePreference.MongoConnectionStringSecretKey, cancellationToken);

    public Task<StorageValidationResult> ValidateProfileAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default)
        => _validator.ValidateAsync(profile, mongoConnectionString, cancellationToken);

    public async Task<StorageProfileSaveResult> SaveProfileAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateProfileAsync(profile, mongoConnectionString, cancellationToken);
        profile.LastValidatedAt = DateTime.UtcNow;
        profile.ValidationStatus = validation.IsValid ? StorageValidationStatus.Valid : StorageValidationStatus.Invalid;
        profile.ValidationMessage = validation.Message;

        if (!validation.IsValid)
        {
            return new(false, profile, validation.Message);
        }

        var remoteSave = await _adminStorageApiClient.SaveProfileAsync(profile, mongoConnectionString, cancellationToken);
        profile = remoteSave.Profile;
        profile.LastValidatedAt ??= DateTime.UtcNow;

        if (!remoteSave.IsSuccess)
        {
            return remoteSave;
        }

        var secretKey = string.IsNullOrWhiteSpace(profile.MongoConnectionStringSecretKey)
            ? "storage.mongo.connection"
            : profile.MongoConnectionStringSecretKey;

        profile.MongoConnectionStringSecretKey = secretKey;
        profile.MongoConnectionStringPreview = MaskConnectionString(mongoConnectionString);
        await _secretStore.SetSecretAsync(secretKey, mongoConnectionString, cancellationToken);
        await _userPreferenceService.UpdateStoragePreferenceAsync(new StoragePreference
        {
            DisplayName = profile.DisplayName,
            ApiBaseUrl = profile.ApiBaseUrl,
            FileServiceBaseUrl = profile.FileServiceBaseUrl,
            FileServiceApiPath = profile.FileServiceApiPath,
            MongoConnectionStringSecretKey = profile.MongoConnectionStringSecretKey,
            MongoConnectionStringPreview = profile.MongoConnectionStringPreview,
            MongoDatabaseName = profile.MongoDatabaseName,
            IsActive = true,
            LastValidatedAt = profile.LastValidatedAt,
            ValidationStatus = profile.ValidationStatus,
            ValidationMessage = profile.ValidationMessage,
        });

        return new(true, profile, remoteSave.Message);
    }

    private static string MaskConnectionString(string connectionString)
    {
        try
        {
            var mongoUrl = MongoUrl.Create(connectionString);
            var credentials = mongoUrl.Username is null ? string.Empty : $"{mongoUrl.Username}:***@";
            var hosts = string.Join(",", mongoUrl.Servers.Select(server => $"{server.Host}:{server.Port}"));
            return $"mongodb://{credentials}{hosts}/{mongoUrl.DatabaseName ?? string.Empty}";
        }
        catch
        {
            return "mongodb://***";
        }
    }
}
