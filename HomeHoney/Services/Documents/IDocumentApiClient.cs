using HomeHoney.Models;
using HomeHoney.Services.Storage;

namespace HomeHoney.Services.Documents;

public interface IDocumentApiClient
{
	Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync(CancellationToken cancellationToken = default);

	Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync(CancellationToken cancellationToken = default);

	Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default);

	Task<ManualRecord?> GetManualRecordAsync(Guid id, CancellationToken cancellationToken = default);

	Task<InsuranceRecord> SaveInsuranceRecordAsync(InsuranceRecord record, CancellationToken cancellationToken = default);

	Task<ManualRecord> SaveManualRecordAsync(ManualRecord record, CancellationToken cancellationToken = default);

	Task<bool> DeleteInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default);

	Task<bool> DeleteManualRecordAsync(Guid id, CancellationToken cancellationToken = default);

	Task<FileStorageResult> UploadInsuranceFileAsync(Guid id, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default);

	Task<FileStorageResult> UploadManualFileAsync(Guid id, Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default);

	Task<FileDownloadResult> DownloadInsuranceFileAsync(Guid id, CancellationToken cancellationToken = default);

	Task<FileDownloadResult> DownloadManualFileAsync(Guid id, CancellationToken cancellationToken = default);
}
