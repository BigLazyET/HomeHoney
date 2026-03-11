using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public interface IDocumentMetadataRepository
{
    Task<IReadOnlyList<InsuranceRecord>> GetInsuranceRecordsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ManualRecord>> GetManualRecordsAsync(CancellationToken cancellationToken = default);

    Task<InsuranceRecord?> GetInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ManualRecord?> GetManualRecordAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveInsuranceRecordAsync(InsuranceRecord record, CancellationToken cancellationToken = default);

    Task SaveManualRecordAsync(ManualRecord record, CancellationToken cancellationToken = default);

    Task<bool> DeleteInsuranceRecordAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteManualRecordAsync(Guid id, CancellationToken cancellationToken = default);
}
