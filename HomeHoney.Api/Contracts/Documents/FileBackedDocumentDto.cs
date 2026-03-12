using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Documents;

public sealed class FileResourceDto
{
    public Guid FileResourceId { get; set; }

    public string? ExternalFileId { get; set; }

    public string? ExternalPath { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public string? ChecksumOrEtag { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public FileAvailabilityStatus AvailabilityStatus { get; set; } = FileAvailabilityStatus.Unknown;

    public string? StatusMessage { get; set; }
}

public sealed class InsuranceRecordDto
{
    public Guid Id { get; set; }

    public bool HasAttachment => AttachmentCount > 0 || PrimaryFile is not null;

    public string PolicyName { get; set; } = string.Empty;

    public Guid InsuredMemberId { get; set; }

    public InsuranceCategory InsuranceCategory { get; set; } = InsuranceCategory.Medical;

    public InsuranceStatus Status { get; set; } = InsuranceStatus.Active;

    public DateOnly EffectiveDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int AttachmentCount { get; set; }

    public FileResourceDto? PrimaryFile { get; set; }

    public string IntegrityStatus { get; set; } = "Healthy";

    public string? SyncMessage { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime LastUpdatedAt { get; set; }
}

public sealed class ManualRecordDto
{
    public Guid Id { get; set; }

    public bool HasAttachment => AttachmentCount > 0 || PrimaryFile is not null;

    public string DeviceName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public Guid SpaceId { get; set; }

    public ManualCategory ManualCategory { get; set; } = ManualCategory.Appliance;

    public DateOnly PurchaseDate { get; set; }

    public DateOnly WarrantyExpiryDate { get; set; }

    public string Summary { get; set; } = string.Empty;

    public int AttachmentCount { get; set; }

    public FileResourceDto? PrimaryFile { get; set; }

    public string IntegrityStatus { get; set; } = "Healthy";

    public string? SyncMessage { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime LastUpdatedAt { get; set; }
}
