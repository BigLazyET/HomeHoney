using HomeHoney.Api.Contracts.Documents;
using HomeHoney.Models;

namespace HomeHoney.Api.Application.Documents;

public static class DocumentDtoMapper
{
    public static InsuranceRecordDto ToDto(InsuranceRecord record)
        => new()
        {
            Id = record.Id,
            PolicyName = record.PolicyName,
            InsuredMemberId = record.InsuredMemberId,
            InsuranceCategory = record.InsuranceCategory,
            Status = record.Status,
            EffectiveDate = record.EffectiveDate,
            ExpiryDate = record.ExpiryDate,
            ProviderName = record.ProviderName,
            ContactName = record.ContactName,
            ContactPhone = record.ContactPhone,
            Summary = record.Summary,
            AttachmentCount = record.AttachmentCount,
            PrimaryFile = ToDto(record.PrimaryFile),
            IntegrityStatus = record.IntegrityStatus,
            SyncMessage = record.SyncMessage,
            Tags = [.. record.Tags],
            LastUpdatedAt = record.LastUpdatedAt,
        };

    public static ManualRecordDto ToDto(ManualRecord record)
        => new()
        {
            Id = record.Id,
            DeviceName = record.DeviceName,
            Brand = record.Brand,
            Model = record.Model,
            SpaceId = record.SpaceId,
            ManualCategory = record.ManualCategory,
            PurchaseDate = record.PurchaseDate,
            WarrantyExpiryDate = record.WarrantyExpiryDate,
            Summary = record.Summary,
            AttachmentCount = record.AttachmentCount,
            PrimaryFile = ToDto(record.PrimaryFile),
            IntegrityStatus = record.IntegrityStatus,
            SyncMessage = record.SyncMessage,
            Tags = [.. record.Tags],
            LastUpdatedAt = record.LastUpdatedAt,
        };

    public static InsuranceRecord ToModel(UpsertInsuranceDocumentRequest request)
        => new()
        {
            Id = request.Id,
            PolicyName = request.PolicyName,
            InsuredMemberId = request.InsuredMemberId,
            InsuranceCategory = request.InsuranceCategory,
            Status = request.Status,
            EffectiveDate = request.EffectiveDate,
            ExpiryDate = request.ExpiryDate,
            ProviderName = request.ProviderName,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            Summary = request.Summary,
            Tags = [.. request.Tags],
        };

    public static ManualRecord ToModel(UpsertManualDocumentRequest request)
        => new()
        {
            Id = request.Id,
            DeviceName = request.DeviceName,
            Brand = request.Brand,
            Model = request.Model,
            SpaceId = request.SpaceId,
            ManualCategory = request.ManualCategory,
            PurchaseDate = request.PurchaseDate,
            WarrantyExpiryDate = request.WarrantyExpiryDate,
            Summary = request.Summary,
            Tags = [.. request.Tags],
        };

    public static FileResourceDto? ToDto(FileResource? resource)
        => resource is null
            ? null
            : new FileResourceDto
            {
                FileResourceId = resource.FileResourceId,
                ExternalFileId = resource.ExternalFileId,
                ExternalPath = resource.ExternalPath,
                FileName = resource.FileName,
                ContentType = resource.ContentType,
                SizeBytes = resource.SizeBytes,
                ChecksumOrEtag = resource.ChecksumOrEtag,
                UploadedAt = resource.UploadedAt,
                LastSyncedAt = resource.LastSyncedAt,
                AvailabilityStatus = resource.AvailabilityStatus,
                StatusMessage = resource.StatusMessage,
            };
}
