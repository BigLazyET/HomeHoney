using HomeHoney.Models;

namespace HomeHoney.Services.Storage;

public interface IAdminStorageApiClient
{
	Task<StorageConnectionProfile> GetProfileAsync(string? backendBaseUrl = null, CancellationToken cancellationToken = default);

	Task<StorageProfileSaveResult> SaveProfileAsync(StorageConnectionProfile profile, string mongoConnectionString, CancellationToken cancellationToken = default);
}
