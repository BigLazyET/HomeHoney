namespace HomeHoney.Api.Contracts.Storage;

public sealed class DownstreamStorageSettingsDto
{
    public string FileServiceBaseUrl { get; set; } = "http://localhost:8999";

    public string FileServiceApiPath { get; set; } = "/api";

    public string FileServiceUsername { get; set; } = string.Empty;

    public string FileServiceAuthHeaderName { get; set; } = string.Empty;

    public bool HasFileServicePassword { get; set; }

    public bool HasFileServiceAuthHeaderValue { get; set; }

    public string MongoDatabaseName { get; set; } = "homehoney";

    public DateTime? LastValidatedAt { get; set; }

    public string MongoConnectionStringPreview { get; set; } = string.Empty;

    public ValidationStatusDto ValidationStatus { get; set; } = ValidationStatusDto.Unknown;

    public string? ValidationMessage { get; set; }
}

public sealed class UpdateBackendServiceProfileRequest
{
    public string DisplayName { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = string.Empty;
}

public sealed class UpdateDownstreamStorageSettingsRequest
{
    public string FileServiceBaseUrl { get; set; } = string.Empty;

    public string FileServiceApiPath { get; set; } = "/api";

    public string? FileServiceUsername { get; set; }

    public string? FileServicePassword { get; set; }

    public string? FileServiceAuthHeaderName { get; set; }

    public string? FileServiceAuthHeaderValue { get; set; }

    public string MongoDatabaseName { get; set; } = string.Empty;

    public string MongoConnectionString { get; set; } = string.Empty;
}
