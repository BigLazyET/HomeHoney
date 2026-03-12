using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Storage;

namespace HomeHoney.Api.Application.Storage;

public interface IBackendStorageSettingsService
{
    Task<BackendServiceProfileDto> GetBackendProfileAsync(CancellationToken cancellationToken = default);

    Task<ApiOperationResult<BackendServiceProfileDto>> SaveBackendProfileAsync(UpdateBackendServiceProfileRequest request, CancellationToken cancellationToken = default);

    Task<DownstreamStorageSettingsDto> GetDownstreamSettingsAsync(CancellationToken cancellationToken = default);

    Task<ApiOperationResult<DownstreamStorageSettingsDto>> SaveDownstreamSettingsAsync(UpdateDownstreamStorageSettingsRequest request, CancellationToken cancellationToken = default);
}
