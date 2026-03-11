namespace HomeHoney.Models;

public enum FileAvailabilityStatus
{
    Unknown,
    PendingUpload,
    Available,
    Missing,
    SyncError,
}

public sealed class FileResource
{
    public Guid FileResourceId { get; set; } = Guid.NewGuid();

    public string? ExternalFileId { get; set; }

    public string? ExternalPath { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public string? ChecksumOrEtag { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastSyncedAt { get; set; }

    public FileAvailabilityStatus AvailabilityStatus { get; set; } = FileAvailabilityStatus.Unknown;

    public string? StatusMessage { get; set; }
}
