namespace HomeHoney.Api.Contracts.Storage;

public enum ValidationStatusDto
{
    Unknown,
    Valid,
    Invalid,
}

public sealed class BackendServiceProfileDto
{
    public Guid ProfileId { get; set; } = Guid.NewGuid();

    public string DisplayName { get; set; } = "默认后端";

    public string ApiBaseUrl { get; set; } = "https://localhost:7080";

    public bool IsActive { get; set; } = true;

    public DateTime? LastValidatedAt { get; set; }

    public ValidationStatusDto ValidationStatus { get; set; } = ValidationStatusDto.Unknown;

    public string? ValidationMessage { get; set; }
}
