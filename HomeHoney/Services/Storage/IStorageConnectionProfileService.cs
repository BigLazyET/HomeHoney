using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public sealed record StorageValidationResult(bool IsValid, string Message);

public sealed record StorageProfileSaveResult(bool IsSuccess, StorageConnectionProfile Profile, string Message);

public interface IStorageConnectionProfileService
{
    Task<StorageConnectionProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default);

    Task<string?> GetMongoConnectionStringAsync(CancellationToken cancellationToken = default);

    Task<StorageValidationResult> ValidateProfileAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default);

    Task<StorageProfileSaveResult> SaveProfileAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default);
}
