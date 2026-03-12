namespace HomeHoney.Models;

public enum InsuranceCategory
{
    Medical,
    CriticalIllness,
    Accident,
    Life,
    Other,
}

public enum InsuranceStatus
{
    Active,
    PendingRenewal,
    Expired,
    Archived,
}

public sealed class InsuranceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PolicyName { get; set; } = string.Empty;

    public Guid InsuredMemberId { get; set; }

    public InsuranceCategory InsuranceCategory { get; set; }

    public InsuranceStatus Status { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int AttachmentCount { get; set; }

    public FileResource? PrimaryFile { get; set; }

    public string IntegrityStatus { get; set; } = string.Empty;

    public string? SyncMessage { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime LastUpdatedAt { get; set; }
}
