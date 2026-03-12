using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Preferences;

public sealed class NotificationPreferenceDto
{
    public bool InsuranceEnabled { get; set; } = true;

    public bool ManualEnabled { get; set; } = true;

    public bool MemoEnabled { get; set; } = true;

    public bool FridgeNoteEnabled { get; set; } = true;
}

public sealed class StoragePreferenceDto
{
    public string DisplayName { get; set; } = "默认后端";

    public string ApiBaseUrl { get; set; } = "http://localhost:7080";

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

public sealed class UserPreferenceDto
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    public NotificationPreferenceDto NotificationSettings { get; set; } = new();

    public List<HomeModuleType> HomeModuleOrder { get; set; } = [];

    public HashSet<HomeModuleType> HiddenHomeModules { get; set; } = [];

    public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.Standard;

    public bool OnboardingCompleted { get; set; }

    public StoragePreferenceDto StoragePreference { get; set; } = new();
}
