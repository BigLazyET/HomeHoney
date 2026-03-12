using System.Net.Http.Json;
using HomeHoney.Models;
using HomeHoney.Services.Storage;
using System.Text.Json;

namespace HomeHoney.Services.Preferences;

public sealed class PreferenceApiClient : IPreferenceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly BackendApiHttpClientFactory _httpClientFactory;

    public PreferenceApiClient(BackendApiHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<UserPreference?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.GetAsync("api/v1/preferences/me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var dto = await response.Content.ReadFromJsonAsync<UserPreferenceDto>(cancellationToken: cancellationToken);
        return dto is null ? null : Map(dto);
    }

    public async Task<UserPreference?> UpdateAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        var client = await _httpClientFactory.CreateAsync(cancellationToken);
        using var response = await client.PutAsJsonAsync("api/v1/preferences/me", Map(preference), JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<UserPreferenceDto>>(JsonOptions, cancellationToken);
        return envelope?.Data is null ? null : Map(envelope.Data);
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }

    private sealed class UserPreferenceDto
    {
        public ThemeMode ThemeMode { get; set; } = ThemeMode.System;
        public NotificationPreferenceDto NotificationSettings { get; set; } = new();
        public List<HomeModuleType> HomeModuleOrder { get; set; } = [];
        public HashSet<HomeModuleType> HiddenHomeModules { get; set; } = [];
        public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.Standard;
        public bool OnboardingCompleted { get; set; }
        public StoragePreferenceDto StoragePreference { get; set; } = new();
    }

    private sealed class NotificationPreferenceDto
    {
        public bool InsuranceEnabled { get; set; } = true;
        public bool ManualEnabled { get; set; } = true;
        public bool MemoEnabled { get; set; } = true;
        public bool FridgeNoteEnabled { get; set; } = true;
    }

    private sealed class StoragePreferenceDto
    {
        public string DisplayName { get; set; } = "默认后端";
        public string ApiBaseUrl { get; set; } = "https://localhost:7080";
        public string FileServiceBaseUrl { get; set; } = "http://localhost:8999";
        public string FileServiceApiPath { get; set; } = "/api";
        public string MongoConnectionStringSecretKey { get; set; } = "storage.mongo.connection";
        public string MongoConnectionStringPreview { get; set; } = string.Empty;
        public string MongoDatabaseName { get; set; } = "homehoney";
        public bool IsActive { get; set; } = true;
        public DateTime? LastValidatedAt { get; set; }
        public StorageValidationStatus ValidationStatus { get; set; } = StorageValidationStatus.Unknown;
        public string? ValidationMessage { get; set; }
    }

    private static UserPreference Map(UserPreferenceDto dto)
        => new()
        {
            ThemeMode = dto.ThemeMode,
            NotificationSettings = new NotificationPreference
            {
                InsuranceEnabled = dto.NotificationSettings.InsuranceEnabled,
                ManualEnabled = dto.NotificationSettings.ManualEnabled,
                MemoEnabled = dto.NotificationSettings.MemoEnabled,
                FridgeNoteEnabled = dto.NotificationSettings.FridgeNoteEnabled,
            },
            HomeModuleOrder = [.. dto.HomeModuleOrder],
            HiddenHomeModules = [.. dto.HiddenHomeModules],
            PrivacyMode = dto.PrivacyMode,
            OnboardingCompleted = dto.OnboardingCompleted,
            StoragePreference = new StoragePreference
            {
                DisplayName = dto.StoragePreference.DisplayName,
                ApiBaseUrl = dto.StoragePreference.ApiBaseUrl,
                FileServiceBaseUrl = dto.StoragePreference.FileServiceBaseUrl,
                FileServiceApiPath = dto.StoragePreference.FileServiceApiPath,
                MongoConnectionStringSecretKey = dto.StoragePreference.MongoConnectionStringSecretKey,
                MongoConnectionStringPreview = dto.StoragePreference.MongoConnectionStringPreview,
                MongoDatabaseName = dto.StoragePreference.MongoDatabaseName,
                IsActive = dto.StoragePreference.IsActive,
                LastValidatedAt = dto.StoragePreference.LastValidatedAt,
                ValidationStatus = dto.StoragePreference.ValidationStatus,
                ValidationMessage = dto.StoragePreference.ValidationMessage,
            },
        };

    private static UserPreferenceDto Map(UserPreference model)
        => new()
        {
            ThemeMode = model.ThemeMode,
            NotificationSettings = new NotificationPreferenceDto
            {
                InsuranceEnabled = model.NotificationSettings.InsuranceEnabled,
                ManualEnabled = model.NotificationSettings.ManualEnabled,
                MemoEnabled = model.NotificationSettings.MemoEnabled,
                FridgeNoteEnabled = model.NotificationSettings.FridgeNoteEnabled,
            },
            HomeModuleOrder = [.. model.HomeModuleOrder],
            HiddenHomeModules = [.. model.HiddenHomeModules],
            PrivacyMode = model.PrivacyMode,
            OnboardingCompleted = model.OnboardingCompleted,
            StoragePreference = new StoragePreferenceDto
            {
                DisplayName = model.StoragePreference.DisplayName,
                ApiBaseUrl = model.StoragePreference.ApiBaseUrl,
                FileServiceBaseUrl = model.StoragePreference.FileServiceBaseUrl,
                FileServiceApiPath = model.StoragePreference.FileServiceApiPath,
                MongoConnectionStringSecretKey = model.StoragePreference.MongoConnectionStringSecretKey,
                MongoConnectionStringPreview = model.StoragePreference.MongoConnectionStringPreview,
                MongoDatabaseName = model.StoragePreference.MongoDatabaseName,
                IsActive = model.StoragePreference.IsActive,
                LastValidatedAt = model.StoragePreference.LastValidatedAt,
                ValidationStatus = model.StoragePreference.ValidationStatus,
                ValidationMessage = model.StoragePreference.ValidationMessage,
            },
        };
}