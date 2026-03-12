namespace HomeHoney.Models;

public enum StorageValidationStatus
{
    Unknown,
    Valid,
    Invalid,
}

public sealed class StorageConnectionProfile
{
    public const string DefaultBackendApiBaseUrl = "http://localhost:7080";

    public Guid ProfileId { get; set; } = Guid.NewGuid();

    public string DisplayName { get; set; } = "默认后端";

    public string ApiBaseUrl { get; set; } = DefaultBackendApiBaseUrl;

    public string FileServiceBaseUrl { get; set; } = "http://localhost:8999";

    public string FileServiceApiPath { get; set; } = "/api";

    public string MongoConnectionStringSecretKey { get; set; } = "storage.mongo.connection";

    public string MongoConnectionStringPreview { get; set; } = string.Empty;

    public string MongoDatabaseName { get; set; } = "homehoney";

    public bool IsActive { get; set; } = true;

    public DateTime? LastValidatedAt { get; set; }

    public StorageValidationStatus ValidationStatus { get; set; } = StorageValidationStatus.Unknown;

    public string? ValidationMessage { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiBaseUrl);
}
