using HomeHoney.Api.Contracts.Storage;

namespace HomeHoney.Api.Application.Storage;

public static class StorageAdminMapper
{
    public static DownstreamStorageSettingsDto ApplyValidation(this DownstreamStorageSettingsDto settings, ValidationStatusDto status, string message, DateTime? validatedAt = null)
    {
        settings.ValidationStatus = status;
        settings.ValidationMessage = message;
        settings.LastValidatedAt = validatedAt ?? settings.LastValidatedAt;
        return settings;
    }
}