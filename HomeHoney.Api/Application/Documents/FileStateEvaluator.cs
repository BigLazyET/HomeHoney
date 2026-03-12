using HomeHoney.Models;
using HomeHoney.Services.Storage;

namespace HomeHoney.Api.Application.Documents;

public static class FileStateEvaluator
{
    public static void ApplyUploadResult(InsuranceRecord record, FileStorageResult result)
    {
        Apply(record.PrimaryFile ??= new FileResource(), result);
        ApplyState(record);
        record.SyncMessage = result.Message;
    }

    public static void ApplyUploadResult(ManualRecord record, FileStorageResult result)
    {
        Apply(record.PrimaryFile ??= new FileResource(), result);
        ApplyState(record);
        record.SyncMessage = result.Message;
    }

    public static InsuranceRecord ApplyState(InsuranceRecord record)
    {
        Evaluate(record.PrimaryFile, updateCount: count => record.AttachmentCount = count, updateIntegrity: value => record.IntegrityStatus = value, updateMessage: value => record.SyncMessage = value);
        return record;
    }

    public static ManualRecord ApplyState(ManualRecord record)
    {
        Evaluate(record.PrimaryFile, updateCount: count => record.AttachmentCount = count, updateIntegrity: value => record.IntegrityStatus = value, updateMessage: value => record.SyncMessage = value);
        return record;
    }

    private static void Apply(FileResource resource, FileStorageResult result)
    {
        resource.ExternalPath = result.RemotePath;
        resource.ExternalFileId = result.RemoteId;
        resource.FileName = string.IsNullOrWhiteSpace(result.FileName) ? resource.FileName : result.FileName;
        resource.ContentType = result.ContentType ?? resource.ContentType;
        resource.SizeBytes = result.SizeBytes ?? resource.SizeBytes;
        resource.AvailabilityStatus = result.AvailabilityStatus;
        resource.LastSyncedAt = result.LastSyncedAt ?? DateTime.UtcNow;
        resource.StatusMessage = result.Message;
    }

    private static void Evaluate(FileResource? resource, Action<int> updateCount, Action<string> updateIntegrity, Action<string?> updateMessage)
    {
        if (resource is null)
        {
            updateCount(0);
            updateIntegrity("MetadataOutOfSync");
            updateMessage("资料已保存，等待上传文件。");
            return;
        }

        switch (resource.AvailabilityStatus)
        {
            case FileAvailabilityStatus.Available:
                updateCount(1);
                updateIntegrity("Healthy");
                updateMessage(resource.StatusMessage);
                break;
            case FileAvailabilityStatus.Missing:
                updateCount(1);
                updateIntegrity("FileMissing");
                updateMessage(resource.StatusMessage ?? "文件当前不可用。");
                break;
            case FileAvailabilityStatus.SyncError:
                updateCount(1);
                updateIntegrity("SyncError");
                updateMessage(resource.StatusMessage ?? "文件同步失败。");
                break;
            case FileAvailabilityStatus.PendingUpload:
            case FileAvailabilityStatus.Unknown:
            default:
                updateCount(0);
                updateIntegrity("MetadataOutOfSync");
                updateMessage(resource.StatusMessage ?? "资料已保存，等待上传文件。");
                break;
        }
    }
}