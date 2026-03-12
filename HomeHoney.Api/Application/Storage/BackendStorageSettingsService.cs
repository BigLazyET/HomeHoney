using HomeHoney.Api.Contracts.Common;
using HomeHoney.Api.Contracts.Storage;
using HomeHoney.Api.Infrastructure.Configuration;
using MongoDB.Driver;

namespace HomeHoney.Api.Application.Storage;

public sealed class BackendStorageSettingsService : IBackendStorageSettingsService
{
    private readonly SecureSettingsStore _secureSettingsStore;
    private readonly StorageValidationService _storageValidationService;
    private readonly BackendProfileService _backendProfileService;

    public BackendStorageSettingsService(SecureSettingsStore secureSettingsStore, StorageValidationService storageValidationService, BackendProfileService backendProfileService)
    {
        _secureSettingsStore = secureSettingsStore;
        _storageValidationService = storageValidationService;
        _backendProfileService = backendProfileService;
    }

    public Task<BackendServiceProfileDto> GetBackendProfileAsync(CancellationToken cancellationToken = default)
        => _backendProfileService.GetAsync(cancellationToken);

    public async Task<ApiOperationResult<BackendServiceProfileDto>> SaveBackendProfileAsync(UpdateBackendServiceProfileRequest request, CancellationToken cancellationToken = default)
        => await _backendProfileService.SaveAsync(request, cancellationToken);

    public Task<DownstreamStorageSettingsDto> GetDownstreamSettingsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_secureSettingsStore.GetDownstreamSettings());

    public async Task<ApiOperationResult<DownstreamStorageSettingsDto>> SaveDownstreamSettingsAsync(UpdateDownstreamStorageSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var validationMessage = await _storageValidationService.ValidateAsync(request, cancellationToken);
        if (validationMessage is not null)
        {
            var invalid = _secureSettingsStore.GetDownstreamSettings();
            invalid.ApplyValidation(ValidationStatusDto.Invalid, validationMessage);
            _secureSettingsStore.SaveDownstreamValidation(invalid.ValidationStatus, invalid.ValidationMessage, DateTime.UtcNow);
            return new(ApiOperationResult.Failure("save-downstream-settings", validationMessage, "invalid_downstream_settings"), invalid);
        }

        var saved = _secureSettingsStore.SaveDownstreamSettings(request);
        saved.ApplyValidation(ValidationStatusDto.Valid, "下游存储配置已保存并验证通过。", DateTime.UtcNow);
        _secureSettingsStore.SaveDownstreamValidation(saved.ValidationStatus, saved.ValidationMessage, saved.LastValidatedAt ?? DateTime.UtcNow);
        return new(ApiOperationResult.Success("save-downstream-settings", saved.ValidationMessage ?? "下游存储配置已保存并验证通过。"), saved);
    }
}
