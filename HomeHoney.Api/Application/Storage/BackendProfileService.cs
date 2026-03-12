using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Storage;
using HomeHoney.Api.Infrastructure.Configuration;

namespace HomeHoney.Api.Application.Storage;

public sealed class BackendProfileService
{
    private readonly SecureSettingsStore _secureSettingsStore;

    public BackendProfileService(SecureSettingsStore secureSettingsStore)
    {
        _secureSettingsStore = secureSettingsStore;
    }

    public Task<BackendServiceProfileDto> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_secureSettingsStore.GetBackendProfile());
    }

    public Task<ApiOperationResult<BackendServiceProfileDto>> SaveAsync(UpdateBackendServiceProfileRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Uri.TryCreate(request.ApiBaseUrl, UriKind.Absolute, out _))
        {
            var invalid = _secureSettingsStore.GetBackendProfile();
            invalid.ValidationStatus = ValidationStatusDto.Invalid;
            invalid.ValidationMessage = "后端 API 地址不是有效的绝对地址。";
            return Task.FromResult(new ApiOperationResult<BackendServiceProfileDto>(ApiOperationResult.Failure("save-backend-profile", invalid.ValidationMessage, "invalid_backend_url"), invalid));
        }

        var updated = _secureSettingsStore.SaveBackendProfile(request.DisplayName, request.ApiBaseUrl);
        updated.LastValidatedAt = DateTime.UtcNow;
        updated.ValidationStatus = ValidationStatusDto.Valid;
        updated.ValidationMessage = "后端连接配置已保存。";
        _secureSettingsStore.SaveBackendValidation(updated.ValidationStatus, updated.ValidationMessage, updated.LastValidatedAt.Value);
        return Task.FromResult(new ApiOperationResult<BackendServiceProfileDto>(ApiOperationResult.Success("save-backend-profile", updated.ValidationMessage), updated));
    }
}