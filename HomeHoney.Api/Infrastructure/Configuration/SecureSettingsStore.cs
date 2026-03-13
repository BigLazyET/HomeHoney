using System.Collections.Concurrent;
using HomeHoney.Api.Contracts.Storage;

namespace HomeHoney.Api.Infrastructure.Configuration;

public sealed class SecureSettingsStore
{
    private const string BackendDisplayNameKey = "backend.profile.display-name";
    private const string BackendBaseUrlKey = "backend.profile.base-url";
    private const string BackendLastValidatedAtKey = "backend.profile.last-validated-at";
    private const string BackendValidationStatusKey = "backend.profile.validation-status";
    private const string BackendValidationMessageKey = "backend.profile.validation-message";
    private const string DownstreamMongoConnectionKey = "backend.storage.mongo-connection";
    private const string DownstreamFileServicePasswordKey = "backend.storage.file-service-password";
    private const string DownstreamLastValidatedAtKey = "backend.storage.last-validated-at";
    private const string DownstreamValidationStatusKey = "backend.storage.validation-status";
    private const string DownstreamValidationMessageKey = "backend.storage.validation-message";
    private const string DownstreamFileServiceAuthHeaderValueKey = "backend.storage.file-service-auth-header-value";

    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, string> _overrides = new(StringComparer.Ordinal);

    public SecureSettingsStore(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public BackendServiceProfileDto GetBackendProfile()
        => new()
        {
            DisplayName = GetValue(BackendDisplayNameKey, "默认后端"),
            ApiBaseUrl = GetValue(BackendBaseUrlKey, "http://localhost:7080"),
            IsActive = true,
            LastValidatedAt = GetDateTimeValue(BackendLastValidatedAtKey),
            ValidationStatus = GetValidationStatus(BackendValidationStatusKey),
            ValidationMessage = GetNullableValue(BackendValidationMessageKey),
        };

    public BackendServiceProfileDto SaveBackendProfile(string displayName, string apiBaseUrl)
    {
        _overrides[BackendDisplayNameKey] = displayName;
        _overrides[BackendBaseUrlKey] = apiBaseUrl.TrimEnd('/');
        return GetBackendProfile();
    }

    public DownstreamStorageSettingsDto GetDownstreamSettings()
    {
        var options = GetEffectiveStorageOptions();
        var connectionString = options.MongoConnectionString;
        return new()
        {
            FileServiceBaseUrl = options.FileServiceBaseUrl,
            FileServiceApiPath = options.FileServiceApiPath,
            FileServiceUsername = options.FileServiceUsername,
            FileServiceAuthHeaderName = options.FileServiceAuthHeaderName,
            HasFileServicePassword = !string.IsNullOrWhiteSpace(options.FileServicePassword),
            HasFileServiceAuthHeaderValue = !string.IsNullOrWhiteSpace(options.FileServiceAuthHeaderValue),
            MongoDatabaseName = options.MongoDatabaseName,
            LastValidatedAt = GetDateTimeValue(DownstreamLastValidatedAtKey),
            MongoConnectionStringPreview = MaskConnectionString(connectionString),
            ValidationStatus = GetValidationStatus(DownstreamValidationStatusKey),
            ValidationMessage = GetNullableValue(DownstreamValidationMessageKey),
        };
    }

    public DownstreamStorageSettingsDto SaveDownstreamSettings(UpdateDownstreamStorageSettingsRequest request)
    {
        _overrides[$"{BackendStorageOptions.SectionName}:FileServiceBaseUrl"] = request.FileServiceBaseUrl.TrimEnd('/');
        _overrides[$"{BackendStorageOptions.SectionName}:FileServiceApiPath"] = request.FileServiceApiPath;
        if (request.FileServiceUsername is not null)
        {
            _overrides[$"{BackendStorageOptions.SectionName}:FileServiceUsername"] = request.FileServiceUsername.Trim();
        }

        if (request.FileServicePassword is not null)
        {
            _overrides[DownstreamFileServicePasswordKey] = request.FileServicePassword;
        }

        if (request.FileServiceAuthHeaderName is not null)
        {
            _overrides[$"{BackendStorageOptions.SectionName}:FileServiceAuthHeaderName"] = request.FileServiceAuthHeaderName.Trim();
        }

        if (request.FileServiceAuthHeaderValue is not null)
        {
            _overrides[DownstreamFileServiceAuthHeaderValueKey] = request.FileServiceAuthHeaderValue;
        }

        _overrides[$"{BackendStorageOptions.SectionName}:MongoDatabaseName"] = request.MongoDatabaseName;
        _overrides[DownstreamMongoConnectionKey] = request.MongoConnectionString;
        return GetDownstreamSettings();
    }

    public void SaveBackendValidation(ValidationStatusDto status, string? message, DateTime validatedAt)
    {
        _overrides[BackendLastValidatedAtKey] = validatedAt.ToString("O");
        _overrides[BackendValidationStatusKey] = status.ToString();
        _overrides[BackendValidationMessageKey] = message ?? string.Empty;
    }

    public void SaveDownstreamValidation(ValidationStatusDto status, string? message, DateTime validatedAt)
    {
        _overrides[DownstreamLastValidatedAtKey] = validatedAt.ToString("O");
        _overrides[DownstreamValidationStatusKey] = status.ToString();
        _overrides[DownstreamValidationMessageKey] = message ?? string.Empty;
    }

    public BackendStorageOptions GetEffectiveStorageOptions()
    {
        var options = _configuration.GetSection(BackendStorageOptions.SectionName).Get<BackendStorageOptions>() ?? new BackendStorageOptions();
        options.FileServiceBaseUrl = GetValue($"{BackendStorageOptions.SectionName}:FileServiceBaseUrl", options.FileServiceBaseUrl);
        options.FileServiceApiPath = GetValue($"{BackendStorageOptions.SectionName}:FileServiceApiPath", options.FileServiceApiPath);
        options.FileServiceUsername = GetValue($"{BackendStorageOptions.SectionName}:FileServiceUsername", options.FileServiceUsername);
        options.FileServicePassword = GetValue(DownstreamFileServicePasswordKey, options.FileServicePassword);
        options.FileServiceAuthHeaderName = GetValue($"{BackendStorageOptions.SectionName}:FileServiceAuthHeaderName", options.FileServiceAuthHeaderName);
        options.FileServiceAuthHeaderValue = GetValue(DownstreamFileServiceAuthHeaderValueKey, options.FileServiceAuthHeaderValue);
        options.MongoDatabaseName = GetValue($"{BackendStorageOptions.SectionName}:MongoDatabaseName", options.MongoDatabaseName);
        options.MongoConnectionString = GetValue(DownstreamMongoConnectionKey, options.MongoConnectionString);
        return options;
    }

    public BackendStorageOptions GetEffectiveStorageOptions(UpdateDownstreamStorageSettingsRequest request)
    {
        var options = GetEffectiveStorageOptions();
        options.FileServiceBaseUrl = request.FileServiceBaseUrl.TrimEnd('/');
        options.FileServiceApiPath = request.FileServiceApiPath;
        if (request.FileServiceUsername is not null)
        {
            options.FileServiceUsername = request.FileServiceUsername.Trim();
        }

        if (request.FileServicePassword is not null)
        {
            options.FileServicePassword = request.FileServicePassword;
        }

        if (request.FileServiceAuthHeaderName is not null)
        {
            options.FileServiceAuthHeaderName = request.FileServiceAuthHeaderName.Trim();
        }

        if (request.FileServiceAuthHeaderValue is not null)
        {
            options.FileServiceAuthHeaderValue = request.FileServiceAuthHeaderValue;
        }

        options.MongoDatabaseName = request.MongoDatabaseName;
        options.MongoConnectionString = request.MongoConnectionString;
        return options;
    }

    private string GetValue(string key, string fallback)
        => _overrides.TryGetValue(key, out var value) ? value : _configuration[key] ?? fallback;

    private string? GetNullableValue(string key)
        => _overrides.TryGetValue(key, out var value)
            ? string.IsNullOrWhiteSpace(value) ? null : value
            : string.IsNullOrWhiteSpace(_configuration[key]) ? null : _configuration[key];

    private DateTime? GetDateTimeValue(string key)
        => DateTime.TryParse(GetNullableValue(key), out var value) ? value : null;

    private ValidationStatusDto GetValidationStatus(string key)
        => Enum.TryParse<ValidationStatusDto>(GetNullableValue(key), ignoreCase: true, out var value) ? value : ValidationStatusDto.Unknown;

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        try
        {
            var mongoUrl = MongoDB.Driver.MongoUrl.Create(connectionString);
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
