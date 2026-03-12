using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Documents;
using HomeHoney.Api.Infrastructure.FileBrowser;
using HomeHoney.Api.Infrastructure.Mongo.Repositories;
using HomeHoney.Models;

namespace HomeHoney.Api.Application.Documents;

public sealed class InsuranceDocumentService
{
    private readonly DocumentRepository _documentRepository;
    private readonly DocumentFileOrchestrator _documentFileOrchestrator;

    public InsuranceDocumentService(DocumentRepository documentRepository, DocumentFileOrchestrator documentFileOrchestrator)
    {
        _documentRepository = documentRepository;
        _documentFileOrchestrator = documentFileOrchestrator;
    }

    public async Task<IReadOnlyList<InsuranceRecordDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _documentRepository.GetInsuranceRecordsAsync(cancellationToken))
            .Select(FileStateEvaluator.ApplyState)
            .Select(EnsureAttachmentMetadata)
            .Select(DocumentDtoMapper.ToDto)
            .ToList();

    public async Task<InsuranceRecordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetInsuranceRecordAsync(id, cancellationToken);
        return record is null ? null : DocumentDtoMapper.ToDto(EnsureAttachmentMetadata(FileStateEvaluator.ApplyState(record)));
    }

    public async Task<ApiOperationResult<InsuranceRecordDto>> SaveAsync(UpsertInsuranceDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var record = DocumentDtoMapper.ToModel(request);
        record.Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id;
        record.LastUpdatedAt = DateTime.UtcNow;
        FileStateEvaluator.ApplyState(record);
        await _documentRepository.SaveInsuranceRecordAsync(record, cancellationToken);
        return new(ApiOperationResult.Success("upsert-insurance-document", "保险资料已保存。", targetType: "insurance", targetId: record.Id), DocumentDtoMapper.ToDto(record));
    }

    public async Task<ApiOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetInsuranceRecordAsync(id, cancellationToken);
        await _documentFileOrchestrator.DeletePrimaryFileAsync(record?.PrimaryFile?.ExternalPath, cancellationToken);

        var removed = await _documentRepository.DeleteInsuranceRecordAsync(id, cancellationToken);
        return removed
            ? ApiOperationResult.Success("delete-insurance-document", "保险资料已删除。", targetType: "insurance", targetId: id)
            : ApiOperationResult.Failure("delete-insurance-document", "未找到要删除的保险资料。", "not_found", targetType: "insurance", targetId: id);
    }

    public async Task<ApiOperationResult<InsuranceRecordDto>> UploadFileAsync(Guid documentId, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetInsuranceRecordAsync(documentId, cancellationToken);
        if (record is null)
        {
            return new(ApiOperationResult.Failure("upload-insurance-file", "未找到保险资料。", "not_found", targetType: "insurance", targetId: documentId), new InsuranceRecordDto { Id = documentId });
        }

        var result = await _documentFileOrchestrator.UploadPrimaryFileAsync($"insurance/{documentId}", content, fileName, contentType, cancellationToken);
        FileStateEvaluator.ApplyUploadResult(record, result);
        record.LastUpdatedAt = DateTime.UtcNow;
        await _documentRepository.SaveInsuranceRecordAsync(record, cancellationToken);

        var operation = result.IsSuccess
            ? ApiOperationResult.Success("upload-insurance-file", result.Message, targetType: "insurance", targetId: documentId)
            : ApiOperationResult.Failure("upload-insurance-file", result.Message, "upload_failed", targetType: "insurance", targetId: documentId);
        return new(operation, DocumentDtoMapper.ToDto(EnsureAttachmentMetadata(record)));
    }

    public async Task<FileBrowserDownloadResult> DownloadFileAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var record = await _documentRepository.GetInsuranceRecordAsync(documentId, cancellationToken);
        return await _documentFileOrchestrator.DownloadPrimaryFileAsync(record?.PrimaryFile?.ExternalPath, cancellationToken);
    }

    private static InsuranceRecord EnsureAttachmentMetadata(InsuranceRecord record)
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
