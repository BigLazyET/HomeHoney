using HomeHoney.Api.Application.Shared;
using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Infrastructure.Configuration;
using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Api.Infrastructure.Mongo;
using HomeHoney.Api.Infrastructure.Mongo.Collections;
using HomeHoney.Models;
using HomeHoney.Services.Storage;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Documents;

public sealed class DocumentWriteService
{
    private readonly IMongoDatabaseFactory _mongoDatabaseFactory;
    private readonly SeedDataService _seedDataService;
    private readonly IFileBrowserGateway _fileBrowserGateway;
    private readonly SecureSettingsStore _secureSettingsStore;
    private readonly DocumentReadService _documentReadService;

    public DocumentWriteService(
        IMongoDatabaseFactory mongoDatabaseFactory,
        SeedDataService seedDataService,
        IFileBrowserGateway fileBrowserGateway,
        SecureSettingsStore secureSettingsStore,
        DocumentReadService documentReadService)
    {
        _mongoDatabaseFactory = mongoDatabaseFactory;
        _seedDataService = seedDataService;
        _fileBrowserGateway = fileBrowserGateway;
        _secureSettingsStore = secureSettingsStore;
        _documentReadService = documentReadService;
    }

    public async Task<ApiOperationResult<InsuranceRecord>> SaveInsuranceRecordAsync(InsuranceRecord record, CancellationToken cancellationToken = default)
    {
        record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
        record.LastUpdatedAt = DateTime.UtcNow;
        await SaveEntityAsync(DocumentCollections.InsuranceRecords, record, item => item.Id == record.Id, cancellationToken);
        return new(ApiOperationResult.Success("upsert-insurance-document", "保险资料已保存。", targetType: "insurance", targetId: record.Id), record);
    }

    public async Task<ApiOperationResult<ManualRecord>> SaveManualRecordAsync(ManualRecord record, CancellationToken cancellationToken = default)
    {
        record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
        record.LastUpdatedAt = DateTime.UtcNow;
        await SaveEntityAsync(DocumentCollections.ManualRecords, record, item => item.Id == record.Id, cancellationToken);
        return new(ApiOperationResult.Success("upsert-manual-document", "说明书资料已保存。", targetType: "manual", targetId: record.Id), record);
    }

    public async Task<ApiOperationResult> DeleteInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _documentReadService.GetInsuranceRecordAsync(id, cancellationToken);
        if (record?.PrimaryFile?.ExternalPath is { Length: > 0 } filePath)
        {
            var options = _secureSettingsStore.GetEffectiveStorageOptions();
            await _fileBrowserGateway.DeleteFileAsync(options.FileServiceBaseUrl, options.FileServiceApiPath, filePath, cancellationToken);
        }

        var removed = await DeleteEntityAsync<InsuranceRecord>(DocumentCollections.InsuranceRecords, id, item => item.Id == id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-insurance-document", "保险资料已删除。", targetType: "insurance", targetId: id)
            : ApiOperationResult.Failure("delete-insurance-document", "未找到要删除的保险资料。", "not_found", targetType: "insurance", targetId: id);
    }

    public async Task<ApiOperationResult> DeleteManualRecordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _documentReadService.GetManualRecordAsync(id, cancellationToken);
        if (record?.PrimaryFile?.ExternalPath is { Length: > 0 } filePath)
        {
            var options = _secureSettingsStore.GetEffectiveStorageOptions();
            await _fileBrowserGateway.DeleteFileAsync(options.FileServiceBaseUrl, options.FileServiceApiPath, filePath, cancellationToken);
        }

        var removed = await DeleteEntityAsync<ManualRecord>(DocumentCollections.ManualRecords, id, item => item.Id == id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-manual-document", "说明书资料已删除。", targetType: "manual", targetId: id)
            : ApiOperationResult.Failure("delete-manual-document", "未找到要删除的说明书资料。", "not_found", targetType: "manual", targetId: id);
    }

    public async Task<ApiOperationResult<InsuranceRecord>> UploadInsuranceFileAsync(Guid documentId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var record = await _documentReadService.GetInsuranceRecordAsync(documentId, cancellationToken);
        if (record is null)
        {
            return new(ApiOperationResult.Failure("upload-insurance-file", "未找到保险资料。", "not_found", targetType: "insurance", targetId: documentId), new InsuranceRecord { Id = documentId });
        }

        var upload = await UploadFileAsync($"insurance/{documentId}", content, fileName, contentType, cancellationToken);
        FileSyncStateMapper.ApplyUploadResult(record, upload);
        record.LastUpdatedAt = DateTime.UtcNow;
        await SaveEntityAsync(DocumentCollections.InsuranceRecords, record, item => item.Id == record.Id, cancellationToken);

        var result = upload.IsSuccess
            ? ApiOperationResult.Success("upload-insurance-file", upload.Message, targetType: "insurance", targetId: documentId)
            : ApiOperationResult.Failure("upload-insurance-file", upload.Message, "upload_failed", targetType: "insurance", targetId: documentId);
        return new(result, record);
    }

    public async Task<ApiOperationResult<ManualRecord>> UploadManualFileAsync(Guid documentId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var record = await _documentReadService.GetManualRecordAsync(documentId, cancellationToken);
        if (record is null)
        {
            return new(ApiOperationResult.Failure("upload-manual-file", "未找到说明书资料。", "not_found", targetType: "manual", targetId: documentId), new ManualRecord { Id = documentId });
        }

        var upload = await UploadFileAsync($"manuals/{documentId}", content, fileName, contentType, cancellationToken);
        FileSyncStateMapper.ApplyUploadResult(record, upload);
        record.LastUpdatedAt = DateTime.UtcNow;
        await SaveEntityAsync(DocumentCollections.ManualRecords, record, item => item.Id == record.Id, cancellationToken);

        var result = upload.IsSuccess
            ? ApiOperationResult.Success("upload-manual-file", upload.Message, targetType: "manual", targetId: documentId)
            : ApiOperationResult.Failure("upload-manual-file", upload.Message, "upload_failed", targetType: "manual", targetId: documentId);
        return new(result, record);
    }

    public async Task<FileBrowserDownloadResult> DownloadInsuranceFileAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var record = await _documentReadService.GetInsuranceRecordAsync(documentId, cancellationToken);
        return await DownloadFileAsync(record?.PrimaryFile?.ExternalPath, cancellationToken);
    }

    public async Task<FileBrowserDownloadResult> DownloadManualFileAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var record = await _documentReadService.GetManualRecordAsync(documentId, cancellationToken);
        return await DownloadFileAsync(record?.PrimaryFile?.ExternalPath, cancellationToken);
    }

    private async Task<FileStorageResult> UploadFileAsync(string folderPath, Stream content, string fileName, string? contentType, CancellationToken cancellationToken)
    {
        var options = _secureSettingsStore.GetEffectiveStorageOptions();
        var upload = await _fileBrowserGateway.UploadFileAsync(options.FileServiceBaseUrl, options.FileServiceApiPath, folderPath, fileName, content, contentType, cancellationToken);
        return new FileStorageResult(
            upload.IsSuccess,
            upload.Message,
            upload.File?.Path,
            upload.File?.Path,
            upload.File?.Name,
            upload.File?.ContentType,
            upload.File?.SizeBytes,
            DateTime.UtcNow,
            upload.IsSuccess ? FileAvailabilityStatus.Available : FileAvailabilityStatus.SyncError);
    }

    private async Task<FileBrowserDownloadResult> DownloadFileAsync(string? remotePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(remotePath))
        {
            return new(false, "当前资料还没有可下载的文件。");
        }

        var options = _secureSettingsStore.GetEffectiveStorageOptions();
        return await _fileBrowserGateway.DownloadFileAsync(options.FileServiceBaseUrl, options.FileServiceApiPath, remotePath, cancellationToken);
    }

    private async Task SaveEntityAsync<T>(string collectionName, T entity, Func<T, bool> seedPredicate, CancellationToken cancellationToken)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            await database.GetCollection<T>(collectionName)
                .ReplaceOneAsync(Builders<T>.Filter.Eq("Id", entity?.GetType().GetProperty("Id")?.GetValue(entity)), entity, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return;
        }

        switch (entity)
        {
            case InsuranceRecord insurance:
                await _seedDataService.SaveInsuranceRecordAsync(insurance);
                break;
            case ManualRecord manual:
                await _seedDataService.SaveManualRecordAsync(manual);
                break;
        }
    }

    private async Task<bool> DeleteEntityAsync<T>(string collectionName, Guid id, Func<T, bool> seedPredicate, CancellationToken cancellationToken)
    {
        var database = await _mongoDatabaseFactory.GetDatabaseAsync(cancellationToken);
        if (database is not null)
        {
            var result = await database.GetCollection<T>(collectionName).DeleteOneAsync(Builders<T>.Filter.Eq("Id", id), cancellationToken);
            return result.DeletedCount > 0;
        }

        return typeof(T) == typeof(InsuranceRecord)
            ? await _seedDataService.DeleteInsuranceRecordAsync(id)
            : await _seedDataService.DeleteManualRecordAsync(id);
    }
}