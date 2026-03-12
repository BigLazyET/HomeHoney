using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Documents;
using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;
using HomeHoney.Models;

namespace HomeHoney.Api.Application.Documents;

public sealed class ManualDocumentService
{
    private readonly DocumentRepository _documentRepository;
    private readonly DocumentFileOrchestrator _documentFileOrchestrator;

    public ManualDocumentService(DocumentRepository documentRepository, DocumentFileOrchestrator documentFileOrchestrator)
    {
        _documentRepository = documentRepository;
        _documentFileOrchestrator = documentFileOrchestrator;
    }

    public async Task<IReadOnlyList<ManualRecordDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _documentRepository.GetManualRecordsAsync(cancellationToken))
            .Select(FileStateEvaluator.ApplyState)
            .Select(EnsureAttachmentMetadata)
            .Select(DocumentDtoMapper.ToDto)
            .ToList();

    public async Task<ManualRecordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetManualRecordAsync(id, cancellationToken);
        return record is null ? null : DocumentDtoMapper.ToDto(EnsureAttachmentMetadata(FileStateEvaluator.ApplyState(record)));
    }

    public async Task<ApiOperationResult<ManualRecordDto>> SaveAsync(UpsertManualDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var record = DocumentDtoMapper.ToModel(request);
        record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
        record.LastUpdatedAt = DateTime.UtcNow;
        FileStateEvaluator.ApplyState(record);
        await _documentRepository.SaveManualRecordAsync(record, cancellationToken);
        return new(ApiOperationResult.Success("upsert-manual-document", "说明书资料已保存。", targetType: "manual", targetId: record.Id), DocumentDtoMapper.ToDto(record));
    }

    public async Task<ApiOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetManualRecordAsync(id, cancellationToken);
        await _documentFileOrchestrator.DeletePrimaryFileAsync(record?.PrimaryFile?.ExternalPath, cancellationToken);

        var removed = await _documentRepository.DeleteManualRecordAsync(id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-manual-document", "说明书资料已删除。", targetType: "manual", targetId: id)
            : ApiOperationResult.Failure("delete-manual-document", "未找到要删除的说明书资料。", "not_found", targetType: "manual", targetId: id);
    }

    public async Task<ApiOperationResult<ManualRecordDto>> UploadFileAsync(Guid documentId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetManualRecordAsync(documentId, cancellationToken);
        if (record is null)
        {
            return new(ApiOperationResult.Failure("upload-manual-file", "未找到说明书资料。", "not_found", targetType: "manual", targetId: documentId), new ManualRecordDto { Id = documentId });
        }

        var result = await _documentFileOrchestrator.UploadPrimaryFileAsync($"manuals/{documentId}", content, fileName, contentType, cancellationToken);
        FileStateEvaluator.ApplyUploadResult(record, result);
        record.LastUpdatedAt = DateTime.UtcNow;
        await _documentRepository.SaveManualRecordAsync(record, cancellationToken);

        var operation = result.IsSuccess
            ? ApiOperationResult.Success("upload-manual-file", result.Message, targetType: "manual", targetId: documentId)
            : ApiOperationResult.Failure("upload-manual-file", result.Message, "upload_failed", targetType: "manual", targetId: documentId);
        return new(operation, DocumentDtoMapper.ToDto(EnsureAttachmentMetadata(record)));
    }

    public async Task<FileBrowserDownloadResult> DownloadFileAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetManualRecordAsync(documentId, cancellationToken);
        return await _documentFileOrchestrator.DownloadPrimaryFileAsync(record?.PrimaryFile?.ExternalPath, cancellationToken);
    }

    private static ManualRecord EnsureAttachmentMetadata(ManualRecord record)
    {
        if (record.PrimaryFile is null)
        {
            return record;
        }

        record.PrimaryFile.FileName = string.IsNullOrWhiteSpace(record.PrimaryFile.FileName) ? "附件信息待补充" : record.PrimaryFile.FileName;
        record.PrimaryFile.LastSyncedAt ??= record.LastUpdatedAt == default ? DateTime.UtcNow : record.LastUpdatedAt;
        record.PrimaryFile.StatusMessage ??= record.SyncMessage;
        return record;
    }
}
