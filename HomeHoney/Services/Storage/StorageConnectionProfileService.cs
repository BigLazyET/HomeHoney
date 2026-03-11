using HomeHoney.Models;
using HomeHoney.Services.Preferences;
using MongoDB.Driver;

namespace HomeHoney.Services.Storage;

public sealed class StorageConnectionProfileService : IStorageConnectionProfileService
{
    private readonly UserPreferenceService _userPreferenceService;
    private readonly ISecretStore _secretStore;
    private readonly StorageConfigurationValidator _validator;

    public StorageConnectionProfileService(UserPreferenceService userPreferenceService, ISecretStore secretStore, StorageConfigurationValidator validator)
    {
        _userPreferenceService = userPreferenceService;
        _secretStore = secretStore;
        _validator = validator;
    }

    public Task<StorageConnectionProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        var preference = _userPreferenceService.GetPreferences().StoragePreference;
        var profile = new StorageConnectionProfile
        {
            DisplayName = preference.DisplayName,
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

        return Task.FromResult(profile);
    }

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

        var secretKey = string.IsNullOrWhiteSpace(profile.MongoConnectionStringSecretKey)
            ? "storage.mongo.connection"
            : profile.MongoConnectionStringSecretKey;

        profile.MongoConnectionStringSecretKey = secretKey;
        profile.MongoConnectionStringPreview = MaskConnectionString(mongoConnectionString);
        await _secretStore.SetSecretAsync(secretKey, mongoConnectionString, cancellationToken);
        await _userPreferenceService.UpdateStoragePreferenceAsync(new StoragePreference
        {
            DisplayName = profile.DisplayName,
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

        return new(true, profile, validation.Message);
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
