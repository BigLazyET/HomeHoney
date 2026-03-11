using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public static class FileSyncStateMapper
{
    public static void ApplyUploadResult(InsuranceRecord record, FileStorageResult result)
    {
        Apply(record.PrimaryFile ??= new FileResource(), record, result);
    }

    public static void ApplyUploadResult(ManualRecord record, FileStorageResult result)
    {
        Apply(record.PrimaryFile ??= new FileResource(), record, result);
    }

    private static void Apply(FileResource resource, InsuranceRecord record, FileStorageResult result)
    {
        resource.ExternalPath = result.RemotePath;
        resource.ExternalFileId = result.RemoteId;
        resource.ContentType = result.ContentType ?? resource.ContentType;
        resource.SizeBytes = result.SizeBytes ?? resource.SizeBytes;
        resource.AvailabilityStatus = result.AvailabilityStatus;
        resource.LastSyncedAt = DateTime.UtcNow;
        resource.StatusMessage = result.Message;
        record.AttachmentCount = result.IsSuccess ? 1 : record.AttachmentCount;
        record.IntegrityStatus = result.IsSuccess ? "Healthy" : "SyncError";
        record.SyncMessage = result.Message;
    }

    private static void Apply(FileResource resource, ManualRecord record, FileStorageResult result)
    {
        resource.ExternalPath = result.RemotePath;
        resource.ExternalFileId = result.RemoteId;
        resource.ContentType = result.ContentType ?? resource.ContentType;
        resource.SizeBytes = result.SizeBytes ?? resource.SizeBytes;
        resource.AvailabilityStatus = result.AvailabilityStatus;
        resource.LastSyncedAt = DateTime.UtcNow;
        resource.StatusMessage = result.Message;
        record.AttachmentCount = result.IsSuccess ? 1 : record.AttachmentCount;
        record.IntegrityStatus = result.IsSuccess ? "Healthy" : "SyncError";
        record.SyncMessage = result.Message;
    }
}
