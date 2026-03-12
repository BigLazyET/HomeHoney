namespace HomeHoney.Models;

public enum ThemeMode
{
    Light,
    Dark,
    System,
}

public enum PrivacyMode
{
    Standard,
    MaskSensitive,
    RequireDetailVerification,
}

public sealed class NotificationPreference
{
    public bool InsuranceEnabled { get; set; } = true;

    public bool ManualEnabled { get; set; } = true;

    public bool MemoEnabled { get; set; } = true;

    public bool FridgeNoteEnabled { get; set; } = true;
}

public sealed class StoragePreference
{
    public string DisplayName { get; set; } = "默认后端";

    public string ApiBaseUrl { get; set; } = StorageConnectionProfile.DefaultBackendApiBaseUrl;

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

public sealed class UserPreference
{
    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    public NotificationPreference NotificationSettings { get; set; } = new();

    public List<HomeModuleType> HomeModuleOrder { get; set; } =
    [
        HomeModuleType.QuickActions,
        HomeModuleType.UpcomingReminders,
    ];

    public HashSet<HomeModuleType> HiddenHomeModules { get; set; } = [];

    public PrivacyMode PrivacyMode { get; set; } = PrivacyMode.Standard;

    public bool OnboardingCompleted { get; set; }

    public StoragePreference StoragePreference { get; set; } = new();
}
